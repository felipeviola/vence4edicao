using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Vence.Input4Edicao.Controllers
{
    public class AlunoController : Controller
    {
        // GET: Aluno
        public ActionResult Index()
        {
            return View();
        }

        public PartialViewResult ValidarToken(string token = "", int codEdicao = 0)
        {

            var listaAes = new List<AES>();
            int codDe = 0;

            codDe = ObterDeApartirDoToken(token);

            if (codDe > 0)
            {
                listaAes = ObterAES(codDe, codEdicao);
                return PartialView("partialAES", listaAes);
            }
            else
            {
                ViewBag.ErroPesquisaToken = true;
                return PartialView("partialViewError");
            }
        }

        private List<AES> ObterAES(int codDe, int codEdicao)
        {

            string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            var conn = new SqlConnection(_connectionString);
            var listaAES = new List<AES>();
            string comando = string.Format(@"   select  
                                                    cod_de,
                                                    Numero_AES,
                                                    Mantenedora,
                                                    edicao  
                                                from [dbo].[vw_mantida_curso] 
	                                            where cod_de =  {0}
	                                                and  (EDICAO = {1} or {2} =  0)
		                                            group by   cod_de,
                                                    Numero_AES,
                                                    Mantenedora, edicao order by Numero_AES,  Mantenedora ", codDe, codEdicao, codEdicao);

            var cmd = new SqlCommand(comando, conn);

            cmd.Connection.Open();
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var aes = new AES();
                aes.Mantenedora = reader["Mantenedora"].ToString();
                aes.CodDE = reader["cod_de"] != DBNull.Value ? Convert.ToInt32(reader["cod_de"]) : 0;
                aes.NumeroAes = reader["Numero_AES"].ToString();
                aes.CodEdicao = reader["edicao"] != DBNull.Value ? Convert.ToInt32(reader["edicao"]) : 0;
                listaAES.Add(aes);
            }

            return listaAES.Count() > 0 ? listaAES.Where(_ => _.CodEdicao < 5).ToList() : listaAES;


        }
        public PartialViewResult ObterTotalAlunos(string AES)
        {
            var alunosAtivos = ObterAlunos(AES);

            return PartialView("partialAlunos", alunosAtivos);
        }

        public PartialViewResult AtualizarAtivos(Alunos alunos)
        {
            if (ValidarAlunos(alunos))
            {

                if (AtualiarAlunos(alunos))
                {

                    var alunosAtualizados = ObterAlunoAtualizados(alunos.NumeroAes);

                    return PartialView("partialAtivos", alunosAtualizados);
                }
                else
                {
                    ViewBag.ErroPesquisaToken = false;
                    return PartialView("partialViewError");

                }
            }
            else
            {
                ViewBag.ErroPesquisaToken = false;
                return PartialView("partialViewError");
            }
        }

        public PartialViewResult AtualizarGrid(string AES)
        {
            var alunosAtualizados = ObterAlunoAtualizados(AES);

            return PartialView("partialAtivos", alunosAtualizados);
        }
        private List<Alunos> ObterAlunoAtualizados(string numeroAes)
        {
            var listaAlunos = new List<Alunos>();
            string comando = "";
            DateTime dt;
            string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            var conn = new SqlConnection(_connectionString);
            comando = string.Format(@" SELECT wmc.Mantida, wmc.curso, ac.item_aes, total_alunos_ativos, data_referencia
                                          FROM [dbo].[AlunosAtivosContigencia] ac inner join 
                                              [dbo].[vw_mantida_curso] wmc on ac.cod_mantida = wmc.Cod_Mantida and wmc.cod_Curso = ac.cod_Curso
	                                          where wmc.Numero_AES = '{0}'
	                                          group by wmc.Mantida, ac.item_aes, total_alunos_ativos,data_referencia, wmc.curso
	                                          order by wmc.Mantida, ac.item_aes, data_referencia", numeroAes);

            var cmd = new SqlCommand(comando, conn);
            cmd.Connection.Open();
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var aluno = new Alunos();

                aluno.Mantida = reader["Mantida"].ToString();
                aluno.ItemAes = reader["item_aes"].ToString();
                aluno.Curso = reader["curso"].ToString();
                aluno.TotalAtivos = reader["total_alunos_ativos"] != DBNull.Value ? Convert.ToInt32(reader["total_alunos_ativos"]) : 0;
                if (reader["data_referencia"] != DBNull.Value)
                {
                    dt = Convert.ToDateTime(reader["data_referencia"]);

                    string mesAno = dt.Month < 10 ? string.Format("0{0}/{1}", dt.Month, dt.Year) : string.Format("{0}/{1}", dt.Month, dt.Year);
                    aluno.MesAno = mesAno;
                }
                listaAlunos.Add(aluno);
            }

            return listaAlunos;

        }

        private bool ValidarAlunos(Alunos alunos)
        {
            bool alunoValido = true;

            if (string.IsNullOrEmpty(alunos.NumeroAes))
                alunoValido = false;
            if (alunos.CodDe == 0)
                alunoValido = false;
            if (alunos.CodMantenedora == 0)
                alunoValido = false;
            if (alunos.CodCurso == 0)
                alunoValido = false;
            if (string.IsNullOrEmpty(alunos.ItemAes))
                alunoValido = false;
            if (alunos.TotalAtivos == 0)
                alunoValido = false;
            if (alunos.DataReferencia == DateTime.MinValue)
                alunoValido = false;


            return alunoValido;
        }

        private AlunosAtivosViewModel ObterAlunos(string AES)
        {
            var alunosViewModel = new AlunosAtivosViewModel();
            var aes = new AES();
            string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            var conn = new SqlConnection(_connectionString);
            var listaAlunos = new List<Alunos>();
            string comando = string.Format(@" select 
                                    cod_curso,
                                    cod_mantenedora,
                                   cod_de,
                                   Numero_AES,
                                   Mantenedora,
                                    Mantida,
	                               Item_AES,
	                               NomeCurso ,
	                               vagas,	
                                    cod_mantida,
	                               (select matriculados from [autorizacaoServicoCursoMtd] 
	                                  where codMtn = wmc.cod_mantenedora 
		                              and edicao = wmc.edicao
		                              and codMtd = wmc.Cod_Mantida
		                              and codCurso = wmc.Cod_Curso
		                              ) as matriculados
                                from [dbo].[vw_mantida_curso] wmc
	                            where Numero_AES =  '{0}'
	                            
	                             order by Mantida, Item_AES ", AES);

            var cmd = new SqlCommand(comando, conn);

            cmd.Connection.Open();
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var alunos = new Alunos();
                alunos.CodCurso = reader["cod_curso"] != DBNull.Value ? Convert.ToInt32(reader["cod_curso"]) : 0;
                alunos.CodMantenedora = reader["cod_mantenedora"] != DBNull.Value ? Convert.ToInt32(reader["cod_mantenedora"]) : 0;
                alunos.CodDe = reader["cod_de"] != DBNull.Value ? Convert.ToInt32(reader["cod_de"]) : 0;
                alunos.NumeroAes = reader["Numero_AES"].ToString();
                alunos.Mantenedora = reader["Mantenedora"].ToString();
                alunos.ItemAes = reader["Item_AES"].ToString();
                alunos.Curso = reader["NomeCurso"].ToString();
                alunos.TotalVagas = reader["vagas"] != DBNull.Value ? Convert.ToInt32(reader["vagas"]) : 0;
                alunos.TotalMatriculados = reader["matriculados"] != DBNull.Value ? Convert.ToInt32(reader["matriculados"]) : 0;
                alunos.Mantida = reader["Mantida"].ToString();
                alunos.CodMantida = reader["cod_mantida"] != DBNull.Value ? Convert.ToInt32(reader["cod_mantida"]) : 0;
                listaAlunos.Add(alunos);
            }

            alunosViewModel.ListarAlunos = listaAlunos;
            aes.CodDE = listaAlunos.FirstOrDefault().CodDe;
            aes.NumeroAes = listaAlunos.FirstOrDefault().NumeroAes;
            aes.CodMantenedora = listaAlunos.FirstOrDefault().CodMantenedora;
            aes.Mantenedora = listaAlunos.FirstOrDefault().Mantenedora;
            alunosViewModel.AES = aes;

            return alunosViewModel;
        }

        private int ObterDeApartirDoToken(string token)
        {
            int codDe = 0;

            string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            var conn = new SqlConnection(_connectionString);
            var cmd = new SqlCommand(@" select cd_de from [dbo].[DiretoriaTokenContigencia] where token =  '" + token + "'", conn);
            cmd.Connection.Open();
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                codDe = Convert.ToInt32(reader["cd_de"]);
            }

            return codDe;

        }

        private bool AtualiarAlunos(Alunos alunos)
        {
            string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection conn = new SqlConnection(_connectionString);
            StringBuilder sb = new StringBuilder();

            try
            {
                Excluir(alunos);

                sb.Append(" INSERT INTO AlunosAtivosContigencia (numero_aes, cod_de, cod_mantenedora, cod_mantida, cod_curso, item_aes,total_alunos_ativos,data_referencia ) values('").Append(alunos.NumeroAes).Append("',").Append(alunos.CodDe).Append(",").Append(alunos.CodMantenedora).Append(",").Append(alunos.CodMantida).Append(",").Append(alunos.CodCurso).Append(",'").Append(alunos.ItemAes).Append("',").Append(alunos.TotalAtivos).Append(",").Append(string.Format("Convert(datetime, '{0}',103)", alunos.DataReferencia.ToShortDateString())).Append(")");
                SqlCommand cmd = new SqlCommand(sb.ToString(), conn);
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                sb.Clear();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }



        }

        private void Excluir(Alunos alunos)
        {
            string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection conn = new SqlConnection(_connectionString);
            string comando = string.Format(@"delete from  [dbo].[AlunosAtivosContigencia] 
                                                where numero_aes = '{0}'
                                                and cod_de = {1}
                                                and cod_mantenedora = {2}
                                                and cod_curso = {3}
                                                and item_aes = {4}
                                                and  year(data_referencia) = {5}
                                                and  month(data_referencia) = {6}"
                                                , alunos.NumeroAes,
                                                alunos.CodDe,
                                                alunos.CodMantenedora,
                                                alunos.CodCurso,
                                                alunos.ItemAes,
                                                alunos.DataReferencia.Year,
                                                alunos.DataReferencia.Month
                                           );

            SqlCommand cmd = new SqlCommand(comando, conn);
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();

        }
    }




    public class Alunos
    {
        public string NumeroAes { get; set; }
        public int CodDe { get; set; }
        public int CodMantenedora { get; set; }
        public string Mantenedora { get; set; }
        public string ItemAes { get; set; }
        public int TotalVagas { get; set; }
        public int TotalMatriculados { get; set; }
        public string Curso { get; set; }
        public int CodCurso { get; set; }
        public DateTime DataReferencia { get; set; }
        public int TotalAtivos { get; set; }
        public string Mantida { get; set; }
        public int CodMantida { get; set; }

        public List<Alunos> ListarAlunos { get; set; }

        public string MesAno { get; set; }
    }

    public class AES
    {
        public int CodDE { get; set; }
        public string NumeroAes { get; set; }
        public string Mantenedora { get; set; }
        public int CodEdicao { get; set; }
        public int CodMantenedora { get; set; }

    }

    public class AlunosAtivosViewModel
    {

        public Alunos Alunos { get; set; }
        public AES AES { get; set; }
        public List<Alunos> ListarAlunos { get; set; }

    }
}