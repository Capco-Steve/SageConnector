using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTLib.Objects
{
	public class BillCredit
	{
		public string transactionDate { get; set; }
		public ObjID bill { get; set; }
		public ObjID credit { get; set; }
		public Amount amountApplied { get; set; }
	}

}
