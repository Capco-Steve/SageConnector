using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTLib.Objects
{
	public class Vendor
	{
		public string id { get; set; }
		public bool form1099Enabled { get; set; }
		public string externalId { get; set; }
		public string name { get; set; }
		public bool active { get; set; }
		public Address address { get; set; }
		public string legalName { get; set; }
		public string vendorType { get; set; }
		public List<Phone> phones { get; set; }
		public List<FundingMethod> fundingMethods { get; set; }
		public List<string> emails { get; set; }
		public List<string> remittanceEmails { get; set; }
		public bool remittanceEnabled { get; set; }
		public string memo { get; set; }
		public string customerAccount { get; set; }
		public string taxId { get; set; }
		public string vatNumber { get; set; }
		//public VendorCompanyDefault vendorCompanyDefault { get; set; }
	}
}
