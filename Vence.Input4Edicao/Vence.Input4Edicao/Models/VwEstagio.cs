using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Vence.Input4Edicao.Models
{
    public class VwEstagio
    {
        public string Token { get; set; }

        public string NumeroAES { get; set; }

        public int ItemAES { get; set; }

        public string NomeMantenedora { get; set; }

        public string NomeMantida { get; set; }

        public int IdMantida { get; set; }

        public string NomeDiretoria { get; set; }

        public int IdCurso { get; set; }

        public string NomeCurso { get; set; }

        public int NumeroVagas { get; set; }

        public decimal ValorHoraAula { get; set;}

        public decimal ValorAluno { get; set; }

        public string Area { get; set; }

        public int IdMatricula { get; set; }

        public string NumeroRA { get; set; }

        public string NomeAluno { get; set; }

        public int CargaHorariaEstagio { get; set; }

        public int TotalHoraEstagio { get; set; }
    }
}