using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTLib.Objects
{
	/// <summary>
	/// Mineral Tree Address Object - property names match Mineral Tree JSON
	/// </summary>
	public class Address
	{
		public string name { get; set; } = "";
		public string address1 { get; set; } = "";
		public string address2 { get; set; } = "";
		public string address3 { get; set; } = "";
		public string address4 { get; set; } = "";
		public string address5 { get; set; } = "";
		public string postalCode { get; set; } = "";
		public string town { get; set; } = "";
		public string ctrySubDivision { get; set; } = "";
		public string country { get; set; } = "";
	}
}
