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
		// MINERAL TREE REFERENCE DATA - USED WHEN SYNCING TO LOOKUP IDS. SAVES MAKING LOTS OF WEB API CALLS 
		// AT THE EXPENSE OF MEMORY USE!
		// TODO - THIS WILL HAVE TO BE UPDATED AS WE CREATE NEW OBJECTS IN MINERAL TREE
		private static List<Vendor> Vendors = null;
		private static List<Department> Departments = null;
		private static List<Item> Items = null;
		private static List<GlAccount> GlAccounts = null;
		private static List<Location> Locations = null;

		public static bool LoadReferenceData(string companyid, string sessiontoken)
		{
			Vendors = MTApi.GetVendorsByCompanyID(companyid, sessiontoken);
			Departments = MTApi.GetDepartmentsByCompanyID(companyid, sessiontoken);
			Items = MTApi.GetItemsByCompanyID(companyid, sessiontoken);
			GlAccounts = MTApi.GetGlAccountsByCompanyID(companyid, sessiontoken);
			Locations = MTApi.GetLocationsByCompanyID(companyid, sessiontoken);

			return true;
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

		#region ITEMS

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
	}
}