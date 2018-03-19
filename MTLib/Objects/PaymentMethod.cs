using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTLib.Objects
{
	public class PaymentMethod
	{
		public string id { get; set; } = "";
		public string type { get; set; } = "";
		public string externalId { get; set; } = "";
		public bool active { get; set; }
		public BankAccount bankAccount { get; set; }
	}
}
