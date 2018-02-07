using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace SageConnector
{
	public static class SCSettings
	{
		public static string Username
		{
			get { return ConfigurationManager.AppSettings["Username"]; }
		}

		public static string Password
		{
			get { return ConfigurationManager.AppSettings["Password"]; }
		}
	}
}
