using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vence.Input4Edicao.Models;

namespace Vence.Input4Edicao.Models
{
    public class ControleEstagioVM
    {
        public string NumeroAES { get; set; }

        public int ItemAES { get; set; }

        public string Chave { get; set; }

        public int CargaHorariaEstagio { get; set; }

        public List<VwEstagio> ListaEstagios { get; set; }
    }
}