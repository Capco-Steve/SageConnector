using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTLib.Objects
{
	public class SearchQuery
	{
		public string view { get; set; }
		public string query { get; set; }
		public int page { get; set; }
		public int count { get; set; }
		public string sortField { get; set; }
		public bool sortAsc { get; set; }

	}
}
