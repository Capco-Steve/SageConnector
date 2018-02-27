using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTLib.Objects
{
	public class Credit
	{
		public string id { get; set; }
		public string creditNumber { get; set; }
		public string transactionDate { get; set; }
		public ObjID vendor { get; set; }
		public Amount amount { get; set; }
		public string externalId { get; set; }
		public string status { get; set; }
		public string memo { get; set; }
	}
}
