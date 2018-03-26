using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTLib;
using MTLib.Objects;

namespace SyncLib
{
	public class MTReferenceData
	{
		// MINERAL TREE REFERENCE DATA - USED WHEN SYNCING TO LOOKUP IDS. SAVES MAKING LOTS OF WEB API CALLS AT THE EXPENSE OF MEMORY USE!
		private static List<Vendor> Vendors = new List<Vendor>();
		private static List<Department> Departments = new List<Department>();
		private static List<Item> Items = new List<Item>();
		private static List<GlAccount> GlAccounts = new List<GlAccount>();
		private static List<Location> Locations = new List<Location>();
		private static List<PaymentMethod> PaymentMethods = new List<PaymentMethod>();

		public static void LoadReferenceData(string companyid, string sessiontoken)
		{
			Vendors = MTApi.GetVendorsByCompanyID(companyid, sessiontoken);
			Departments = MTApi.GetDepartmentsByCompanyID(companyid, sessiontoken);
			Items = MTApi.GetItemsByCompanyID(companyid, sessiontoken);
			GlAccounts = MTApi.GetGlAccountsByCompanyID(companyid, sessiontoken);
			Locations = MTApi.GetLocationsByCompanyID(companyid, sessiontoken);
			PaymentMethods = MTApi.GetPaymentMethodsByCompanyName(SyncSettings.MTCompanyNameToSync, sessiontoken);
		}

		#region VENDORS

		public static Vendor FindVendorByExternalID(string externalid)
		{
			return Vendors.Find(vendor => vendor.externalId == externalid);
		}

		public static Vendor FindVendorByID(string id)
		{
			return Vendors.Find(vendor => vendor.id == id);
		}

		public static void AddVendor(Vendor vendor)
		{
			Vendors.Add(vendor);
		}

		public static int GetVendorCount()
		{
			return Vendors.Count();
		}

		#endregion

		#region DEPARTMENTS

		public static Department FindDepartmentByExternalID(string externalid)
		{
			return Departments.Find(department => department.externalId == externalid);
		}

		public static void AddDepartment(Department department)
		{
			Departments.Add(department);
		}

		public static int GetDepartmentCount()
		{
			return Departments.Count();
		}

		#endregion

		#region ITEMS / STOCKITEMS

		public static Item FindItemByExternalID(string externalid)
		{
			return Items.Find(item => item.externalId == externalid);
		}

		public static void AddItem(Item item)
		{
			Items.Add(item);
		}

		public static int GetItemCount()
		{
			return Items.Count();
		}

		#endregion

		#region GLACCOUNTS

		public static GlAccount FindGlAccountByExternalID(string externalid)
		{
			return GlAccounts.Find(glaccount => glaccount.externalId == externalid);
		}

		public static GlAccount FindGlAccountByAccountNumber(string accountnumber)
		{
			return GlAccounts.Find(glaccount => glaccount.accountNumber == accountnumber);
		}

		public static void AddGlAccount(GlAccount glaccount)
		{
			GlAccounts.Add(glaccount);
		}

		public static int GetGlAccountCount()
		{
			return GlAccounts.Count();
		}

		#endregion

		#region LOCATIONS

		public static Location FindLocationByExternalID(string externalid)
		{
			return Locations.Find(location => location.externalId == externalid);
		}

		public static void AddLocation(Location location)
		{
			Locations.Add(location);
		}

		public static int GetLocationCount()
		{
			return Locations.Count();
		}

		#endregion

		#region PAYMENT METHODS

		public static PaymentMethod FindPaymentMethodByExternalID(string externalid)
		{
			return PaymentMethods.Find(paymentmethod => paymentmethod.externalId == externalid);
		}

		public static void AddPaymentMethod(PaymentMethod paymentmethod)
		{
			PaymentMethods.Add(paymentmethod);
		}

		public static int GetPaymentMethodCount()
		{
			return PaymentMethods.Count();
		}

		#endregion
	}
}