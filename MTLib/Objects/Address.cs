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
		string name { get; set; }				// address name
		string address1 { get; set; }			// Address line 1
		string address2 { get; set; }			// Address line 2
		string address3 { get; set; }			// Address line 3
		string address4 { get; set; }			// Address line 4
		string address5 { get; set; }			// Address line 5
		string postalCode { get; set; }			// Postal or Zipcode
		string town { get; set; }				// City or Town
		string ctrySubDivision { get; set; }	// State/Region
		string country { get; set; }            // Country
	}
}
