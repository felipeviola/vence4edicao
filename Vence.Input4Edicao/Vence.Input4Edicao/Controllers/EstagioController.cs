using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Data;
using Vence.Input4Edicao.Models;
using System.IO;
using System.ComponentModel;
using System.Reflection;
using Vence.Input4Edicao.Models;


namespace Vence.Input4Edicao.Controllers
{
    public class EstagioController : Controller
    {
        public ActionResult Index()
        {
            ControleEstagioVM model = new ControleEstagioVM();
            model.ItemAES = new List<int>();

            return View("Index", model);
        }

        public FileResult GerarRelatorio(string numeroAES, int itemAES)
        {
            try
            {
                Report relatorio = new Report();
                var parametros = new List<ReportParameter>();
                parametros.Add(new ReportParameter { Name = "Numero_AES", Value = numeroAES });
                parametros.Add(new ReportParameter { Name = "Item_AES", Value = itemAES.ToString() });
                byte[] file = relatorio.GetReportFile("RelatorioAcompanhamentoEstagio", "/Vence/Estagio", ReportFormat.PDF, parametros);
                return File(file, ReportFormat.PDF.GetEnumDescription(), "RelatorioAcompanhamentoEstagio.pdf");
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public FileResult GerarArquivosExcel(string chave)
        {
            DataSet ds = this.ObterDataSetEstagio(chave);
            MemoryStream stream = new MemoryStream();
            ExcelLibrary.DataSetHelper.CreateWorkbook(stream, ds);
            byte[] file = stream.ToArray();
            return File(file, ReportFormat.XLS.GetEnumDescription(), "RelatAcompEstagio.xls");
        }

        public JsonResult PesquisarNumeroAESPorChave(string chave)
        {
            var listaAES = this.ObterNumeroAESPorChave(chave);
            return Json(listaAES, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult PesquisarEstagios(string numeroAES, int itemAES)
        {
            var model = new ControleEstagioVM();
            model.ListaEstagios = ObterListaEstagio(numeroAES, itemAES);
            return PartialView("_PartialViewEstagios", model);
        }

        public PartialViewResult PesquisarEstagiosPorToken(string chave)
        {
            var model = new ControleEstagioVM();
            model.ListaEstagios = ObterListaEstagioPorToken(chave);
            return PartialView("_PartialViewEstagios", model);
        }

        public DataSet ObterDataSetEstagio(string chave)
        {
            DbHelper db = new DbHelper();
            db.AddParameter(new System.Data.SqlClient.SqlParameter("@Chave", chave));

            string cmdText = @" select Numero_AES, Item_AES, Mantenedora, Cod_Mantida, Mantida, Diretoria_Ensino, CodCurso, 
                                       NomCurso, vagas, valorAluno, valor_hora_aula, area, esta.idMatricula, 
                                       esta.RA, esta.NomeAluno, cargaHoraEstagio, sum(qtd_Horas_Estagio) as Total_Horas_Estagio
                                  from vw_estagio esta
                                  join Token      tk on esta.Numero_AES = tk.AES
                                 where tk.Chave = @Chave
                              group by Numero_AES, Item_AES, Mantenedora, Cod_Mantida, Mantida, Diretoria_Ensino, CodCurso, NomCurso, 
                                       vagas, valorAluno, valor_hora_aula, area, idMatricula, RA, NomeAluno, cargaHoraEstagio";

            DataSet ds = db.GetDataSet(cmdText);
            db.CloseDbConnection();
            return ds;
        }

        public List<DadosAES> ObterNumeroAESPorChave(string chave)
        {
            List<DadosAES> listaAES = new List<DadosAES>();

            DbHelper db = new DbHelper();

            string cmdText = @"select esta.Numero_AES, esta.Item_AES
                                 from vw_estagio esta
                                 join Token tk on esta.Numero_AES = tk.AES
                                where tk.Chave = @Chave	
                             group by esta.Numero_AES, esta.Item_AES";

            db.AddParameter(new System.Data.SqlClient.SqlParameter("@Chave", chave));

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

        public List<VwEstagio> ObterListaEstagioPorToken(string chave)
        {
            var estagios = new List<VwEstagio>();
            DbHelper db = new DbHelper();

            string cmdText = @" select Numero_AES, Item_AES, Mantenedora, Cod_Mantida, Mantida, Diretoria_Ensino, CodCurso, 
                                       NomCurso, vagas, valorAluno, valor_hora_aula, area, esta.idMatricula, 
                                       esta.RA, esta.NomeAluno, cargaHoraEstagio, sum(qtd_Horas_Estagio) as Total_Horas_Estagio
                                  from vw_estagio esta
                                  join Token      tk on esta.Numero_AES = tk.AES
                                 where tk.Chave = @Chave";

            db.AddParameter(new System.Data.SqlClient.SqlParameter("@Chave", chave));

            cmdText += @" group by Numero_AES, Item_AES, Mantenedora, Cod_Mantida, Mantida, Diretoria_Ensino, CodCurso, NomCurso, 
                                   vagas, valorAluno, valor_hora_aula, area, idMatricula, RA, NomeAluno, cargaHoraEstagio";

            SqlDataReader dr = db.GetDataReader(cmdText);

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    estagios.Add(new VwEstagio
                    {
                        NumeroAES = dr["Numero_AES"].ToString(),
                        ItemAES = Convert.ToInt32(dr["Item_AES"]),
                        NomeMantenedora = dr["Mantenedora"].ToString(),
                        IdMantida = Convert.ToInt32(dr["Cod_Mantida"]),
                        NomeMantida = dr["Mantida"].ToString(),
                        NomeDiretoria = dr["Diretoria_Ensino"].ToString(),
                        IdCurso = Convert.ToInt32(dr["CodCurso"]),
                        NomeCurso = dr["NomCurso"].ToString(),
                        NumeroVagas = Convert.ToInt32(dr["vagas"]),
                        ValorAluno = Convert.ToDecimal(dr["valorAluno"]),
                        ValorHoraAula = Convert.ToDecimal(dr["valor_hora_aula"]),
                        Area = dr["area"].ToString(),
                        IdMatricula = Convert.ToInt32(dr["idMatricula"]),
                        NumeroRA = dr["RA"].ToString(),
                        NomeAluno = dr["NomeAluno"].ToString(),
                        CargaHorariaEstagio = Convert.ToInt32(dr["cargaHoraEstagio"]),
                        TotalHoraEstagio = Convert.ToInt32(dr["Total_Horas_Estagio"])
                    });
                }
            }

            db.CloseDbConnection();

            return estagios;
        }

        public List<VwEstagio> ObterListaEstagio(string numeroAES, int itemAES)
        {
            var estagios = new List<VwEstagio>();
            DbHelper db = new DbHelper();

            string cmdText = @" select Numero_AES, Item_AES, Mantenedora, Cod_Mantida, Mantida, Diretoria_Ensino, CodCurso, 
                                       NomCurso, vagas, valorAluno, valor_hora_aula, area, esta.idMatricula, 
                                       esta.RA, esta.NomeAluno, cargaHoraEstagio, sum(qtd_Horas_Estagio) as Total_Horas_Estagio
                                  from vw_estagio esta
                                 where 1=1";

            if (!string.IsNullOrEmpty(numeroAES))
            {
                cmdText += " and Numero_AES = @numeroAES";
                db.AddParameter(new System.Data.SqlClient.SqlParameter("@numeroAES", numeroAES));
            }
            if (itemAES > -1)
            {
                cmdText += " and Item_AES = @itemAES";
                db.AddParameter(new System.Data.SqlClient.SqlParameter("@itemAES", itemAES));
            }

            cmdText += @" group by Numero_AES, Item_AES, Mantenedora, Cod_Mantida, Mantida, Diretoria_Ensino, CodCurso, NomCurso, 
                                   vagas, valorAluno, valor_hora_aula, area, idMatricula, RA, NomeAluno, cargaHoraEstagio";

            SqlDataReader dr = db.GetDataReader(cmdText);

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    estagios.Add(new VwEstagio
                    {
                        NumeroAES = dr["Numero_AES"].ToString(),
                        ItemAES = Convert.ToInt32(dr["Item_AES"]),
                        NomeMantenedora = dr["Mantenedora"].ToString(),
                        IdMantida = Convert.ToInt32(dr["Cod_Mantida"]),
                        NomeMantida = dr["Mantida"].ToString(),
                        NomeDiretoria = dr["Diretoria_Ensino"].ToString(),
                        IdCurso = Convert.ToInt32(dr["CodCurso"]),
                        NomeCurso = dr["NomCurso"].ToString(),
                        NumeroVagas = Convert.ToInt32(dr["vagas"]),
                        ValorAluno = Convert.ToDecimal(dr["valorAluno"]),
                        ValorHoraAula = Convert.ToDecimal(dr["valor_hora_aula"]),
                        Area = dr["area"].ToString(),
                        IdMatricula = Convert.ToInt32(dr["idMatricula"]),
                        NumeroRA = dr["RA"].ToString(),
                        NomeAluno = dr["NomeAluno"].ToString(),
                        CargaHorariaEstagio = Convert.ToInt32(dr["cargaHoraEstagio"]),
                        TotalHoraEstagio = Convert.ToInt32(dr["Total_Horas_Estagio"])
                    });
                }
            }

            db.CloseDbConnection();

            return estagios;
        }
    }
}