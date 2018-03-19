using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTLib.Objects
{
	public class BankAccount
	{
		public string name { get; set; } = "";
		public string accountNumber { get; set; } = "";
		public string routingNumber { get; set; } = "";
		public AccountBalance accountBalance { get; set; }
	}
}
