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
      
        public JsonResult BuscarAlunos(int IdTurma,string mesReferencia)
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
                lista.Add(new Aluno { Matricula = reader["idMatricula"].ToString(), Nome = reader["NomeAluno"].ToString(), RA = reader["RA"].ToString(),StatusAluno = reader["idstsaluno"].ToString(),
                 AprovadoVence = (reader["Aprovado"] == DBNull.Value) ? 0 : Convert.ToInt32(reader["Aprovado"]), Estagio = reader["Estagio"].ToString(),
                 IgnorarAluno = (reader["Ignorar"] == DBNull.Value) ? 0 : Convert.ToInt32(reader["Ignorar"])});
            }

            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(lista);

            return Json(json, JsonRequestBehavior.AllowGet);
        }
    }
}