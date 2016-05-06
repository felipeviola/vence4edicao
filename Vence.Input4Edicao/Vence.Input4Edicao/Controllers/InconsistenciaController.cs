using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Vence.Input4Edicao.Controllers
{
    public class InconsistenciaController : Controller
    {

        public ActionResult Index()
        {
            
            return View();
        }

        public JsonResult ValidarData(string dataOk = "", string dataAtual = "")
        {
            DateTime dtAtual = ValidarData(dataAtual);
            DateTime dtOk = ValidarData(dataOk);

            return Json((dtAtual == dtOk), JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult Corrigir(int idInconsistente = 0, string numeroAes = "", int idErroCod = 0, int idInconsistenciaItem = 0)
        {

            string _log = string.Format("idInconsistente:{0},  numeroAes:{1} ,  idErroCod:{2},idInconsistenciaItem: {3}", idInconsistente, numeroAes, idErroCod, idInconsistenciaItem);
            Inconsistencia _inconsistencia = new Inconsistencia();
            try
            {

                string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
                SqlConnection conn = new SqlConnection(_connectionString);

                SqlCommand cmd = new SqlCommand(string.Format(@"  select ERConsist,
		                                                                RAGDAE, 
		                                                                NomeAlunoGDAE,   
		                                                                DataNascimentoVence , 
                                                                        dataNascimentoGDAE,
		                                                                NomeMaeVence, 
		                                                                NomeMaeGDAE, 
		                                                                NomeAlunoVence

				                                                        ,(select cargahoraestagio as contratado from [dbo].vw_mantida v where  v.Numero_AES = ac.numeroAES and v.Item_AES = ac.itemAES ) as contratado

				                                                        ,(select top 1  cargaHoraEstagio from dbo.vw_estagio we where  left(we.ra,12) = ac.RAGDAE) as lancadoHP

				                                                        ,(select top 1 cast(left(HorasEstagio,3) as int)  from [dbo].[Aluno4Edicao] where RAGDAE = ac.RAGDAE and HorasEstagio is not null and HorasEstagio <> '') as lancadoVENCE

					                                                        from [Aluno4EdicaoConsist] ac inner join [Aluno4EdicaoConsistErros] ace on ac.idConsist = ace.idConsist
	                                                                    where ac.idConsist =  {0}
	                                                                    and ercod = {1}   and ac.numeroAES = '{2}'  ", idInconsistente, idErroCod, numeroAes), conn);

                cmd.Connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {

                    _inconsistencia.ERConsist = reader["ERConsist"].ToString();
                    _inconsistencia.RAGDAE = reader["RAGDAE"].ToString();
                    _inconsistencia.NomeAlunoGDAE = reader["NomeAlunoGDAE"].ToString();
                    string _dataNascimentoVence = reader["DataNascimentoVence"].ToString();
                    _inconsistencia.NomeMaeVence = reader["NomeMaeVence"].ToString();
                    _inconsistencia.NomeMaeGDAE = reader["NomeMaeGDAE"].ToString();
                    _inconsistencia.NomeAlunoVence = reader["NomeAlunoVence"].ToString();
                    string _dataNascimentoGDAE = reader["dataNascimentoGDAE"].ToString();
                    _inconsistencia.DataNascimentoGDAE = !(string.IsNullOrEmpty(_dataNascimentoGDAE)) ? Convert.ToDateTime(_dataNascimentoGDAE).ToShortDateString() : _dataNascimentoGDAE;
                    _inconsistencia.DataNascimentoVence = !(string.IsNullOrEmpty(_dataNascimentoVence)) ? Convert.ToDateTime(_dataNascimentoVence).ToShortDateString() : _dataNascimentoVence;
                    _inconsistencia.idConsist = idInconsistente;
                    _inconsistencia.IdConsistItem = idInconsistenciaItem;

                    _inconsistencia.TotalEstagioContratado = reader["contratado"] != DBNull.Value ? Convert.ToInt32(reader["contratado"]) : 0;
                    _inconsistencia.TotalEstagioHP = reader["lancadoHP"] != DBNull.Value ? Convert.ToInt32(reader["lancadoHP"]) : 0;
                    _inconsistencia.TotalEstagioVENCE = reader["lancadoVENCE"] != DBNull.Value ? Convert.ToInt32(reader["lancadoVENCE"]) : 0;
                    _inconsistencia.TotalEstagioLancado = (_inconsistencia.TotalEstagioHP + _inconsistencia.TotalEstagioVENCE);

                }

                _inconsistencia.TipoInconsistencia = GetEnum(idErroCod);

                return PartialView("_partialCorrecao", _inconsistencia);

               
            }
            catch (Exception ex)
            {
                SalvarLog(string.Format("Exception: {0}, detalhes: {1}", ex.ToString(),_log));
                return PartialView("_partialErro");
            }


        }

        private TipoInconsistencia GetEnum(int codigo)
        {
            if (codigo == 10)
            {
                return TipoInconsistencia.NomeAlunoNaoConfere;
            }
            else if (codigo == 20)
            {
                return TipoInconsistencia.NomeMaeNaoConfere;
            }
            else if (codigo == 30)
            {
                return TipoInconsistencia.DataNascimentoNaoConfere;
            }
            else if (codigo == 40)
            {
                return TipoInconsistencia.EstagioUltrapassaTotal;
            }
            else
            {
                return TipoInconsistencia.NaoEncontrado;
            }
        }

        public JsonResult IgnorarAluno(int idInconsistente = 0, string numeroAes = "", string token = "")
        {
            string _log = string.Format("idInconsistente:{0},  numeroAes:{1} ,  token:{2}", idInconsistente, numeroAes, token);
            try
            {
                if (idInconsistente != 0 && numeroAes != "" && token != "")
                {

                    string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
                    SqlConnection conn = new SqlConnection(_connectionString);
                    string sql = string.Format(@" INSERT INTO [dbo].[Aluno4EdicaoConsistCorrecao]
							                       ([Token]
							                       ,[IdConsist]
							                       ,[idConsistItem]
							                       ,[NomeAluno]
							                       ,[NomeMae]
							                       ,[HorasEstagio]
							                       ,[FlIgnoradoParcela]
							                       ,[DataNascimento]
							                       )
                                                  select '{0}' as [Token], 
							                              {1} as [IdConsist] , 
									                      idConsistItem ,
									                      null as [NomeAluno],
									                      null as [NomeMae], 
									                      null as [HorasEstagio],
									                      1 as [FlIgnoradoParcela] ,
									                      null as [DataNascimento]
							                         from [dbo].[Aluno4EdicaoConsistErros] where idConsist = {1}  ", token, idInconsistente);


                    SqlCommand cmd = new SqlCommand(sql, conn);

                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
                else {

                    return Json(false, JsonRequestBehavior.AllowGet);

                }

                return Json(true, JsonRequestBehavior.AllowGet);

                //InconsistenciaViewModel _inconsistenciaViewModel = ListarInconsistentes(numeroAes);

                //return PartialView("_partialInconsistentes", _inconsistenciaViewModel);
            }
            catch (Exception ex)
            {
                SalvarLog(string.Format("Exception: {0}, detalhes: {1}", ex.ToString(), _log));
                //return PartialView("_partialInconsistentes");
                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult AtualizarDataNascimento(int idInconsistente = 0, string numeroAes = "", string token = "", string dataNascimento = "", int idInconsistenteItem = 0)
        {
            string _log = string.Format("idInconsistente:{0},  numeroAes:{1} ,  token:{2},idInconsistenciaItem: {3}, dtNascimento: {4}", idInconsistente, numeroAes, token, idInconsistenteItem, dataNascimento);
            try
            {
                DateTime dtValida = ValidarData(dataNascimento);

                if (idInconsistente != 0 && numeroAes != "" && token != "" && dataNascimento != "")
                {


                    string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
                    SqlConnection conn = new SqlConnection(_connectionString);
                    string sql = string.Format(@" INSERT INTO [dbo].[Aluno4EdicaoConsistCorrecao]
                                                                   ([Token]
                                                                   ,[IdConsist]
                                                                    ,[idConsistItem]
                                                                   ,[NomeAluno]
                                                                   ,[NomeMae]
                                                                   ,[DataNascimento]
                                                                   ,[HorasEstagio]
                                                                   ,[FlIgnoradoParcela]
                                                                  
                                                                   )
                                                             VALUES
                                                                   (
			                                                        '{0}'
			                                                        , {1}
			                                                        ,{3}
                                                                    ,null
			                                                        ,NULL
			                                                        ,'{2}'
			                                                        ,null
			                                                        ,0
		   
		                                                           ) ", token, idInconsistente, dtValida.ToShortDateString(), idInconsistenteItem);


                    SqlCommand cmd = new SqlCommand(sql, conn);

                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
                return Json(true, JsonRequestBehavior.AllowGet);
                //InconsistenciaViewModel _inconsistenciaViewModel = ListarInconsistentes(numeroAes);

                //return PartialView("_partialInconsistentes", _inconsistenciaViewModel);
            }
            catch (Exception ex )
            {
                SalvarLog(string.Format("Exception: {0}, detalhes: {1}", ex.ToString(), _log));
                return Json(false, JsonRequestBehavior.AllowGet);
                //return PartialView("_partialInconsistentes");
            }

        }

        public JsonResult AtualizarNomeAluno(int idInconsistente = 0, string numeroAes = "", string token = "", string nomeAluno = "", int idInconsistenteItem = 0)
        {
            string _log = string.Format("idInconsistente:{0},  numeroAes:{1} ,  token:{2},idInconsistenciaItem: {3}, nomeAluno: {4}", idInconsistente, numeroAes, token, idInconsistenteItem, nomeAluno);
            try
            {

                if (idInconsistente != 0 && numeroAes != "" && token != "" && nomeAluno != "")
                {

                    string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
                    SqlConnection conn = new SqlConnection(_connectionString);
                    string sql = string.Format(@" INSERT INTO [dbo].[Aluno4EdicaoConsistCorrecao]
                                                                   ([Token]
                                                                   ,[IdConsist]
                                                                    ,[idConsistItem]
                                                                   ,[NomeAluno]
                                                                   ,[NomeMae]
                                                                   ,[DataNascimento]
                                                                   ,[HorasEstagio]
                                                                   ,[FlIgnoradoParcela])
                                                             VALUES
                                                                   (
			                                                        '{0}'
			                                                        ,{1}
                                                                    ,{3}
			                                                        ,'{2}'
			                                                        ,NULL
			                                                        ,Null
			                                                        ,null
			                                                        ,0
		   
		                                                           ) ", token, idInconsistente, nomeAluno, idInconsistenteItem);


                    SqlCommand cmd = new SqlCommand(sql, conn);

                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
                else {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
                

                return Json(true, JsonRequestBehavior.AllowGet);
                //InconsistenciaViewModel _inconsistenciaViewModel = ListarInconsistentes(numeroAes);

                //return PartialView("_partialInconsistentes", _inconsistenciaViewModel);
            }
            catch (Exception ex)
            {
                SalvarLog(string.Format("Exception: {0}, detalhes: {1}", ex.ToString(), _log));
                return Json(false, JsonRequestBehavior.AllowGet);
                //return PartialView("_partialInconsistentes");
            }

        }

        public JsonResult ValidarNome(string nomeAtual, string nomeGDAE)
        {
            string nmAtual = removerAcentos(nomeAtual);
            string nmGDAE = removerAcentos(nomeGDAE);

            return Json((nmAtual.ToUpper().Trim() == nmGDAE.ToUpper().Trim()), JsonRequestBehavior.AllowGet);
        }

        public static string removerAcentos(string texto)
        {
            string comAcentos = "ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇç";
            string semAcentos = "AAAAAAaaaaaEEEEeeeeIIIIiiiiOOOOOoooooUUUuuuuCc";

            for (int i = 0; i < comAcentos.Length; i++)
            {
                texto = texto.Replace(comAcentos[i].ToString(), semAcentos[i].ToString());
            }
            return texto;
        }

        public JsonResult AtualizarNomeMae(int idInconsistente = 0, string numeroAes = "", string token = "", string nomeMae = "", int idInconsistenteItem = 0)
        {

            string _log = string.Format("idInconsistente:{0},  numeroAes:{1} ,  token:{2},idInconsistenciaItem: {3}, nomeMae: {4}", idInconsistente, numeroAes, token, idInconsistenteItem, nomeMae);
            try
            {

                if (idInconsistente != 0 && numeroAes != "" && token != "" && nomeMae != "")
                {

                    string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
                    SqlConnection conn = new SqlConnection(_connectionString);
                    string sql = string.Format(@" INSERT INTO [dbo].[Aluno4EdicaoConsistCorrecao]
                                                                   ([Token]
                                                                   ,[IdConsist]
                                                                    ,[idConsistItem]
                                                                   ,[NomeAluno]
                                                                   ,[NomeMae]
                                                                   ,[DataNascimento]
                                                                   ,[HorasEstagio]
                                                                   ,[FlIgnoradoParcela])
                                                             VALUES
                                                                   (
			                                                        '{0}'
			                                                        ,{1}
                                                                    ,{3}
			                                                        ,null
			                                                        ,'{2}'
			                                                        ,Null
			                                                        ,null
			                                                        ,0
		   
		                                                           ) ", token, idInconsistente, nomeMae, idInconsistenteItem);


                    SqlCommand cmd = new SqlCommand(sql, conn);

                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
                return Json(true, JsonRequestBehavior.AllowGet);
                //InconsistenciaViewModel _inconsistenciaViewModel = ListarInconsistentes(numeroAes);

                //return PartialView("_partialInconsistentes", _inconsistenciaViewModel);
            }
            catch (Exception ex)
            {
                SalvarLog(string.Format("Exception: {0}, detalhes: {1}", ex.ToString(), _log));
                return Json(false, JsonRequestBehavior.AllowGet);
                //return PartialView("_partialInconsistentes");
            }

        }

        public JsonResult AtualizarEstagio(int idInconsistente = 0, string numeroAes = "", string token = "", int totalEstagio = 0, int idInconsistenteItem = 0, string raGDAE = "")
        {
            string _log = string.Format("idInconsistente:{0},  numeroAes:{1} ,  token:{2},idInconsistenciaItem: {3}, totalEstagio: {4}", idInconsistente, numeroAes, token, idInconsistenteItem, totalEstagio);
            try
            {
                bool _atualizouVence = false;

                if (idInconsistente != 0 && numeroAes != "" && token != "" && idInconsistenteItem > 0 && raGDAE != "")
                {

                    string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
                    SqlConnection conn = new SqlConnection(_connectionString);
                    string sql = string.Format(@" INSERT INTO [dbo].[Aluno4EdicaoConsistCorrecao]
                                                                   ([Token]
                                                                   ,[IdConsist]
                                                                    ,[idConsistItem]
                                                                   ,[NomeAluno]
                                                                   ,[NomeMae]
                                                                   ,[DataNascimento]
                                                                   ,[HorasEstagio]
                                                                   ,[FlIgnoradoParcela])
                                                             VALUES
                                                                   (
			                                                        '{0}'
			                                                        ,{1}
                                                                    ,{2}
			                                                        ,null
			                                                        ,null
			                                                        ,Null
			                                                        ,{3}
			                                                        ,0
		   
		                                                           ) ", token, idInconsistente, idInconsistenteItem, totalEstagio);


                    SqlCommand cmd = new SqlCommand(sql, conn);

                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();


                }
                if (raGDAE != "")
                {
                    _atualizouVence = AtualizarEstagioVence(raGDAE, totalEstagio);

                }
                if (_atualizouVence)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {
                SalvarLog(string.Format("Exception: {0}, detalhes: {1}", ex.ToString(), _log));
                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }
        private bool AtualizarEstagioVence(string ra, int total)
        {

            try
            {
                string vlEstagio = "";
                if (total < 10)
                {
                    vlEstagio = string.Format("00{0}:00", total);
                }
                else if (total > 10 && total < 100)
                {
                    vlEstagio = string.Format("0{0}:00", total);
                }
                else if (total > 100 && total < 999)
                {
                    vlEstagio = string.Format("{0}:00", total);
                }

                string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
                SqlConnection conn = new SqlConnection(_connectionString);
                string sql = string.Format(@"  
                                        UPDATE [dbo].[Aluno4Edicao]
                                           SET 
                                              [HorasEstagio] = '{0}'
    
                                         WHERE RAGDAE = '{1}' ", vlEstagio, ra);


                SqlCommand cmd = new SqlCommand(sql, conn);

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private DateTime ValidarData(string dataNascimento)
        {
            DateTime dt = new DateTime();
            try
            {
                dt = Convert.ToDateTime(dataNascimento);
            }
            catch (Exception)
            {
                dt = DateTime.MinValue;
            }
            return dt;
        }

        public PartialViewResult ObterInconsistentes(string numeroAES = "")
        {
            string _log = string.Format("numeroAes:{0} ", numeroAES);

            try
            {
                InconsistenciaViewModel _inconsistenciaViewModel = ListarInconsistentes(numeroAES);

                return PartialView("_partialInconsistentes", _inconsistenciaViewModel);
            }
            catch (Exception)
            {
                return PartialView("_partialErro");
            }

        }

        private static InconsistenciaViewModel ListarInconsistentes(string numeroAES)
        {
            InconsistenciaViewModel _inconsistenciaViewModel = new InconsistenciaViewModel();
            List<Inconsistencia> lista = new List<Inconsistencia>();
            string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection conn = new SqlConnection(_connectionString);

            SqlCommand cmd = new SqlCommand(string.Format(@" select     mant.Mantenedora
					,       mant.Numero_AES
					,       mant.Item_AES
					,       mant.Mantida
					,       mant.Diretoria_Ensino
					,       mant.NomeCurso
					,       mant.vagas
					,       mant.cargaHoraEstagio
					,       mant.cargaHoraTotal
					,       mant.area
					,       toke.AES
					,       isnull(toke.QtdErros,0) QtdErros
					,       cons.idConsist
					,       erro.idConsistItem
					,       cons.numeroAES
					,       cons.itemAES
					,       cons.IdMatricula
					,       cons.IdInscricao
					,       cons.RAGDAE
					,       cons.NomeAlunoVence
					,       cons.NomeMaeVence
					,       cons.dataNascimentoVence
					,       cons.RGAlunoVence
					,       cons.NomeAlunoGDAE
					,       cons.NomeMaeGDAE
					,       cons.dataNascimentoGDAE
					,       cons.RGAlunoGDAE
					,       erro.ERCod
					,       erro.ERConsist
					from    [dbo].[Aluno4EdicaoConsist]      Cons
					,       [dbo].[Aluno4EdicaoConsistErros] erro
					,       [dbo].[Token]                    toke
					,       [dbo].vw_mantida                 mant
					where   cons.idConsist                   = erro.idConsist
					and     cons.numeroAES                   = toke.AES
					and     cons.numeroAES                   = mant.numero_aes
					and     cons.itemAES                     = mant.Item_aes
					and     toke.AES                         = '{0}'
					and     erro.idConsistItem not in  ( select idConsistItem from Aluno4EdicaoConsistCorrecao )  

					group by 
										mant.Mantenedora
								,       mant.Numero_AES
								,       mant.Item_AES
								,       mant.Mantida
								,       mant.Diretoria_Ensino
								,       mant.NomeCurso
								,       mant.vagas
								,       mant.cargaHoraEstagio
								,       mant.cargaHoraTotal
								,       mant.area
								,       toke.AES
								,       isnull(toke.QtdErros,0)
								,       cons.idConsist
								,       cons.numeroAES
								,       cons.itemAES
								,       cons.IdMatricula
								,       cons.IdInscricao
								,       cons.RAGDAE
								,       cons.NomeAlunoVence
								,       cons.NomeMaeVence
								,       cons.dataNascimentoVence
								,       cons.RGAlunoVence
								,       cons.NomeAlunoGDAE
								,       cons.NomeMaeGDAE
								,       cons.dataNascimentoGDAE
								,       cons.RGAlunoGDAE
								,       erro.ERCod
								,       erro.ERConsist
								,		erro.idConsistItem  ", numeroAES), conn);

            cmd.Connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                _inconsistenciaViewModel.Diretoria_Ensino = reader["Diretoria_Ensino"].ToString();
                lista.Add(new Inconsistencia
                {

                    Mantida = reader["Mantida"].ToString(),
                    RAGDAE = reader["RAGDAE"].ToString(),
                    RGAlunoGDAE = reader["RGAlunoGDAE"].ToString(),
                    RGAlunoVence = reader["RGAlunoVence"].ToString(),
                    NomeAlunoGDAE = reader["NomeAlunoGDAE"].ToString(),
                    NomeAlunoVence = reader["NomeAlunoVence"].ToString(),
                    NomeMaeGDAE = reader["NomeMaeGDAE"].ToString(),
                    NomeMaeVence = reader["NomeMaeVence"].ToString(),
                    DataNascimentoGDAE = reader["dataNascimentoGDAE"].ToString(),
                    DataNascimentoVence = reader["dataNascimentoVence"].ToString(),
                    ERConsist = reader["ERConsist"].ToString(),
                    NomeCurso = reader["NomeCurso"].ToString(),
                    TotalEstagioContratado = reader["cargaHoraEstagio"] != DBNull.Value ? Convert.ToInt32(reader["cargaHoraEstagio"]) : 0,
                    CargaHoraTotal = reader["cargaHoraTotal"] != DBNull.Value ? Convert.ToInt32(reader["cargaHoraTotal"]) : 0,
                    ERCod = reader["ERCod"] != DBNull.Value ? Convert.ToInt32(reader["ERCod"]) : 0,
                    idConsist = reader["idConsist"] != DBNull.Value ? Convert.ToInt32(reader["idConsist"]) : 0,
                    IdConsistItem = reader["idConsistItem"] != DBNull.Value ? Convert.ToInt32(reader["idConsistItem"]) : 0,
                });

            }
            _inconsistenciaViewModel.ListarInconsistencias = lista;
            return _inconsistenciaViewModel;
        }

        private void SalvarLog(string log) 
        {
            try
            {
                string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
                SqlConnection conn = new SqlConnection(_connectionString);
                string sql = string.Format(@" INSERT INTO [dbo].[Aluno4EdicaoConsistCorrecaoLog]
                                           ([LogCorrecao]
                                           ,[Data])
                                     VALUES
                                           (
		                                    '{0}'
                                           ,GETDATE()
		                                   ) ", log);


                SqlCommand cmd = new SqlCommand(sql, conn);

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
            catch (Exception)
            {
                
         
            }
           
        
        }

    }

    public class Inconsistencia
    {
        public int idConsist { get; set; }
        public string Mantida { get; set; }

        public string NomeCurso { get; set; }

        public int TotalEstagioContratado { get; set; }

        public int TotalEstagioHP { get; set; }
        public int TotalEstagioVENCE { get; set; }

        public int TotalEstagioLancado { get; set; }

        public string RAGDAE { get; set; }

        public string NomeAlunoVence { get; set; }

        public string NomeMaeVence { get; set; }

        public string DataNascimentoVence { get; set; }

        public string RGAlunoVence { get; set; }

        public string NomeAlunoGDAE { get; set; }

        public string NomeMaeGDAE { get; set; }

        public string DataNascimentoGDAE { get; set; }

        public string RGAlunoGDAE { get; set; }

        public int ERCod { get; set; }

        public string ERConsist { get; set; }

        public int IdConsistItem { get; set; }

        public TipoInconsistencia TipoInconsistencia { get; set; }


        public int CargaHoraTotal { get; set; }
    }

    public enum TipoInconsistencia
    {
        NaoEncontrado,
        NomeAlunoNaoConfere,
        NomeMaeNaoConfere,
        DataNascimentoNaoConfere,
        EstagioUltrapassaTotal

    }

    public class InconsistenciaViewModel
    {
        public string Diretoria_Ensino { get; set; }
        public List<Inconsistencia> ListarInconsistencias { get; set; }

    }


}