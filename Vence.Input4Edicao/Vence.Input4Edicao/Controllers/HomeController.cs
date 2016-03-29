using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using System.Text;
using System.IO;

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
            conn.Close();
            return Json(retorno, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Help()
        {
            return View();
        }

        public void DownloadPPT()
        {
            System.IO.FileInfo file = new System.IO.FileInfo(HttpContext.Server.MapPath("~/Content/Arquivos/Tutorial.pptx"));
            Response.Clear();
            Response.AddHeader("Content-Disposition", "attachment; filename=" + "Tutorial.pptx");
            Response.ContentType = "application/vnd.ms-powerpoint";
            Response.Buffer = true;

            using (FileStream fileStream = file.Open(FileMode.Open))
            {
                fileStream.CopyTo(Response.OutputStream);
            }

            Response.End();

        }

        public void DownloadWord()
        {
            System.IO.FileInfo file = new System.IO.FileInfo(HttpContext.Server.MapPath("~/Content/Arquivos/EvidênciaModelo.doc"));
            Response.Clear();
            Response.AddHeader("Content-Disposition", "attachment; filename=" + "ModeloEvidencia.doc");
            Response.ContentType = "application/vnd.ms-word";
            Response.Buffer = true;

            using (FileStream fileStream = file.Open(FileMode.Open))
            {
                fileStream.CopyTo(Response.OutputStream);
            }

            Response.End();

        }
        public ActionResult Index()
        {
            return View();
            // return  RedirectToAction("Manutencao", "Home");
        }
        public ActionResult Manutencao()
        {
            return View();
        }
        public ActionResult Finalizar()
        {
            return View();
        }
        public JsonResult Salvar(Formulario formulario)
        {

            string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection conn = new SqlConnection(_connectionString);
            StringBuilder sb = new StringBuilder();

            //VALIDAR SE RA´s existem no GDADE

            var alunosInvalidos = ValidarAlunosGDAE(formulario);

            try
            {
             

                if (alunosInvalidos.Count() == 0)
                {
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

                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
            
    

            return Json(alunosInvalidos, JsonRequestBehavior.AllowGet);

        }

        public JsonResult ValidarLancamentos(string token, string aes)
        {
            //DateTime? dt = null;
            //string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

            //var conn = new SqlConnection(_connectionString);
            //var cmd = new SqlCommand(string.Format(@" select DataFinalizado from [dbo].[Token] where Chave= '{0}' and AES = '{1}'  ", token, aes), conn);
            //cmd.Connection.Open();
            //var reader = cmd.ExecuteReader();
            //while (reader.Read())
            //{
            //    dt = reader["DataFinalizado"] != DBNull.Value ? Convert.ToDateTime(reader["DataFinalizado"]) : DateTime.MinValue;
            //}

            if (ValidarAESFinalizada(token))
                return Json(true, JsonRequestBehavior.AllowGet);
            else
                return Json(false, JsonRequestBehavior.AllowGet);
        }


        public bool ValidarAESFinalizada(string token)
        {
            DateTime? dt = null;
            string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

            var conn = new SqlConnection(_connectionString);
            var cmd = new SqlCommand(string.Format(@" select DataFinalizado from [dbo].[Token] where Chave= '{0}'  ", token), conn);
            cmd.Connection.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                dt = reader["DataFinalizado"] != DBNull.Value ? Convert.ToDateTime(reader["DataFinalizado"]) : DateTime.MinValue;
            }
            conn.Close();
            if (dt.HasValue && dt.Value != DateTime.MinValue)
                return true;
            else
                return false;

        }

        private bool ValidarTokenAcesso(string token)
        {
            string _token = string.Empty;
            string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

            var conn = new SqlConnection(_connectionString);
            var cmd = new SqlCommand(string.Format(@" select chave from [dbo].[Token] where Chave= '{0}' ", token), conn);
            cmd.Connection.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                _token = reader["chave"].ToString();
            }
            conn.Close();
            if (string.IsNullOrEmpty(_token))
                return false;
            else
                return true;
        }

        public JsonResult FinalizarLancamentos(string token)
        {
            string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection conn = new SqlConnection(_connectionString);
            string comando = "";

            if (ValidarTokenAcesso(token))
            {
                comando = string.Format(@" update [dbo].[Token]
                                            set DataFinalizado = getdate()
                                            where Chave= '{0}' ", token);


                var cmd = new SqlCommand(comando, conn);
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(false, JsonRequestBehavior.AllowGet);
        }

        private List<Aluno> ValidarAlunosGDAE(Formulario formulario)
        {

            var listaAlunosRaInvalidos = new List<Aluno>();
            var rasDuplicados = new List<Aluno>();

            foreach (var item in formulario.Aluno)
            {

                var alunoRaInvalido = new Aluno();

                if (!(ValidarRaAlunoGDAE(item.RA)))
                {
                    alunoRaInvalido.RA = item.RA;
                    alunoRaInvalido.Nome = item.Nome;
                    listaAlunosRaInvalidos.Add(alunoRaInvalido);


                }

            }

            var listaAgrupada = formulario.Aluno.GroupBy(_ => _.RA);

            if (listaAgrupada.Count() < formulario.Aluno.Count())
            {
                foreach (var item in formulario.Aluno)
                {
                    var alunoDuplicado = new Aluno();
                    var alunos = formulario.Aluno.Where(_ => _.RA == item.RA);
                    if (alunos.Count() > 1)
                    {
                        alunoDuplicado.RA = item.RA;
                        alunoDuplicado.Nome = item.Nome;
                        listaAlunosRaInvalidos.Add(alunoDuplicado);

                    }
                }

            }





            return listaAlunosRaInvalidos;
        }

        private bool ValidarRaAlunoGDAE(string ra)
        {

            string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionStringSql"].ConnectionString;
            string retorno = string.Empty;
            var conn = new SqlConnection(_connectionString);
            var cmd = new SqlCommand(@" select nr_ra  from [CADALUNOS].[TB_ALUNO] where nr_ra =  '" + ra + "'", conn);
            cmd.Connection.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                retorno = reader["nr_ra"].ToString();
            }
            conn.Close();

            return string.IsNullOrEmpty(retorno) ? false : true;

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
            conn.Close();

            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(turmas);

            return Json(json, JsonRequestBehavior.AllowGet);
        }
        public JsonResult BuscarDatasReferencia(string numeroAes, string itemAes)
        {
            List<Turma> turmas = new List<Turma>();
            string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection conn = new SqlConnection(_connectionString);
            turmas.Add(new Turma { Id = 0, Nome = "Selecione..." });

            SqlCommand cmd = new SqlCommand(@" ",conn);

            cmd.Connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                turmas.Add(new Turma { Id = Convert.ToInt32(reader[0]), Nome = reader[1].ToString() });
            }
            conn.Close();

            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(turmas);

            return Json(json, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult VerificarMesAnoLancados(int idTurma = 0, string mesAno = "")
        {
            int _mesLancado = 0;
            string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection conn = new SqlConnection(_connectionString);
            if (idTurma == 0 && mesAno == "")
            {
                return Json(false);
            }
            else
            {
                SqlCommand cmd = new SqlCommand(string.Format(@" select count(*) as TotalLancadas from frequencia4edicao where idcursoturnoturma = {0} and mesreferencia ='{1}' ", idTurma, mesAno), conn);
                cmd.Connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    _mesLancado = reader["TotalLancadas"] != DBNull.Value ? Convert.ToInt32(reader["TotalLancadas"]) : 0;
                }

                conn.Close();
                return Json((_mesLancado > 0), JsonRequestBehavior.AllowGet);   
            }
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
            conn.Close();
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