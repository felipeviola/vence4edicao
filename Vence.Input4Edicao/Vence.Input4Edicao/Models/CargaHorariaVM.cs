using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vence.Input4Edicao.Models
{
    public class CargaHorariaVM
    {
        public string NumeroAES { get; set; }

        public int ItemAES { get; set; }

        public string Chave { get; set; }

        public List<VwAcompCargaHoraria> ListaCargaHoraria { get; set; }
    }
}