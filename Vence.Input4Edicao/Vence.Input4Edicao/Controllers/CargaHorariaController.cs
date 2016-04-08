using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Vence.Input4Edicao.Controllers
{
    public class CargaHorariaController : Controller
    {
        // GET: CargaHoraria
        public ActionResult Index()
        {
            return View();
        }

        public FileResult GerarArquivosExcel(string chave)
        {
            DataSet ds = this.ObterDataSetCargaHoraria(chave);
            MemoryStream stream = new MemoryStream();
            ExcelLibrary.DataSetHelper.CreateWorkbook(stream, ds);
            byte[] file = stream.ToArray();
            return File(file, ReportFormat.XLS.GetEnumDescription(), "ConsultaCargaHoraria.xls");
        }

        public PartialViewResult PesquisarCargaHoraria(string chave)
        {
            var model = new CargaHorariaVM();
            model.ListaCargaHoraria = ObterListaCargaHoraria(chave);
            return PartialView("_CargaHorariaGridView", model);
        }

        public DataSet ObterDataSetCargaHoraria(string chave)
        {
            DbHelper db = new DbHelper();
            db.AddParameter(new System.Data.SqlClient.SqlParameter("@Chave", chave));

//            string cmdText = @" select caho.edicao, 
//                                       caho.Numero_AES, 
//	                                   caho.Item_AES, 
//	                                   caho.Cod_Mantenedora, 
//	                                   caho.Mantenedora, 
//	                                   caho.Cod_Mantida, 
//	                                   caho.Mantida, 
//	                                   caho.Data_Inicio_Aula, 
//	                                   caho.CodCurso, 
//	                                   caho.Curso, 
//	                                   caho.Carga_Hora_Total, 
//	                                   caho.Duracao, 
//	                                   caho.Tot_Carga_Hora_Exec,
//	                                   caho.Duracao_Exec,
//	                                   caho.Carga_Hora_Total - caho.Tot_Carga_Hora_Exec as Saldo,
//	                                   caho.Qtd_Tuma,  
//	                                   caho.Ultimo_Mes_Frequencia, 
//	                                   caho.Integralizado, 
//	                                   count(1) diasLetivos, 
//	                                   sum( cast(cargaHoraria as int)/60) cargaHoraria, 
//	                                   cale.mesReferencia
//                                  from vw_acomp_carga_horaria caho,      
//                                       Calendario4edicao      cale,
//									   Token                    tk
//                                 where caho.idCursoTurnoTurma = cale.idCursoTurnoTurma
//								   and tk.AES = caho.Numero_AES  
//                                   and cale.staPLCV IS NULL
//								   and tk.Chave = @Chave 
//		  					  group by caho.edicao, 
//									   caho.Numero_AES, 
//									   caho.Item_AES, 
//									   caho.Cod_Mantenedora, 
//									   caho.Mantenedora, 
//									   caho.Cod_Mantida, 
//									   caho.Mantida, 
//									   caho.Data_Inicio_Aula, 
//									   caho.CodCurso, 
//									   caho.Curso, 
//									   caho.Carga_Hora_Total, 
//									   caho.Duracao, 
//									   caho.Tot_Carga_Hora_Exec, 
//									   caho.Qtd_Tuma, 
//									   caho.Ultimo_Mes_Frequencia, 
//									   caho.Integralizado, 
//									   cale.mesReferencia, 
//									   caho.Duracao_Exec";
            string sql = @"	  with HorasRealizadas as 
                            (select 
                               Numero_AES, nomeCurso
                        ,      Item_AES
                        ,      idMatricula
                        ,      ra
                        ,      NomeAluno
                        ,      mes_ref
                        ,      Carga_Hora_Total
                        ,      Carga_Mes, idCursoTurnoTurma
                        ,      sum(horas_dia*qtd_presenca) Carga_Mes_Realizada
                        from   [dbo].[vw_frequencia]
                        where  Numero_AES  in ( select AES from Token where chave = @Chave )
                        --and    Item_AES    = 4
                        --and    mes_ref      = '02/2014'
                        --and    idMatricula = 27916
                        group  by
                               Numero_AES
                        ,      Item_AES
                        ,      ra
                        ,      NomeAluno
                        ,      Carga_Hora_Total
                        ,      Carga_Mes
                        ,      mes_ref
                        ,      idMatricula, idCursoTurnoTurma , nomeCurso)

                        select Numero_AES, nomeCurso
                        ,      Item_AES
                        ,      idMatricula
                        ,      ra
                        ,      NomeAluno
                        ,      format(max( cast(concat(substring(mes_ref,1,2),'/01/',substring(mes_ref,4,4)) as date ) ),'MM/yyyy') ult_mes_lanc
                        ,      Carga_Hora_Total



                        ,      (select sum(cutc.cargaHoraria) / 60
                          from   [dbo].[CursoTurnoTurma] cutu
                          ,      [dbo].[CursoTurnoTurmaCalendario] cutc
                          where  cutu.idCursoTurnoTurma = hora.idCursoTurnoTurma
                          and    cutu.idCursoTurnoTurma = cutc.idCursoTurnoTurma ) CargaRealizadaCurso


                        ,      sum(Carga_Mes_Realizada) Carga_Realizada_Aluno


                        from   HorasRealizadas hora

                        group  by 
                               Numero_AES,nomeCurso
                        ,      Item_AES
                        ,      idMatricula
                        ,      ra
                        ,      NomeAluno
                        ,      Carga_Hora_Total
                        ,hora.idCursoTurnoTurma";
            DataSet ds = db.GetDataSet(sql);
            db.CloseDbConnection();
            return ds;
        }

        public List<VwAcompCargaHoraria> ObterListaCargaHoraria(string chave)
        {
            var lista = new List<VwAcompCargaHoraria>();
            DbHelper db = new DbHelper();
            db.AddParameter(new System.Data.SqlClient.SqlParameter("@Chave", chave));

            //            string cmdText = @" select caho.edicao, 
            //                                       caho.Numero_AES, 
            //	                                   caho.Item_AES, 
            //	                                   caho.Cod_Mantenedora, 
            //	                                   caho.Mantenedora, 
            //	                                   caho.Cod_Mantida, 
            //	                                   caho.Mantida, 
            //	                                   caho.Data_Inicio_Aula, 
            //	                                   caho.CodCurso, 
            //	                                   caho.Curso, 
            //	                                   caho.Carga_Hora_Total, 
            //	                                   caho.Duracao, 
            //	                                   caho.Tot_Carga_Hora_Exec,
            //	                                   caho.Duracao_Exec,
            //	                                   caho.Carga_Hora_Total - caho.Tot_Carga_Hora_Exec as Saldo,
            //	                                   caho.Qtd_Tuma,  
            //	                                   caho.Ultimo_Mes_Frequencia, 
            //	                                   caho.Integralizado, 
            //	                                   count(1) diasLetivos, 
            //	                                   sum( cast(cargaHoraria as int)/60) cargaHoraria, 
            //	                                   cale.mesReferencia
            //                                  from vw_acomp_carga_horaria caho,      
            //                                       Calendario4edicao      cale,
            //									   Token                    tk
            //                                 where caho.idCursoTurnoTurma = cale.idCursoTurnoTurma
            //								   and tk.AES = caho.Numero_AES  
            //                                   and cale.staPLCV IS NULL
            //								   and tk.Chave = @Chave 
            //		  					  group by caho.edicao, 
            //									   caho.Numero_AES, 
            //									   caho.Item_AES, 
            //									   caho.Cod_Mantenedora, 
            //									   caho.Mantenedora, 
            //									   caho.Cod_Mantida, 
            //									   caho.Mantida, 
            //									   caho.Data_Inicio_Aula, 
            //									   caho.CodCurso, 
            //									   caho.Curso, 
            //									   caho.Carga_Hora_Total, 
            //									   caho.Duracao, 
            //									   caho.Tot_Carga_Hora_Exec, 
            //									   caho.Qtd_Tuma, 
            //									   caho.Ultimo_Mes_Frequencia, 
            //									   caho.Integralizado, 
            //									   cale.mesReferencia, 
            //									   caho.Duracao_Exec";

            string sql = @"   with HorasRealizadas as 
                            (select 
                               Numero_AES, nomeCurso
                        ,      Item_AES
                        ,      idMatricula
                        ,      ra
                        ,      NomeAluno
                        ,      mes_ref
                        ,      Carga_Hora_Total
                        ,      Carga_Mes, idCursoTurnoTurma
                        ,      sum(horas_dia*qtd_presenca) Carga_Mes_Realizada
                        from   [dbo].[vw_frequencia]
                        where  Numero_AES  in ( select AES from Token where chave = @Chave )
                        --and    Item_AES    = 4
                        --and    mes_ref      = '02/2014'
                        --and    idMatricula = 27916
                        group  by
                               Numero_AES
                        ,      Item_AES
                        ,      ra
                        ,      NomeAluno
                        ,      Carga_Hora_Total
                        ,      Carga_Mes
                        ,      mes_ref
                        ,      idMatricula, idCursoTurnoTurma , nomeCurso)

                        select Numero_AES, nomeCurso
                        ,      Item_AES
                        ,      idMatricula
                        ,      ra
                        ,      NomeAluno
                        ,      format(max( cast(concat(substring(mes_ref,1,2),'/01/',substring(mes_ref,4,4)) as date ) ),'MM/yyyy') ult_mes_lanc
                        ,      Carga_Hora_Total



                        ,      (select sum(cutc.cargaHoraria) / 60
                          from   [dbo].[CursoTurnoTurma] cutu
                          ,      [dbo].[CursoTurnoTurmaCalendario] cutc
                          where  cutu.idCursoTurnoTurma = hora.idCursoTurnoTurma
                          and    cutu.idCursoTurnoTurma = cutc.idCursoTurnoTurma ) CargaRealizadaCurso


                        ,      sum(Carga_Mes_Realizada) Carga_Realizada_Aluno


                        from   HorasRealizadas hora

                        group  by 
                               Numero_AES,nomeCurso
                        ,      Item_AES
                        ,      idMatricula
                        ,      ra
                        ,      NomeAluno
                        ,      Carga_Hora_Total
                        ,hora.idCursoTurnoTurma";

            SqlDataReader dr = db.GetDataReader(sql);

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    lista.Add(new VwAcompCargaHoraria
                    {
                        Numero_AES = dr["Numero_AES"] != DBNull.Value ? dr["Numero_AES"].ToString() : "",
                        Item_AES = dr["Item_AES"] != DBNull.Value ? dr["Item_AES"].ToString() : "",
                        idMatricula = dr["idMatricula"] != DBNull.Value ?  Convert.ToInt32(dr["idMatricula"]) : 0,
                        ra = dr["ra"] != DBNull.Value ? dr["ra"].ToString() : "",
                        NomeAluno =  dr["NomeAluno"] != DBNull.Value ? dr["NomeAluno"].ToString() : "",
                        ult_mes_lanc = dr["ult_mes_lanc"].ToString(),
                        Carga_Hora_Total = dr["Carga_Hora_Total"] != DBNull.Value ?  Convert.ToInt32(dr["Carga_Hora_Total"]) : 0,
                        CargaRealizadaCurso =  dr["CargaRealizadaCurso"] != DBNull.Value ? Convert.ToInt32(dr["CargaRealizadaCurso"]) : 0 ,
                        Carga_Realizada_Aluno =  dr["Carga_Realizada_Aluno"] != DBNull.Value ?  Convert.ToInt32(dr["Carga_Realizada_Aluno"]) : 0,
                        NomeCurso = dr["nomeCurso"].ToString()

                    });
                }
            }

            db.CloseDbConnection();

            return lista;
        }



    }


    public class VwAcompCargaHoraria
    {
        public string Numero_AES { get; set; }
        public string Item_AES { get; set; }
        public int idMatricula { get; set; }
        public string ra { get; set; }
        public string NomeAluno { get; set; }
        public string ult_mes_lanc { get; set; }
        public int Carga_Hora_Total { get; set; }
        public int CargaRealizadaCurso { get; set; }
        public int Carga_Realizada_Aluno { get; set; }
        public string NomeCurso { get; set; }

        //public int Edicao { get; set; }

        //public string NumeroAES { get; set; }

        //public int ItemAES { get; set; }

        //public int IdMantenedora { get; set; }

        //public string NomeMantenedora { get; set; }

        //public int IdMantida { get; set; }

        //public string NomeMantida { get; set; }

        //public string DataInicioAula { get; set; }

        //public int IdCurso { get; set; }

        //public string NomeCurso { get; set; }

        //public int TotalCargaHorario { get; set; }

        //public int Saldo { get; set; }

        //public int Duracao { get; set; }

        //public int TotalCargaHorarioExecutada { get; set; }

        //public int QtdTurma { get; set; }

        //public int DuracaoExecutada { get; set; }

        //public string UltimoMesFrequencia { get; set; }

        //public string Integralizado { get; set; }

        //public int NumeroDiasLetivos { get; set; }

        //public int CargaHorario { get; set; }

        //public string MesReferencia { get; set; }
    }

    public class CargaHorariaVM
    {
        public string NumeroAES { get; set; }

        public int ItemAES { get; set; }

        public string Chave { get; set; }

        public List<VwAcompCargaHoraria> ListaCargaHoraria { get; set; }
    }
}