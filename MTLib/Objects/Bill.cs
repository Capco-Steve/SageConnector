using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTLib.Objects
{
	public class Bill
	{
		public string id { get; set; }
		public string externalId { get; set; }
		public Term term { get; set; }
		public ObjID classification { get; set; }
		public Department department { get; set; }
		public Location location { get; set; }
		public Subsidiary subsidiary { get; set; }
		public string dueDate { get; set; }
		public string transactionDate { get; set; }
		public string invoiceNumber { get; set; }
		public Amount amount { get; set; }
		public Amount balance { get; set; }
		public Amount totalTaxAmount { get; set; }
		public string memo { get; set; }
		public string poNumber { get; set; }
		public string state { get; set; }
		public ObjID vendor { get; set; }
		public List<Expenses> expenses { get; set; }
		public List<Item> items { get; set; }
	}

}
