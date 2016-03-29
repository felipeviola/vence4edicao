using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Vence.Input4Edicao.Controllers
{
    public class ConsultaController : Controller
    {
        // GET: Consulta
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult BuscarDatasReferencia(int idTurma)
        {
            List<MesesReferencia> lista = new List<MesesReferencia>();
            string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection conn = new SqlConnection(_connectionString);
            lista.Add(new MesesReferencia { value = "", text = "Selecione..." });
            SqlCommand cmd = new SqlCommand(string.Format(@" select MesReferencia  from aluno4edicao where idcursoturnoturma = {0} group by MesReferencia ", idTurma), conn);
            cmd.Connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new MesesReferencia
                {
                    text = reader["MesReferencia"].ToString(),
                    value = reader["MesReferencia"].ToString()
                });
            }

            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(lista);

            return Json(json, JsonRequestBehavior.AllowGet);

        }
        public JsonResult BuscarAlunos(int IdTurma, string mesReferencia)
        {
            List<Aluno> lista = new List<Aluno>();
            string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection conn = new SqlConnection(_connectionString);
            SqlCommand cmd = new SqlCommand(@"select insc.NomeAluno,insc.NomeAluno,matr.idMatricula,matr.idstsaluno,(select top 1 RAGDAE from aluno4edicao a where matr.idMatricula = a.idMatricula) as RA,
                                                (select HorasEstagio from aluno4edicao a where matr.idMatricula = a.idMatricula and MesReferencia = '" + mesReferencia + @"') as Estagio,
                                                (select AprovadoVence from aluno4edicao a where matr.idMatricula = a.idMatricula and MesReferencia = '" + mesReferencia + @"') as Aprovado,
                                                (select IgnorarAluno from aluno4edicao a where matr.idMatricula = a.idMatricula and MesReferencia = '" + mesReferencia + @"') as Ignorar
                                                from    Matricula matr
                                                ,       inscricao insc
                                                where  matr.idCursoTurnoTurma = " + IdTurma + @" 
                                                and    matr.idinscricao = insc.idinscricao
                                                order by matr.idstsaluno,insc.NomeAluno", conn);
            cmd.Connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Aluno
                {
                    Matricula = reader["idMatricula"].ToString(),
                    Nome = reader["NomeAluno"].ToString(),
                    RA = reader["RA"].ToString(),
                    StatusAluno = reader["idstsaluno"].ToString(),
                    AprovadoVence = (reader["Aprovado"] == DBNull.Value) ? 0 : Convert.ToInt32(reader["Aprovado"]),
                    Estagio = reader["Estagio"].ToString(),
                    IgnorarAluno = (reader["Ignorar"] == DBNull.Value) ? 0 : Convert.ToInt32(reader["Ignorar"])
                });
            }

            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(lista);

            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DetalheAluno(string Matricula, string MesReferencia, string IdCursoTurnoTurma)
        {
            List<Presenca> lista = new List<Presenca>();
            string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection conn = new SqlConnection(_connectionString);
            string cmdText = string.Format(@" select   DiaPresenca,a.MesReferencia,CargaHoraria,(select count(*) from Calendario4edicao where MesReferencia = '{1}' and idcursoturnoturma = {2}) as DiasLetivos
                        from Frequencia4Edicao a left join Calendario4edicao b on a.idcursoturnoturma = b.IdCursoTurnoTurma
                        where a.idcursoturnoturma = b.IdCursoTurnoTurma
						-- where a.DiaPresenca = b.DiaLetivo 
						--and a.MesReferencia = b.MesReferencia 
						and idMatricula =  {0} 
						and a.MesReferencia = '{1}'
                        group by 
						 DiaPresenca,a.MesReferencia,CargaHoraria ", Matricula, MesReferencia, IdCursoTurnoTurma);

//            string cmdText = string.Format(@"select distinct  DiaPresenca,a.MesReferencia,CargaHoraria,(select count(*) from Calendario4edicao where MesReferencia = '{1}' and idcursoturnoturma = {2}) as DiasLetivos
//                                                from Frequencia4Edicao a , Calendario4edicao b
//                                                where a.DiaPresenca = b.DiaLetivo and a.MesReferencia = b.MesReferencia and idMatricula = '{0}' and a.MesReferencia = '{1}'", Matricula, MesReferencia, IdCursoTurnoTurma);

            SqlCommand cmd = new SqlCommand(cmdText, conn);
            cmd.Connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Presenca { DiaLetivo = reader["DiaPresenca"].ToString(), Horas = reader["CargaHoraria"].ToString(), TotalDiasLetivos = Convert.ToInt32(reader["DiasLetivos"]) });
            }
            cmd.Connection.Close();
            if (lista.Count() == 0)
            {
                cmdText = string.Format("select count(*) as DiasLetivos from Calendario4edicao where MesReferencia = '{1}' and idcursoturnoturma = {2}", Matricula, MesReferencia, IdCursoTurnoTurma);
                cmd = new SqlCommand(cmdText, conn);
                cmd.Connection.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new Presenca { DiaLetivo = "", Horas = "", TotalDiasLetivos = Convert.ToInt32(reader["DiasLetivos"]) });
                }
                cmd.Connection.Close();
            }
            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(lista);

            return Json(json, JsonRequestBehavior.AllowGet);
        }


    }

    public class MesesReferencia
    {
        public string value { get; set; }
        public string text { get; set; }
    }
}