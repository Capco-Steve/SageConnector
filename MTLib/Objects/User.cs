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
		public string id { get; set; }
		public string name { get; set; }
		public List<string> emails { get; set; }
	}

}
