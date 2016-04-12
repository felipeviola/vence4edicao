using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using Vence.Input4Edicao.Models;


namespace Vence.Input4Edicao.Controllers
{
    public class RelatAcompMensalController : Controller
    {
        public ActionResult Index()
        {
            RelatAcompMensalVM model = new RelatAcompMensalVM();
            model.ItemAES = new List<int>();

            return View("Index", model);
        }

        public FileResult GerarRelatorio(string numeroAES, int itemAES, string mesReferencia)
        {
            try
            {
                var parametros = new List<ReportParameter>();
                parametros.Add(new ReportParameter { Name = "Numero_AES", Value = numeroAES });
                parametros.Add(new ReportParameter { Name = "Item_AES", Value = itemAES.ToString() });
                parametros.Add(new ReportParameter { Name = "mes_ref", Value = mesReferencia });
                parametros.Add(new ReportParameter { Name = "PFinal", Value = "N" });
                parametros.Add(new ReportParameter { Name = "CodMantida", Value = "0" });

                Report relatorio = new Report();
                byte[] file = relatorio.GetReportFile("RelatAcompMensal", "/Vence", ReportFormat.PDF, parametros);
                return File(file, ReportFormat.PDF.GetEnumDescription(), "RelatAcompMensal.pdf");
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public PartialViewResult Pesquisar(string numeroAES, int itemAES)
        {
            var model = new RelatAcompMensalVM();
            model.ListaAcompanhamentos = ObterListagemObjetos(numeroAES, itemAES);
            return PartialView("_AcompMensalGridView", model);
        }

        public JsonResult PesquisarItemAESPorNumeroAES(string numeroAES)
        {
            var listaAES = this.ObterItemAESPorNumeroAES(numeroAES);
            return Json(listaAES, JsonRequestBehavior.AllowGet);
        }

        public List<DadosAES> ObterItemAESPorNumeroAES(string numeroAES)
        {
            List<DadosAES> listaAES = new List<DadosAES>();

            DbHelper db = new DbHelper();

            string cmdText = @"select b.Numero_AES, b.Item_AES
	                             from Frequencia4Edicao a, [dbo].[vw_mantida_curso_turma]  b
	                            where 1=1
	                              and a.idCursoTurnoTurma = b.idCursoTurnoTurma 
	                              and b.Numero_AES = @Numero_AES
                             group by b.Numero_AES, b.Item_AES 
                             order by 2";

            db.AddParameter(new System.Data.SqlClient.SqlParameter("@Numero_AES", numeroAES));

            SqlDataReader dr = db.GetDataReader(cmdText);

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    listaAES.Add(new DadosAES
                    {
                        NumeroAES = dr["Numero_AES"].ToString(),
                        ItemAES = Convert.ToInt32(dr["Item_AES"])
                    });

                }
            }

            db.CloseDbConnection();

            return listaAES;
        }

        public List<VwAcompMensal> ObterListagemObjetos(string numeroAES, int itemAES)
        {
            var acompList = new List<VwAcompMensal>();
            DbHelper db = new DbHelper();

            db.AddParameter(new System.Data.SqlClient.SqlParameter("@numeroAES", numeroAES));
            db.AddParameter(new System.Data.SqlClient.SqlParameter("@itemAES", itemAES));

            string cmdText = @"select b.Numero_AES, b.Item_AES, count(distinct idMatricula) qtdAlunos, a.MesReferencia
                                 from Frequencia4Edicao a, [dbo].[vw_mantida_curso_turma]  b
                                where 1=1
                                  and a.idCursoTurnoTurma = b.idCursoTurnoTurma 
                                  and b.Numero_AES = @numeroAES
	                              and b.Item_AES   = @itemAES
                                  --and staPLCV = 'P'    
                             group by b.Numero_AES, b.Item_AES, a.MesReferencia
                             order by 1,2";

            SqlDataReader dr = db.GetDataReader(cmdText);

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    acompList.Add(new VwAcompMensal
                    {
                        NumeroAES = dr["Numero_AES"].ToString(),
                        ItemAES = Convert.ToInt32(dr["Item_AES"]),
                        QtdAlunos = Convert.ToInt32(dr["qtdAlunos"]),
                        MesReferencia = dr["MesReferencia"].ToString()
                    });
                }
            }

            db.CloseDbConnection();

            return acompList;
        }
    }
}