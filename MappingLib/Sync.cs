using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTLib;
using MTLib.Objects;
using SageLib;
using SageSupplier = Sage.Accounting.PurchaseLedger.Supplier;
using Bank = Sage.Accounting.CashBook.Bank;
using SageDepartment = Sage.Accounting.SystemManager.Department;
using StockItem = Sage.Accounting.Stock.StockItem;
using NominalCode = Sage.Accounting.NominalLedger.NominalCode;
using CostCentre = Sage.Accounting.SystemManager.CostCentre;
using POPOrder = Sage.Accounting.POP.POPOrder;

namespace SyncLib
{
	/// <summary>
	/// Performs object sychronisation
	/// </summary>
    public static class Sync
    {
		public static event EventHandler<SyncEventArgs> OnProgress;
		public static event EventHandler<SyncEventArgs> OnComplete;

		private static int Errors = 0;

		public static void SyncAll()
		{
			Progress("Start Sync");
			Errors = 0;
			string sessiontoken = MTApi.GetSessionToken();
			if(sessiontoken.Length > 0)
			{
				Progress("Got Session Token OK");
			}
			else
			{
				Progress("Failed to get Session Token - stopping");
				Complete("Sync Failed");
				return;
			}

			List<Company> companies = MTApi.GetCompaniesForCurrentUser(sessiontoken);
			if(companies != null)
			{
				Progress(string.Format("Got {0} Companies", companies.Count()));
				foreach(Company company in companies)
				{
					Progress(company.name);
				}
			}
			else
			{
				Progress("Failed to get companies - stopping");
				Complete("Sync Failed");
				return;
			}

			Progress(string.Format("Finding company to sync to: {0}", SyncSettings.CompanyNameToSync));
			Company found = companies.Find(o => o.name == SyncSettings.CompanyNameToSync);
			if(found == null)
			{
				Progress(string.Format("Could not locate company to sync - check app settings - stopping"));
				Complete("Sync Failed");
				return;
			}

			string companyid = found.id;

			Progress("Loading Reference Data");

			// MAYBE SHOULD DO THIS AFTER THE VENDOR, REFERENCE DATA SYNC SO WE PICK UP THE LATEST DATA???
			MTReferenceData.LoadReferenceData(companyid, sessiontoken);

			Progress(string.Format("Syncing to Mineral Tree Company: {0}", found.name));

			if(SageApi.Connect())
			{
				Progress("Connected to Sage OK");
			}
			else
			{
				Progress("Failed to connect to Sage - stopping");
				Complete("Sync Failed");
				return;
			}

			//SageSuppliersToMineralTreeVendors(companyid, sessiontoken);
			//SageBankAccountsToMineralTreePaymentMethods(companyid, sessiontoken);
			//SageDepartmentsToMineralTreeDepartments(companyid, sessiontoken);
			//SageStockItemsToMineralTreeItems(companyid, sessiontoken);
			//SageNominalCodesToMineralTreeGLAccounts(companyid, sessiontoken);
			//SageCostCentresToMineralTreeLocations(companyid, sessiontoken);
			//SagePaymentTermsToMineralTreeTerms(companyid, sessiontoken);

			SagePurchaseOrdersToMineralTreePurchaseOrders(companyid, sessiontoken);
			// TODO, INVOICES, PAYMENTS

			Progress(string.Format("End Sync - {0} error(s)", Errors));
			return;
		}

		#region EVENTS

		private static void Progress(string message)
		{
			if(OnProgress != null)
			{
				OnProgress(null, new SyncEventArgs() { Message = message });
			}
		}

		private static void Complete(string message)
		{
			if (OnComplete != null)
			{
				OnComplete(null, new SyncEventArgs() { Message = message });
			}
		}

		#endregion

		#region VENDOR

		public static bool SageSuppliersToMineralTreeVendors(string companyid, string sessiontoken)
		{
			Progress("Start Syncing Suppliers...");
			// GET ALL THE SAGE SUPPLIERS AND CREATE A LIST OF CORRESPONDING VENDORS TO SYNC WITH MINERAL TREE
			List<SageSupplier> suppliers = SageApi.GetSuppliers();
			Progress(string.Format("Loaded {0} Suppliers from Sage", suppliers.Count()));

			// GET ALL THE VENDORS ALREADY LOADED INTO MINERAL TREE
			List<VendorRoot> vendors = MTApi.GetVendorByCompanyID(companyid, sessiontoken);
			Progress(string.Format("Loaded {0} existing vendors from Mineral Tree", vendors.Count()));

			foreach (SageSupplier supplier in suppliers)
			{
				// DOES THE VENDOR ALREADY EXIST? SAGE SOURCE REFERENCE == MT EXTERNAL ID
				VendorRoot found = vendors.Find(o => o.vendor.externalId == supplier.SourceReference);
				if (found != null)
				{
					// UPDATE -- NOT CURRENTLY WORKING - MT API FAILS WITH PUT REQUEST
					VendorRoot vendorroot = Mapper.SageSupplierToMTVendor(supplier);
					vendorroot.vendor.id = found.vendor.id; // MAY NOT BE NEEDED
					Vendor result = MTApi.UpdateVendor(found.vendor.id, vendorroot, sessiontoken);
					if (result == null)
					{
						Progress(string.Format("Supplier: {0} already exists - updating...Failed", supplier.Name));
						Errors++;
					}
					else
					{
						Progress(string.Format("Supplier: {0} already exists - updating...Success", supplier.Name));
					}

				}
				else
				{
					// CREATE
					VendorRoot vendorroot = Mapper.SageSupplierToMTVendor(supplier);
					Vendor result = MTApi.CreateVendor(companyid, vendorroot, sessiontoken);
					Progress(string.Format("Supplier {0} already exists - creating...{1}", supplier.Name, result == null ? "Failed" : "Success"));
				}
			}

			Progress("Finish Syncing Suppliers...");
			return true;
		}

		#endregion

		#region BANK ACCOUNTS

		public static bool SageBankAccountsToMineralTreePaymentMethods(string companyid, string sessiontoken)
		{
			// GET ALL THE SAGE BANK ACCOUNTS TO SYNC WITH MINERAL TREE
			List<Bank> banks = SageApi.GetBanks();

			foreach(Bank bank in banks)
			{
				PaymentMethodRoot paymentmethodroot = Mapper.SageBankAccountToMTPaymentMethod(bank);
			
				MTApi.CreatePaymentMethod(companyid, paymentmethodroot, sessiontoken);
			}

			return true;
		}

		#endregion

		#region DEPARTMENTS

		public static bool SageDepartmentsToMineralTreeDepartments(string companyid, string sessiontoken)
		{
			// GET ALL THE SAGE DEPARTMENTS TO SYNC WITH MINERAL TREE
			List<SageDepartment> departments = SageApi.GetDepartments();

			foreach (SageDepartment department in departments)
			{
				DepartmentRoot departmentroot = Mapper.SageDepartmentToMTDepartment(department);
				MTApi.CreateDepartment(companyid, departmentroot, sessiontoken);
			}

			return true;
		}

		#endregion

		#region ITEMS

		public static bool SageStockItemsToMineralTreeItems(string companyid, string sessiontoken)
		{
			// GET ALL THE SAGE STOCKITEMS TO SYNC WITH MINERAL TREE
			List<StockItem> stockitems = SageApi.GetStockItems();

			foreach (StockItem stockitem in stockitems)
			{
				ItemRoot itemroot = Mapper.SageStockItemToMTItem(stockitem);

				MTApi.CreateItem(companyid, itemroot, sessiontoken);
			}

			return true;
		}

		#endregion

		#region NOMINAL CODES / GLACOUUNTS

		public static bool SageNominalCodesToMineralTreeGLAccounts(string companyid, string sessiontoken)
		{
			// GET ALL THE SAGE NOMINAL CODES TO SYNC WITH MINERAL TREE
			List<NominalCode> nominalcodes = SageApi.GetNominalCodes();

			foreach (NominalCode nominalcode in nominalcodes)
			{
				GlAccountRoot glaccountroot = Mapper.SageNominalCodeToMTGlAccount(nominalcode);

				MTApi.CreateGlAccount(companyid, glaccountroot, sessiontoken);
			}

			return true;
		}

		#endregion

		#region COST CENTRES - LOCATIONS

		public static bool SageCostCentresToMineralTreeLocations(string companyid, string sessiontoken)
		{
			// GET ALL THE SAGE COST CENTRES TO SYNC WITH MINERAL TREE
			List<CostCentre> costcentres = SageApi.GetCostCentres();

			foreach (CostCentre costcentre in costcentres)
			{
				LocationRoot locationroot = Mapper.SageCostCentreToMTLocation(costcentre);
				MTApi.CreateLocation(companyid, locationroot, sessiontoken);
			}

			return true;
		}

		#endregion

		#region PAYMENT TERMS

		public static bool SagePaymentTermsToMineralTreeTerms(string companyid, string sessiontoken)
		{
			// GET ALL THE SAGE PAYMENT TERMS TO SYNC WITH MINERAL TREE
			List<Tuple<decimal, int, int>> terms = SageApi.GetPaymentTerms();

			foreach (Tuple<decimal, int, int> term in terms)
			{
				TermRoot termroot = Mapper.SagePaymentTermsToMTTerms(term);
				MTApi.CreateTerm(companyid, termroot, sessiontoken);
			}

			return true;
		}

		#endregion

		#region PURCHASE ORDERS

		public static bool SagePurchaseOrdersToMineralTreePurchaseOrders(string companyid, string sessiontoken)
		{
			// GET ALL THE SAGE PURCHASE ORDERS TO SYNC WITH MINERAL TREE
			List<POPOrder> orders = SageApi.GetPurchaseOrders();

			foreach (POPOrder order in orders)
			{
				PurchaseOrderRoot poroot = Mapper.SagePurchaseOrderToMTPurchaseOrder(order);
				//MTApi.CreateTerm(companyid, termroot, sessiontoken);
			}

			return true;
		}

		#endregion
	}
}
