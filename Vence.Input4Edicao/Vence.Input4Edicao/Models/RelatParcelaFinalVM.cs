using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vence.Input4Edicao.Models
{
    public class RelatParcelaFinalVM
    {
        public string Chave { get; set; }

        public bool SomenteParcelasPlanoContigencia { get; set; }

        public List<DadosAES> ListaAES { get; set; }
    }
}