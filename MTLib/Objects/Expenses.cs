using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTLib.Objects
{
	public class Expenses
	{
		public ObjID classification { get; set; }
		public ObjID department { get; set; }
		public ObjID location { get; set; }
		public ObjID glAccount { get; set; }
		public Amount amountDue { get; set; }
		public int lineNumber { get; set; }
		public bool closed { get; set; }
		public string memo { get; set; }

	}
}
