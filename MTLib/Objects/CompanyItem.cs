using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTLib.Objects
{
	public class CompanyItem
	{
		public string id { get; set; }
		public GlAccount glAccount { get; set; }
		public Classification classification { get; set; }
		public Department department { get; set; }
		public List<Subsidiary> subsidiaries { get; set; }
		public Amount amountDue { get; set; }
		public Amount netAmount { get; set; }
		public Amount taxAmount { get; set; }
		public string type { get; set; }
		public Cost cost { get; set; }
		public Residual residual { get; set; }
		public string name { get; set; }
		public bool active { get; set; }
		public string externalId { get; set; }
		public string description { get; set; }
	}
}
