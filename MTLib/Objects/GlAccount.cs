using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTLib.Objects
{
	public class GlAccount
	{
		public string id { get; set; }
		public List<Subsidiary> subsidiaries { get; set; }
		public bool departmentRequired { get; set; }
		public bool locationRequired { get; set; }
		public bool projectRequired { get; set; }
		public bool customerRequired { get; set; }
		public bool vendorRequired { get; set; }
		public bool employeeRequired { get; set; }
		public bool itemRequired { get; set; }
		public bool classRequired { get; set; }
		public string ledgerType { get; set; }
		public string accountNumber { get; set; }
		public string externalId { get; set; }
		public string name { get; set; }
		public bool active { get; set; }
	}

}
