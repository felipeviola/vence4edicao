using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.IO;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Reflection;


namespace Vence
{
    public class Report
    {
        #region [ Member Variables ]

        private string _urlServer = string.Empty;
        private string _userId = string.Empty;
        private string _userPassword = string.Empty;
        private WebClient _client = null;
        private string _session = string.Empty;
        private JasperServerEdition _edition;

        #endregion

        #region [ Constructors ]

        public Report()
        {
            this.InitializeAppSettings();
            this.InitializeSession();
        }

        #endregion

        #region [ Private Methods ]

        private void InitializeAppSettings()
        {
            // Obtém os dados de configuração para acesso ao JasperServer.
            this._urlServer = System.Configuration.ConfigurationSettings.AppSettings["JasperServerUrl"];
            this._userId = System.Configuration.ConfigurationSettings.AppSettings["JasperServerUserId"];
            this._userPassword = System.Configuration.ConfigurationSettings.AppSettings["JasperServerUserPwd"];
            this._edition = (JasperServerEdition)Enum.Parse(typeof(JasperServerEdition), System.Configuration.ConfigurationSettings.AppSettings["JasperServerEdition"], true);
            if (this._edition == JasperServerEdition.Enterprise)
                this._urlServer += "-pro";
        }

        private void InitializeSession()
        {
            // Define uma instância de WebClient.
            this._client = new WebClient();
            this._client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

            // Define os parâmetros de autêticação no jasperserver.
            var authenticationParams = new NameValueCollection();
            authenticationParams.Add("j_username", this._userId);
            authenticationParams.Add("j_password", this._userPassword);

            // Realiza a autênticação via post rest_v1.
            this._client.UploadValues(this._urlServer + "/rest/login", "POST", authenticationParams);

            // Recupera o cookie da sessão.
            this._session = this._client.ResponseHeaders.Get("Set-Cookie");
            this._client.Headers.Add("Cookie", this._session);
        }

        private string GetUrlParameters(List<ReportParameter> parameters)
        {
            string urlParams = "";
            foreach (var param in parameters)
            {
                urlParams += param.Name + "=" + param.Value + "&";
            }
            return urlParams.Remove(urlParams.Length - 1, 1);
        }

        #endregion

        #region [ Public Methods ]

        public byte[] GetReportFile(string reportName, string reportFoldersPath, ReportFormat reportFormat, List<ReportParameter> parameters)
        {
            byte[] file = null;

            try
            {
                reportName += "." + reportFormat.ToString().ToLower();
                var report = String.Format("rest_v2/reports/reports{0}/{1}", reportFoldersPath, reportName);

                // Recupera o arquivo no formato byte array.
                if (parameters.Count > 0)
                {
                    var urlParams = this.GetUrlParameters(parameters);
                    file = this._client.DownloadData(String.Format("{0}/{1}?{2}", this._urlServer, report, urlParams));
                }
                else
                {
                    file = this._client.DownloadData(String.Format("{0}/{1}", this._urlServer, report));
                }

                return file;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }

    #region [ Helpers ]

    public enum JasperServerEdition
    {
        Community = 1,

        Enterprise = 2
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

    public class ReportParameter
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    #endregion

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
}