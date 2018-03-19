using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Configuration;

namespace SyncLib
{
	/// <summary>
	/// Interface to the AppSettings section in App.Config
	/// </summary>
	public static class SyncSettings
	{
		public static string MTCompanyNameToSync
		{
			get { return ConfigurationManager.AppSettings["MTCompanyNameToSync"]; }
		}

		public static SageElementCollection SageCompaniesToSync
		{
			get
			{
				return (ConfigurationManager.GetSection("SageCompanyConfig") as SageCompanyConfigSection).SageCompanies;
			}
		}

		public static string ActivityLogFileDirectory
		{
			get { return ConfigurationManager.AppSettings["ActivityLogFileDirectory"]; }
		}

		public static DateTime StartDate
		{
			get { return DateTime.ParseExact(ConfigurationManager.AppSettings["StartDate"], "yyyy/MM/dd", CultureInfo.InvariantCulture);}
		}
	}
}
