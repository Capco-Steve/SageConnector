using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Models;
using Web.Code;

namespace Web.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Login(string Username, string Password)
		{
			SessionStateManager.LogIn(); // TODO - REMOVE!!
			if (Username != null && Password != null)
			{
				if(Username == "steve" && Password == "test")
				{
					SessionStateManager.LogIn();
				}
			}

			return View("Index");
		}

		public ActionResult About()
		{
			ViewBag.Message = "Your application description page.";

			return View();
		}

		public ActionResult Contact()
		{
			ViewBag.Message = "Your contact page.";

			return View();
		}
	}
}