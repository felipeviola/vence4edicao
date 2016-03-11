using System;
using System.Collections.Generic;
using System.Linq;
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

        public PartialViewResult ValidarToken(string token)
        {
            var alunos = new Alunos();
            var listaAlunos = new List<Alunos>();

            return PartialView("partialAlunos", listaAlunos);
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

        public List<Alunos> ListarAlunos { get; set; }
    }
}