using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vence.Input4Edicao.Models;

namespace Vence.Input4Edicao.Controllers
{
    public class RelatParcelaFinalController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public FileResult GerarRelatorio(string nomeRelatorio, string numeroAES, int itemAES, string mesRef = "", string PFinal = "N", int codigoMantida = 0)
        {
            try
            {
                Report relatorio = new Report();
                byte[] file = null;
                var parametros = new List<ReportParameter>();

                switch (nomeRelatorio)
                {
                    case "RelatorioFisicoFinaceiroParcelaFinal":
                        {
                            parametros.Add(new ReportParameter { Name = "Numero_AES", Value = numeroAES });
                            parametros.Add(new ReportParameter { Name = "Item_AES", Value = itemAES.ToString() });
                            parametros.Add(new ReportParameter { Name = "Cod_Mantida", Value = codigoMantida.ToString() });
                            file = relatorio.GetReportFile(nomeRelatorio, "/Vence/Financeiro", ReportFormat.PDF, parametros);
                            break;
                        }
                    case "RelatorioFisicoFinaceiroParcelaMensal":
                        {
                            parametros.Add(new ReportParameter { Name = "Numero_AES", Value = numeroAES });
                            parametros.Add(new ReportParameter { Name = "Item_AES", Value = itemAES.ToString() });
                            parametros.Add(new ReportParameter { Name = "mes_ref", Value = mesRef });
                            file = relatorio.GetReportFile(nomeRelatorio, "/Vence/Financeiro", ReportFormat.PDF, parametros);
                            break;
                        }
                    case "SumarioFisicoFinanceirodaParcela":
                        {
                            parametros.Add(new ReportParameter { Name = "Numero_AES", Value = numeroAES });
                            parametros.Add(new ReportParameter { Name = "Item_AES", Value = itemAES.ToString() });
                            parametros.Add(new ReportParameter { Name = "Cod_Mantida", Value = codigoMantida.ToString() });
                            parametros.Add(new ReportParameter { Name = "mes_ref", Value = mesRef });
                            file = relatorio.GetReportFile(nomeRelatorio, "/Vence/Financeiro", ReportFormat.PDF, parametros);
                            break;
                        }
                    case "RelatorioAcompFreqDiariaAlunos":
                        {
                            parametros.Add(new ReportParameter { Name = "Numero_AES", Value = numeroAES });
                            parametros.Add(new ReportParameter { Name = "Item_AES", Value = itemAES.ToString() });
                            parametros.Add(new ReportParameter { Name = "CodMantida", Value = codigoMantida.ToString() });
                            parametros.Add(new ReportParameter { Name = "mes_ref", Value = mesRef });
                            parametros.Add(new ReportParameter { Name = "PFinal", Value = PFinal });
                            file = relatorio.GetReportFile(nomeRelatorio, "/Vence/Frequencia", ReportFormat.PDF, parametros);
                            break;
                        }
                }

                return File(file, ReportFormat.PDF.GetEnumDescription(), nomeRelatorio + ".pdf");
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public PartialViewResult Pesquisar(string chave, bool somentePlanoContigencia)
        {
            var codDe = this.ObterDeApartirDoToken(chave);
            var model = new RelatParcelaFinalVM();
            model.ListaAES = this.ObterAES(codDe, somentePlanoContigencia);
            return PartialView("_PesquisaAESGridView", model);
        }

        private int ObterDeApartirDoToken(string chave)
        {
            DbHelper db = new DbHelper();
            db.AddParameter(new System.Data.SqlClient.SqlParameter("@chave", chave));
            string cmdText = "select cd_de from [dbo].[DiretoriaTokenContigencia] where token = @chave";
            int codDe = db.GetExecuteScalar<int>(cmdText);
            db.CloseDbConnection();
            return codDe;
        }

        private List<DadosAES> ObterAES(int codDe, bool somentePlanoContigencia)
        {
            var lista = new List<DadosAES>();
            DbHelper db = new DbHelper();

            db.AddParameter(new System.Data.SqlClient.SqlParameter("@codDe", codDe));

            string cmdText = string.Format(@"SELECT wmc.Cod_DE,
			                                        mant.edicao,      
			                                        mant.NM_Credenciamento,      
			                                        mant.Numero_AES,      
			                                        mant.Mantida,        
			                                        dt.Diretoria,      
			                                        mant.Item_AES,
			                                        mant.Cod_Mantida,
			                                        format(parc.mesInicial,'MM/yyyy') as mes_ref,
			                                        parc.tipoparcela,
                                                    CASE parc.tipoparcela
			                                          WHEN 'F' THEN 'SIM' ELSE 'NÃO'
			                                        END AS PARCELA_FINAL,
                                                    parc.staPLCV
	                                          from [dbo].vw_mantida           mant,      
		                                           [VENCE].[dbo].[MtdParcela] parc,      
		                                           [dbo].[vw_mantida_curso]   wmc,
		                                           DiretoriaTokenContigencia  dt 
	                                         where {0}
                                               and wmc.cod_de         = @codDe
	                                           and wmc.Cod_DE         = dt.CD_DE 
	                                           and wmc.Cod_Mantida    = mant.Cod_Mantida
	                                           and mant.idContratoMtd = PARC.idContratoMtd
                                          group by wmc.Cod_DE,
	                                               mant.edicao,      
		                                           mant.NM_Credenciamento,      
		                                           mant.Numero_AES,      
		                                           mant.Mantida,        
		                                           dt.Diretoria,      
		                                           mant.Item_AES,
		                                           mant.Cod_Mantida,
                                                   parc.staPLCV,
		                                           parc.mesInicial,
                                                   parc.tipoparcela
                                          order by mant.Cod_Mantida,
	  	                                           mant.Item_AES,
		                                           parc.mesInicial", (somentePlanoContigencia == true ? "parc.staPLCV is not null" : "1=1"));

            SqlDataReader dr = db.GetDataReader(cmdText);

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    lista.Add(new DadosAES
                    {
                        CodMantida = Convert.ToInt32(dr["Cod_Mantida"]),
                        Mantida = dr["Mantida"].ToString(),
                        CodDE = dr["Cod_DE"] != DBNull.Value ? Convert.ToInt32(dr["Cod_DE"]) : 0,
                        NumeroAES = dr["Numero_AES"].ToString(),
                        Diretoria = dr["Diretoria"].ToString(),
                        ItemAES = Convert.ToInt32(dr["Item_AES"]),
                        MesReferencia = dr["mes_ref"].ToString(),
                        TipoParcela = dr["tipoparcela"].ToString(),
                        EhParcelaFinal = dr["PARCELA_FINAL"].ToString(),
                        staPLCV = dr["Cod_DE"] != DBNull.Value ? dr["staPLCV"].ToString() : ""
                    });
                }
            }

            db.CloseDbConnection();

            return lista;
        }
    }
}