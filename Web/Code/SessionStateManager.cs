using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Code
{
	public static class SessionStateManager
	{

		public static void LogIn()
		{
			HttpContext.Current.Session["CurrentUser"] = 1;
		}

		public static void LogOut()
		{
			HttpContext.Current.Session.Remove("CurrentUser");
		}

		public static bool IsLoggedIn()
		{
			if (HttpContext.Current.Session["CurrentUser"] == null)
				return false;
			else
				return true;
		}

	}
}