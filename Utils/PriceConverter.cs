using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
	/// <summary>
	/// Converts to/from MT values - they only store whole numbers so its 10 to the power of precision, and then multiply or divide
	/// </summary>
	public static class PriceConverter
	{
		public static long FromDecimal(decimal value, int precision)
		{
			if (value > 0)
			{
				double pow = Math.Pow(10.0, precision);
				double v1 = Math.Round((double)value * (int)pow);
				return (long)v1;
			}
			else
			{
				return 0;
			}
		}

		public static decimal ToDecimal(long value, int precision)
		{
			if (value > 0)
			{
				double pow = Math.Pow(10.0, precision);
				double v1 = (value / pow);
				return (decimal)(value / pow);
			}
			else
			{
				return 0;
			}
		}
	}
}
