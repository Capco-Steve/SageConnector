using System;
using System.IO;
using System.Threading;
using System.Web;
using System.Text;
using System.Configuration;

namespace Utils
{
	public class ActivityLogger
	{
		private static string _LogFilePath;

		static ActivityLogger()
		{
			_LogFilePath = SetLogFilePath();
		}

		public static void WriteLog(string str)
		{
			try
			{
				StreamWriter sw = new StreamWriter(_LogFilePath, true);
				DateTime dt = DateTime.Now;
				str = dt.ToString("dd/MM/yyyy HH:mm:ss: ") + str;
				sw.WriteLine(str);
				sw.Flush();
				sw.Close();
			}
			catch (Exception ex)
			{
			}
		}

		public static string GetLogFilePath()
		{
			return _LogFilePath;
		}

		private static string SetLogFilePath()
		{
			DateTime dtnow = DateTime.Now;
			return string.Format("{0}\\SCActivityLog_{1}_{2}_{3}_{4}_{5}.log", ConfigurationManager.AppSettings["ActivityLogFileDirectory"], dtnow.Year, dtnow.Month, dtnow.Day, dtnow.Hour, dtnow.Minute);
		}
	}
}
