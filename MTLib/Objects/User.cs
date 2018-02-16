using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTLib.Objects
{
	public class User
	{
		public string modified { get; set; }
		public string created { get; set; }
		public string id { get; set; }
		public string externalId { get; set; }
		public string memo { get; set; }
		public string description { get; set; }
		public string name { get; set; }
		public string legalName { get; set; }
		public string dBAName { get; set; }
		public string vendorType { get; set; }
		public List<string> emails { get; set; }
		public string address { get; set; }
		public string phones { get; set; }
	}
}
