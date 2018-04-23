using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace SageConnector
{
	public class SageConnectorSettings
	{
		public static int MinsBetweenSync
		{
			get
			{
				return Convert.ToInt32(ConfigurationManager.AppSettings["MinsBetweenSync"]);
			}
		}
	}
}
