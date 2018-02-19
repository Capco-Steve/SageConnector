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
		private static List<VendorRoot> Vendors = null;
		private static List<Department> Departments = null;
		// TODO: ADD CLASSIFICATION, DEPARTMENT, LOCATION, ETC

		public static bool LoadReferenceData(string companyid, string sessiontoken)
		{
			Vendors = MTApi.GetVendorByCompanyID(companyid, sessiontoken);

			return true;
		}

		public static VendorRoot FindVendorByExternalID(string externalid)
		{
			return Vendors.Find(o => o.vendor.externalId == externalid);
		}
	}
}