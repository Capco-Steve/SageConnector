using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTLib.Objects
{
	public class Classification
	{
		public string id { get; set; } = null;
		public List<Subsidiary> subsidiaries { get; set; } = null;
		public string externalId { get; set; } = null;
		public string name { get; set; } = null;
		public bool active { get; set; }
	}
}
