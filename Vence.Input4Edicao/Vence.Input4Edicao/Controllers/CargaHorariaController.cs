using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vence.Input4Edicao.Models;

namespace Vence.Input4Edicao.Controllers
{
    public class CargaHorariaController : Controller
    {        
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

        public FileResult GerarRelatorio(string chave)
        {
            try
            {
                Report relatorio = new Report();
                var parametros = new List<ReportParameter>();
                parametros.Add(new ReportParameter { Name = "Chave", Value = chave.ToString() });
                byte[] file = relatorio.GetReportFile("CargaHorariaVence", "/Vence", ReportFormat.XLS, parametros);
                return File(file, ReportFormat.XLS.GetEnumDescription(), "CargaHorariaVence.xls");
            }
            catch (Exception e)
            {
                return null;
            }
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
}