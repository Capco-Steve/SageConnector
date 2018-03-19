using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTLib.Objects
{
	public class Payment
	{
		public string id { get; set; } = null;
		public FundMethod paymentMethod { get; set; } = null;
		public FundMethod fundMethod { get; set; } = null;
		public List<Bill> bills { get; set; } = null;
		public Amount amount { get; set; } = null;
		public AccountingPeriod accountingPeriod { get; set; } = null;
		public string transactionDate { get; set; } = null;
		public string externalId { get; set; } = null;
		public string checkNumber { get; set; } = null;
		public string sequenceText { get; set; } = null;
		public string status { get; set; } = null;
		public Vendor vendor { get; set; } = null;
	}

}
