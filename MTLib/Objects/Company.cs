using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTLib.Objects
{
	public class Company
	{
		public object modified { get; set; }
		public DateTime created { get; set; }
		public string id { get; set; }
		public object externalId { get; set; }
		public object memo { get; set; }
		public object description { get; set; }
		public string name { get; set; }
		public object legalName { get; set; }
		public string dBAName { get; set; }
		public object vendorType { get; set; }
		public object emails { get; set; }
		public object parent { get; set; }
		public object externalParentId { get; set; }
		public Address address { get; set; }
		public List<Phone> phones { get; set; }
		public object subsidiaries { get; set; }
		public List<object> paymentMethods { get; set; }

	}
}
