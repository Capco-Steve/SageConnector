using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace SyncLib
{
	/// <summary>
	/// Interface to the AppSettings section in App.Config
	/// </summary>
	public static class SyncSettings
	{
		public static string CompanyNameToSync
		{
			get { return ConfigurationManager.AppSettings["CompanyNameToSync"]; }
		}

		public static string ActivityLogFileDirectory
		{
			get { return ConfigurationManager.AppSettings["ActivityLogFileDirectory"]; }
		}
	}
}
