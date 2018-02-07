using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace MTLib
{
	/// <summary>
	/// Interface to the AppSettings section in App.Config
	/// </summary>
	public static class MTSettings
	{
		public static string BaseUrl
		{
			get{ return ConfigurationManager.AppSettings["BaseUrl"];}
		}

		public static string AuthenticationUrl
		{
			get { return ConfigurationManager.AppSettings["AuthenticationUrl"]; }
		}

		public static string VendorUrl
		{
			get { return ConfigurationManager.AppSettings["VendorUrl"]; }
		}

		public static string UserUrl
		{
			get { return ConfigurationManager.AppSettings["UserUrl"]; }
		}

		public static string UserCompaniesUrl
		{
			get { return ConfigurationManager.AppSettings["UserCompaniesUrl"]; }
		}
	}
}
