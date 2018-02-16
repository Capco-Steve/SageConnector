using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace SageLib
{
    /// <summary>
	/// Interface to the AppSettings section in App.Config
	/// </summary>
	public static class SageSettings
    {
        public static string BaseUrl
        {
            get { return ConfigurationManager.AppSettings["BaseUrl"]; }
        }
    }
}
