using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Vence.Input4Edicao.Controllers
{
    public class SEEController : Controller
    {
        // GET: SEE
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult ValidarToken(string token)
        {
            try
            {
                if (token == "")
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(true, JsonRequestBehavior.AllowGet);

                }

            }
            catch (Exception)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult Update(string token, string chave)
        {


            try
            {
                if (token == "" || chave == "")
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(true, JsonRequestBehavior.AllowGet);

                }

            }
            catch (Exception)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }
    }
}