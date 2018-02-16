using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTLib.Objects
{
	public class UserRoot
	{
		public User user{get;set;}
		public List<Company> companies { get; set; }
	}
}
