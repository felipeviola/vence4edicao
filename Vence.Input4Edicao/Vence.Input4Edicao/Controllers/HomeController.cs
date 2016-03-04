using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using System.Text;

namespace Vence.Input4Edicao.Controllers
{
    public class HomeController : Controller
    {
        public JsonResult Token(Formulario formulario)
        {
            string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            string retorno = string.Empty;
            SqlConnection conn = new SqlConnection(_connectionString);
            SqlCommand cmd = new SqlCommand(@"select AES
                                                from   token 
                                                where  chave = '" + formulario.Token + "'", conn);
            cmd.Connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                retorno = reader["AES"].ToString();
            }
            return Json(retorno, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult Salvar(Formulario formulario)
        {
            string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection conn = new SqlConnection(_connectionString);
            StringBuilder sb = new StringBuilder();

            SqlCommand cmdDelete = new SqlCommand(String.Format("delete Calendario4edicao where idcursoturnoturma = {0} and mesReferencia ='{1}'", formulario.IdCursoTurnoTurma.ToString(), formulario.MesReferencia), conn);
            cmdDelete.Connection.Open();
            cmdDelete.ExecuteNonQuery();
            cmdDelete.Connection.Close();

            if (formulario.Calendario != null && formulario.Calendario.Count > 0)
            {
                foreach (var item in formulario.Calendario)
                {
                    sb.Append("insert into Calendario4edicao(IdCursoTurnoTurma,DiaLetivo,CargaHoraria,MesReferencia,CpfSupervisor) values(").Append(formulario.IdCursoTurnoTurma.ToString()).Append(",'").Append(item.Dia).Append("','").Append(item.CargaHoraria).Append("','").Append(formulario.MesReferencia).Append("','").Append(formulario.Cpf).Append("')");
                    SqlCommand cmd = new SqlCommand(sb.ToString(), conn);
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                    sb.Clear();
                }
            }
            if (formulario.Aluno != null & formulario.Aluno.Count > 0)
            {
                foreach (var item in formulario.Aluno)
                {
                    if (item.Estagio == null)
                        item.Estagio = "";
                    if (item.RA == null)
                        item.RA = "";
                    sb.Append("insert into aluno4edicao(IdMatricula,HorasEstagio,IgnorarAluno,AprovadoVence,RAGDAE,MesReferencia,IdCursoTurnoTurma,IdInscricao) values(").Append(item.Matricula).Append(",'").Append(item.Estagio).Append("',").Append(item.IgnorarAluno.ToString()).Append(",")
                        .Append(item.AprovadoVence).Append(",'").Append(item.RA).Append("','").Append(formulario.MesReferencia).Append("',")
                        .Append(formulario.IdCursoTurnoTurma).Append(",").Append(item.Inscricao).Append(")");
                    SqlCommand cmd = new SqlCommand(sb.ToString(), conn);
                    cmdDelete.CommandText = String.Format("delete aluno4edicao where idMatricula = '{0}' and MesReferencia = '{1}'", item.Matricula, formulario.MesReferencia);
                    SqlCommand cmdUpdate = new SqlCommand(string.Format("update aluno4edicao set RAGDAE = '{1}' where idMatricula = '{0}'", item.Matricula, item.RA), conn);
                    cmd.Connection.Open();
                    cmdDelete.ExecuteNonQuery();
                    cmd.ExecuteNonQuery();
                    cmdUpdate.ExecuteNonQuery();
                    cmd.Connection.Close();
                    sb.Clear();

                    cmdDelete.Connection.Open();
                    cmdDelete.CommandText = String.Format("delete Frequencia4Edicao where idMatricula='{0}' and MesReferencia ='{1}' and idCursoTurnoTurma={2}", item.Matricula, formulario.MesReferencia, formulario.IdCursoTurnoTurma.ToString());
                    cmdDelete.ExecuteNonQuery();
                    cmdDelete.Connection.Close();

                    if (item.Presenca != null)
                    {
                        foreach (var item2 in item.Presenca)
                        {
                            sb.Append("insert into Frequencia4Edicao(IdMatricula,DiaPresenca,MesReferencia,IdCursoTurnoTurma,IdInscricao) values(").Append(item.Matricula).Append(",'").Append(item2.DiaLetivo).Append("','")
                                .Append(formulario.MesReferencia.ToString()).Append("',").Append(formulario.IdCursoTurnoTurma)
                                .Append(",").Append(item.Inscricao).Append(")");
                            cmd.Connection.Open();
                            cmd = new SqlCommand(sb.ToString(), conn);
                            cmd.ExecuteNonQuery();
                            cmd.Connection.Close();
                            sb.Clear();
                        }
                    }
                }
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        public JsonResult BuscarTurmas(Formulario filtros)
        {
            List<Turma> turmas = new List<Turma>();
            string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection conn = new SqlConnection(_connectionString);
            turmas.Add(new Turma { Id = 0, Nome = "Selecione..." });
            SqlCommand cmd = new SqlCommand(@"select distinct a.idCursoTurnoTurma,(select (curso + '/' +  b.DescTurno + '/' + nomeTurma) as turno from CursoTurnoTurma a join turno b on a.turno = b.turno 
                                                join CursoMtdTurno c on a.idcursomtdturno = c.idcursomtdturno
                                                join cursos d on d.codCurso = c.codCurso
                                                    where idcursoturnoturma = vwmtd.idCursoTurnoTurma) from   vw_mantida_curso_edicao as vwmtd
                                                join Cursoturnoturma a on a.idcursoTurnoTurma = vwmtd.idcursoturnoturma
                                                where numero_aes = '" + filtros.NumeroAES + @"' and a.StatusCursoTurma <> 3
                                                and item_aes = " + filtros.ItemAES, conn);

            cmd.Connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                turmas.Add(new Turma { Id = Convert.ToInt32(reader[0]), Nome = reader[1].ToString() });
            }

            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(turmas);

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarAlunos(int IdTurma, string mesReferencia)
        {
            List<Aluno> lista = new List<Aluno>();
            string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection conn = new SqlConnection(_connectionString);
            SqlCommand cmd = new SqlCommand(@"select insc.NomeAluno,insc.NomeAluno,matr.idMatricula,(select top 1 RAGDAE from aluno4edicao a where matr.idMatricula = a.idMatricula) as RA,
                                                insc.idInscricao
                                                from    Matricula matr
                                                ,       inscricao insc
                                                where  matr.idCursoTurnoTurma = " + IdTurma + @" 
                                                and    matr.idinscricao = insc.idinscricao and matr.idstsaluno <> 2
                                                order by insc.NomeAluno", conn);
            cmd.Connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Aluno { Matricula = reader["idMatricula"].ToString(), Nome = reader["NomeAluno"].ToString(), RA = reader["RA"].ToString(), Inscricao = Convert.ToInt32(reader["idInscricao"]) });
            }

            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(lista);

            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public JsonResult VerificarCadastro(Formulario formulario)
        {
            string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection conn = new SqlConnection(_connectionString);
            StringBuilder sb = new StringBuilder();
            bool cadastrado = false;

            SqlCommand cmdSelect = new SqlCommand(String.Format("select * from Calendario4edicao where idcursoturnoturma = {0} and mesReferencia ='{1}'", formulario.IdCursoTurnoTurma.ToString(), formulario.MesReferencia), conn);
            cmdSelect.Connection.Open();
            var reader = cmdSelect.ExecuteReader();
            while (reader.Read())
            {
                cadastrado = true;
            }
            cmdSelect.Connection.Close();

            return Json(cadastrado, JsonRequestBehavior.AllowGet);
        }
    }
    public class Formulario
    {
        public string Token { get; set; }
        public int IdCursoTurnoTurma { get; set; }
        public string NumeroAES { get; set; }
        public string ItemAES { get; set; }
        public string MesReferencia { get; set; }
        public string Cpf { get; set; }
        public List<Calendario> Calendario { get; set; }
        public List<Aluno> Aluno { get; set; }
    }
    public class Calendario
    {
        public string CargaHoraria { get; set; }
        public string Dia { get; set; }
    }
    public class Presenca
    {
        public string DiaLetivo { get; set; }
        public string Horas { get; set; }
        public int TotalDiasLetivos { get; set; }
        public bool StatusPresenca { get; set; }
    }
    public class Aluno
    {
        public string Nome { get; set; }
        public string RA { get; set; }
        public string Matricula { get; set; }
        public List<Presenca> Presenca { get; set; }
        public string Estagio { get; set; }
        public int IgnorarAluno { get; set; }
        public int AprovadoVence { get; set; }
        public string StatusAluno { get; set; }
        public int Inscricao { get; set; }

    }
    public class Turma
    {
        public int Id { get; set; }
        public string Nome { get; set; }
    }
}