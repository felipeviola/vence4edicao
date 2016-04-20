using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using Vence.Input4Edicao;

namespace Vence
{
    public class DbHelper
    {
        private string _strConn = "";
        private SqlConnection _dbConn;
        private List<SqlParameter> _parameters;

        public DbHelper()
        {
            this._strConn = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionStringSqlVence"].ConnectionString;
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

            return dr;
        }

        public T GetExecuteScalar<T>(string cmdText)
        {
            T valor = default(T);

            try
            {
                this.OpenDbConnection();
                SqlCommand cmd = new SqlCommand(cmdText, this._dbConn);
                if (this._parameters.Count > 0)
                {
                    foreach (var parameter in this._parameters)
                        cmd.Parameters.Add(parameter);
                }

                var result = cmd.ExecuteScalar();
                if (result == null)
                    return default(T);

                valor = (T)Convert.ChangeType(result, typeof(T));
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return valor;
        }
    }
}