using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
	public static class ExternalIDFormatter
	{
		public static string AppendPrefix(string id, string prefix)
		{
			return string.Format("{0}{1}", prefix, id);
		}

		public static string RemovePrefix(string id, string prefix)
		{
			return id.Remove(0, prefix.Length);
		}
	}
}
