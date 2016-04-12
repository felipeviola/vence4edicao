using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vence.Input4Edicao.Models
{
    public class RelatAcompMensalVM
    {
        public string NumeroAES { get; set; }

        public List<int> ItemAES { get; set; }

        public List<VwAcompMensal> ListaAcompanhamentos { get; set; }
    }
}