using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTLib.Objects
{
	public class PurchaseOrderItem
	{
		//public ObjID companyItem { get; set; }
		//public ObjID classification { get; set; }
		//public ObjID department { get; set; }
		//public ObjID location { get; set; }
		//public ObjID glAccount { get; set; }
		public string name { get; set; }
		public Quantity quantity { get; set; }
		public Quantity quantityReceived { get; set; }
		public Quantity billedQuantity { get; set; }
		public Cost cost { get; set; }
		public Amount amountDue { get; set; }
		public int lineNumber { get; set; }
		public bool closed { get; set; }
		public string description { get; set; }
		public string poItemStatus { get; set; }
	}
}
