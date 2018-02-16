using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTLib.Objects
{
	public class Department
	{
		public string id { get; set; }
		public List<Subsidiary> subsidiaries { get; set; }
		public string externalId { get; set; }
		public string name { get; set; }
		public bool active { get; set; }
	}

}
