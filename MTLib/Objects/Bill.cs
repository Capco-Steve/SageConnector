using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTLib.Objects
{
	public class Bill
	{
		public string id { get; set; } = "";
		public string externalId { get; set; } = "";
		public ObjID term { get; set; } = null;
		public ObjID classification { get; set; } = null;
		public Department department { get; set; } = null;
		public Location location { get; set; } = null;
		public Subsidiary subsidiary { get; set; } = null;
		public string dueDate { get; set; } = null;
		public string transactionDate { get; set; } = null;
		public string invoiceNumber { get; set; } = null;
		public Amount amount { get; set; } = null;
		public Amount netAmount { get; set; } = null;
		public Amount balance { get; set; } = null;
		public Amount totalTaxAmount { get; set; } = null;
		public Amount appliedPaymentAmount { get; set; } = null;
		public string memo { get; set; } = null;
		public string poNumber { get; set; } = null;
		public string state { get; set; } = null;
		//public string status { get; set; } = null;
		public ObjID vendor { get; set; } = null;
		public List<Expenses> expenses { get; set; } = null;
		public List<CompanyItem> items { get; set; } = null;
	}

}
