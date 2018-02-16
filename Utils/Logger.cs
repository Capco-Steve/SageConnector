using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Configuration;

/*
	Provides file based application error logging.
	Requires LogFilePath app settings value from web/app config file
*/

namespace Utils
{
    public static class Logger
    {
		private static ReaderWriterLock RWLFileLock = new ReaderWriterLock();
		private static string Path = ConfigurationManager.AppSettings["LogFilePath"];

		static Logger()
		{
		}

		public static void WriteLog(Exception e)
		{
			RWLFileLock.AcquireWriterLock(2000);
			try
			{
				StreamWriter sw = new StreamWriter(Path, true);
				StringBuilder sb = new StringBuilder(1000);
				sb.AppendFormat("{0}: {1}\r\nStack Trace: {2}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"), e.Message, e.StackTrace);
				sw.WriteLine(sb.ToString());
				sw.Close();
			}
			catch (Exception ex)
			{
				string msg = ex.Message;
			}
			finally
			{
				RWLFileLock.ReleaseWriterLock();
			}
		}

		public static void WriteLog(string text)
		{
			RWLFileLock.AcquireWriterLock(2000);
			try
			{
				StreamWriter sw = new StreamWriter(Path, true);
				StringBuilder sb = new StringBuilder(1000);
				sb.AppendFormat("{0}: {1}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"), text);
				sw.WriteLine(sb.ToString());
				sw.Close();
			}
			catch (Exception ex)
			{
				string msg = ex.Message;
			}
			finally
			{
				RWLFileLock.ReleaseWriterLock();
			}
		}

    }
}
