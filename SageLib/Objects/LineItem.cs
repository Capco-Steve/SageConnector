using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SageLib.Objects
{
	public class LineItem
	{
		public string GLAccountID { get; set; }
		public string DepartmentID { get; set; }
		public string ClassificationID { get; set; }
		public string Description { get; set; }
		public decimal NetAmount { get; set; }
		public decimal TaxAmount { get; set; }
	}
}
