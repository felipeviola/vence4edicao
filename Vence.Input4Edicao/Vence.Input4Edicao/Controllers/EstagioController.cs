using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Data;
using Vence.Input4Edicao.Models;
using System.IO;
using System.ComponentModel;
using System.Reflection;


namespace Vence.Input4Edicao.Controllers
{
    public class EstagioController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public PartialViewResult PesquisarEstagiosPorToken(string chave)
        {
            var model = new ControleEstagioVM();
            model.ListaEstagios = ObterListaEstagioPorToken(chave);
            return PartialView("_PartialViewEstagios", model);
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

            Session["dr"] = dr;

            db.CloseDbConnection();

            return estagios;
        }

        public FileResult GerarArquivosExcel(string chave)
        {
            DataSet ds = this.ObterDataSetEstagio(chave);
            MemoryStream stream = new MemoryStream();
            ExcelLibrary.DataSetHelper.CreateWorkbook(stream, ds);
            byte[] file = stream.ToArray();
            return File(file, ReportFormat.XLS.GetEnumDescription(), "RelatAcompEstagio.xls");
        }
        public DataSet ObterDataSetEstagio(string chave)
        {
            DbHelper db = new DbHelper();

            string cmdText = @" select Numero_AES as AES, 
                                        Item_AES as [Item Aes], 
                                        Mantenedora,
                                        Cod_Mantida as [Codigo Mantida] , 
                                        Mantida, 
                                        Diretoria_Ensino as [Diretoria],
                                        CodCurso as [Codigo Curso], 
                                       NomCurso as [Nome do Curso], 
                                        vagas as Vagas, 
                                        valorAluno as [Valor Aluno], 
                                        valor_hora_aula as [Valor Hora Aula], 
                                        area as Aula, 
                                        --esta.idMatricula, 
                                       esta.RA,
                                        esta.NomeAluno as [Nome Aluno], 
                                        cargaHoraEstagio as [Carga Horaria Contratada], 
                                        sum(qtd_Horas_Estagio) as [Carga Horaria Realizada]
                                  from vw_estagio esta
                                  join Token      tk on esta.Numero_AES = tk.AES
                                 where tk.Chave = @Chave";

            db.AddParameter(new System.Data.SqlClient.SqlParameter("@Chave", chave));

            cmdText += @" group by Numero_AES, Item_AES, Mantenedora, Cod_Mantida, Mantida, Diretoria_Ensino, CodCurso, NomCurso, 
                                   vagas, valorAluno, valor_hora_aula, area, idMatricula, RA, NomeAluno, cargaHoraEstagio";

            DataSet ds = db.GetDataSet(cmdText);
            db.CloseDbConnection();
            return ds;
        } 
    }
    public enum ReportFormat
    {
        [Description("application/pdf")]
        PDF = 1,

        [Description("application/xls")]
        XLS = 2,

        [Description("application/rtf")]
        RTF = 3,

        [Description("application/pdf")]
        HTML = 4,

        [Description("application/csv")]
        CSV = 5,

        [Description("application/xml")]
        XML = 6,

        [Description("application/jrprint")]
        JRPRINT = 7
    }
    public static class ExtensionMethod
    {
        public static string GetEnumDescription(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());

            DescriptionAttribute attribute
                    = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
                        as DescriptionAttribute;

            return attribute == null ? value.ToString() : attribute.Description;
        }
    }
    public class DbHelper
    {
        private string _strConn = "";
        private SqlConnection _dbConn;
        private List<SqlParameter> _parameters;

        public DbHelper()
        {
            this._strConn = System.Configuration.ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            this._dbConn = new SqlConnection();
            this._parameters = new List<SqlParameter>();
        }

        public void ClearParameters()
        {
            this._parameters.Clear();
        }

        public void AddParameter(SqlParameter parameter)
        {
            this._parameters.Add(parameter);
        }

        private void OpenDbConnection()
        {
            if (this._dbConn.State == ConnectionState.Closed)
            {
                this._dbConn.ConnectionString = this._strConn;
                this._dbConn.Open();
            }
        }

        public void CloseDbConnection()
        {
            if (this._dbConn.State == ConnectionState.Open)
                this._dbConn.Close();
        }

        public DataSet GetDataSet(string cmdText)
        {
            DataSet ds = new DataSet();

            try
            {
                this.OpenDbConnection();
                SqlCommand cmd = new SqlCommand(cmdText, this._dbConn);
                if (this._parameters.Count > 0)
                {
                    foreach (var parameter in this._parameters)
                        cmd.Parameters.Add(parameter);
                }
                SqlDataAdapter ad = new SqlDataAdapter(cmd);
                ad.Fill(ds);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //this.CloseDbConnection();
            }

            return ds;
        }

        public SqlDataReader GetDataReader(string cmdText)
        {
            SqlDataReader dr;

            try
            {
                this.OpenDbConnection();
                SqlCommand cmd = new SqlCommand(cmdText, this._dbConn);
                if (this._parameters.Count > 0)
                {
                    foreach (var parameter in this._parameters)
                        cmd.Parameters.Add(parameter);
                }
                dr = cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //this.CloseDbConnection();
            }

            return dr;
        }
    }

}