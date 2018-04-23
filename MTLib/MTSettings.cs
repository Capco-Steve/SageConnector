using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace MTLib
{
	/// <summary>
	/// Interface to the AppSettings section in App.Config
	/// </summary>
	public static class MTSettings
	{
		public static string MTUsername
		{
			get { return ConfigurationManager.AppSettings["MTUsername"]; }
		}

		public static string MTPassword
		{
			get { return ConfigurationManager.AppSettings["MTPassword"]; }
		}

		//public static bool LogHTTPRequests
		//{
		//	get{ return ConfigurationManager.AppSettings["LogHTTPRequests"].ToLower() == "true" ? true : false; }
		//}

		public static string BaseUrl
		{
			get{ return ConfigurationManager.AppSettings["BaseUrl"];}
		}

		public static string AuthenticationUrl
		{
			get { return ConfigurationManager.AppSettings["AuthenticationUrl"]; }
		}

		public static string VendorUrl
		{
			get { return ConfigurationManager.AppSettings["VendorUrl"]; }
		}

		public static string UserUrl
		{
			get { return ConfigurationManager.AppSettings["UserUrl"]; }
		}

		public static string UserCompaniesUrl
		{
			get { return ConfigurationManager.AppSettings["UserCompaniesUrl"]; }
		}

		public static string SearchUrl
		{
			get { return ConfigurationManager.AppSettings["SearchUrl"]; }
		}

		public static string DepartmentUrl
		{
			get { return ConfigurationManager.AppSettings["DepartmentUrl"]; }
		}

		public static string ItemUrl
		{
			get { return ConfigurationManager.AppSettings["ItemUrl"]; }
		}

		public static string GlAccountUrl
		{
			get { return ConfigurationManager.AppSettings["GlAccountUrl"]; }
		}

		public static string LocationUrl
		{
			get { return ConfigurationManager.AppSettings["LocationUrl"]; }
		}

		public static string TermUrl
		{
			get { return ConfigurationManager.AppSettings["TermUrl"]; }
		}

		public static string PaymentMethodUrl
		{
			get { return ConfigurationManager.AppSettings["PaymentMethodUrl"]; }
		}

		public static string PurchaseOrderUrl
		{
			get { return ConfigurationManager.AppSettings["PurchaseOrderUrl"]; }
		}

		public static string BillUrl
		{
			get { return ConfigurationManager.AppSettings["BillUrl"]; }
		}

		public static string PaymentUrl
		{
			get { return ConfigurationManager.AppSettings["PaymentUrl"]; }
		}

		public static string CreditUrl
		{
			get { return ConfigurationManager.AppSettings["CreditUrl"]; }
		}

		public static string BillCreditUrl
		{
			get { return ConfigurationManager.AppSettings["BillCreditUrl"]; }
		}

		public static string ClassificationUrl
		{
			get { return ConfigurationManager.AppSettings["ClassificationUrl"]; }
		}
	}
}
