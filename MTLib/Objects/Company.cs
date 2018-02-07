using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTLib.Objects
{
	public class Company
	{
		public string modified { get; set; }
		public string id { get; set; }
		public object memo { get; set; }
		public string name { get; set; }
		public string dBAName { get; set; }
		public Address address { get; set; }
	}
}
