using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTLib.Objects
{
	public class PurchaseOrder
	{
		public string externalId { get; set; }
		public ObjID vendor { get; set; }
		public ObjID classification { get; set; }
		public ObjID department { get; set; }
		public ObjID location { get; set; }
		public ObjID subsidiary { get; set; }
		public ObjID terms { get; set; }
		public string dueDate { get; set; }
		public string poNumber { get; set; }
		public string memo { get; set; }
		public string state { get; set; }
		public string poType { get; set; }
		public List<Expenses> expenses { get; set; }
		public List<PurchaseOrderItem> items { get; set; }
	}

}
