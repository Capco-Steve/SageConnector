using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MTLib;
using MTLib.Objects;
using SageLib;
using SageLib.Objects;
using Utils;
using SageSupplier = Sage.Accounting.PurchaseLedger.Supplier;
using Bank = Sage.Accounting.CashBook.Bank;
using SageDepartment = Sage.Accounting.SystemManager.Department;
using StockItem = Sage.Accounting.Stock.StockItem;
using NominalCode = Sage.Accounting.NominalLedger.NominalCode;
using CostCentre = Sage.Accounting.SystemManager.CostCentre;
using POPOrder = Sage.Accounting.POP.POPOrder;
using TaxCode = Sage.Accounting.TaxModule.TaxCode;

namespace SyncLib
{
	/// <summary>
	/// Performs object sychronisation
	/// </summary>
    public class Sync
    {
		public static event EventHandler<SyncEventArgs> OnProgress;
		public static event EventHandler<SyncEventArgs> OnError;
		public static event EventHandler<SyncEventArgs> OnCancelled;
		public static event EventHandler<SyncEventArgs> OnComplete;

		private static int Errors = 0;
		private static bool Continue = true;

		static Sync()
		{
			MTApi.OnError += MTApi_OnError;
		}

		#region SYNC

		public static void SyncAll(CancellationToken token, bool fullsync, bool enablehttplogging, DateTime? lastsynctime)
		{
			MTApi.EnableHTTPLogging = enablehttplogging;
			Errors = 0;
			Continue = true;
			DateTime dtstart = DateTime.Now;

			Progress(string.Format("Start Sync at {0}", dtstart.ToString("yyyy-MM-dd HH:mm:ss")));
			// GET SESSION TOKEN
			Progress("Getting Session Token...");
			string sessiontoken = MTApi.GetSessionToken();
			if (!CanContinue(token)){ return; }
			if (sessiontoken.Length == 0)
			{
				Progress("Failed to get session token - stopping");
				Complete("Sync Failed");
				return;
			}
			Progress(string.Format("Session Token: {0}", sessiontoken));

			// GET COMPANY LIST
			Progress("Getting Company List...");
			List<Company> companies = MTApi.GetCompaniesForCurrentUser(sessiontoken);
			if (!CanContinue(token)) { return; }
			if (companies == null)
			{
				Progress("Failed to get companies - stopping");
				Complete("Sync Failed");
				return;
			}
			
			Progress(string.Format("Got {0} Companies", companies.Count()));
			foreach (Company company in companies)
			{
				Progress(company.name);
			}

			Progress(string.Format("Finding company to sync to: {0}", SyncSettings.MTCompanyNameToSync));
			Company found = companies.Find(o => o.name == SyncSettings.MTCompanyNameToSync);
			if(found == null)
			{
				Progress(string.Format("Could not locate company to sync - stopping"));
				Complete("Sync Failed");
				return;
			}

			Progress(string.Format("Found Company: {0}, Company ID: {1}", found.name, found.id));

			if (!Sage200Api.Connect(SyncSettings.SageCompanyNameToSync))
			{
				Progress("Failed to connect to Sage - stopping");
				Complete("Sync Failed");
				return;
			}

			Progress("Connected to Sage OK");

			if(lastsynctime.HasValue)
			{
				Progress(string.Format("Last Sync was at {0}, performing delta sync", lastsynctime.Value.ToString("yyyy/MM/dd HH:mm:ss")));
			}
			else
			{
				Progress("Missing last sync date, performing full sync");
			}

			Progress(string.Format("Syncing to Company: {0}", found.name));

			if (fullsync)
			{
				
				SageVatRatesToMineralTreeClasses(found.id, sessiontoken, lastsynctime);
				if (!CanContinue(token)) { return; }
				SageNominalCodesToMineralTreeGLAccounts(found.id, sessiontoken, lastsynctime);
				if (!CanContinue(token)) { return; }
				SageSuppliersToMineralTreeVendors(found.id, sessiontoken, lastsynctime);
				if (!CanContinue(token)) { return; }
				SageStockItemsToMineralTreeItems(found.id, sessiontoken, lastsynctime);
				if (!CanContinue(token)) { return; }
				SageBankAccountsToMineralTreePaymentMethods(SyncSettings.MTCompanyNameToSync, found.id, sessiontoken, lastsynctime);
				if (!CanContinue(token)) { return; }

				/* */
				//SageDepartmentsToMineralTreeDepartments(found.id, sessiontoken, lastsynctime);
				//if (!CanContinue(token)) { return; }
				//SageCostCentresToMineralTreeLocations(found.id, sessiontoken);
				//if (!CanContinue(token)) { return; }
			}
			
			SageLivePurchaseOrdersToMineralTreePurchaseOrders(found.id, sessiontoken);
			if (!CanContinue(token)) { return; }
			NewSageInvoicesToMineralTreeBills(found.id, sessiontoken);
			if (!CanContinue(token)) { return; }
			NewMineralTreeBillsToSageInvoices(found.id, sessiontoken);
			if (!CanContinue(token)) { return; }
			NewMineralTreePaymentsToSagePayments(found.id, sessiontoken);
			if (!CanContinue(token)) { return; }
			// FUDGE TO GET AROUND THE FACT THAT MT TAKE A FEW SECONDS TO REBUILD INDEXES 
			// SO YOU CAN'T SEARCH FOR NEW ITEMS IMMEDIATELY
			// NEED TO DO THIS PROPERLY BY KEEPING A CACHE OF EVERYING WE CREATE IN A GIVEN SESSION
			Thread.Sleep(6000);
			// END FUDGE
			SageCreditNotesToMineralTreeCredit(found.id, sessiontoken);
			if (!CanContinue(token)) { return; }
			
			DateTime dtfinish = DateTime.Now;
			TimeSpan tsduration = dtfinish - dtstart;
			
			Complete(string.Format("End Sync at {0} (Duration: {1}) - {2} error(s)", dtfinish.ToString("yyyy-MM-dd HH:mm:ss"), tsduration.ToString("hh\\:mm\\:ss"), Errors));
		}

		private static bool CanContinue(CancellationToken token)
		{
			if(Continue == false || token.IsCancellationRequested == true)
			{
				if(token.IsCancellationRequested == true)
				{
					Cancelled("Sync Cancelled");
				}
				return false;
			}
			return true;
		}

		#endregion

		#region HISTORICAL INVOICES UPLOAD

		public static string GetHistoricalInvoiceCount(DateTime from, bool enablehttplogging)
		{
			MTApi.EnableHTTPLogging = enablehttplogging;
			StringBuilder sb = new StringBuilder();
			
			Sage200Api.Connect(SyncSettings.SageCompanyNameToSync);
			sb.AppendFormat("{0}", Sage200Api.GetHistoricalInvoices(from).Count());
			return sb.ToString();
		}

		public static void LoadHistoricalInvoices(DateTime from, bool enablehttplogging, DateTime? lastsynctime)
		{
			MTApi.EnableHTTPLogging = enablehttplogging;
			Errors = 0;
			Continue = true;
			DateTime dtstart = DateTime.Now;

			Progress(string.Format("Start Loading historical invoices at {0}", dtstart.ToString("yyyy-MM-dd HH:mm:ss")));
			// GET SESSION TOKEN
			Progress("Getting Session Token...");
			string sessiontoken = MTApi.GetSessionToken();
			if (!Continue == true) { return; }
			if (sessiontoken.Length == 0)
			{
				Progress("Failed to get session token - stopping");
				Complete("Load Failed");
				return;
			}
			Progress(string.Format("Session Token: {0}", sessiontoken));

			// GET COMPANY LIST
			Progress("Getting Company List...");
			List<Company> companies = MTApi.GetCompaniesForCurrentUser(sessiontoken);
			if (!Continue == true) { return; }
			if (companies == null)
			{
				Progress("Failed to get companies - stopping");
				Complete("Load Failed");
				return;
			}

			Progress(string.Format("Got {0} Companies", companies.Count()));
			foreach (Company company in companies)
			{
				Progress(company.name);
			}

			Progress(string.Format("Finding company to sync to: {0}", SyncSettings.MTCompanyNameToSync));
			Company found = companies.Find(o => o.name == SyncSettings.MTCompanyNameToSync);
			if (found == null)
			{
				Progress(string.Format("Could not locate company - check app settings - stopping"));
				Complete("Load Failed");
				return;
			}

			Progress(string.Format("Found Company: {0}, Company ID: {1}", found.name, found.id));

			if (!Sage200Api.Connect(SyncSettings.SageCompanyNameToSync))
			{
				Progress("Failed to connect to Sage - stopping");
				Complete("Load Failed");
				return;
			}

			Progress("Connected to Sage OK");
			Progress(string.Format("Loading Invoices from {0} to Company: {1}", SyncSettings.SageCompanyNameToSync, found.name));

			// VAT RATES, NOMINALS AND VENDORS MUST BE SYNCED FIRST
			if (Continue) { SageVatRatesToMineralTreeClasses(found.id, sessiontoken, lastsynctime); }
			if (Continue) { SageNominalCodesToMineralTreeGLAccounts(found.id, sessiontoken, lastsynctime); };
			if (Continue) { SageSuppliersToMineralTreeVendors(found.id, sessiontoken, lastsynctime); }
			if (Continue) { SageHistoricalInvoicesToMineralTreeBills(found.id, sessiontoken, from); };
			
			DateTime dtfinish = DateTime.Now;
			TimeSpan tsduration = dtfinish - dtstart;

			Progress(string.Format("End Invoice Upload at {0} (Duration: {1}) - {2} error(s)", dtfinish.ToString("yyyy-MM-dd HH:mm:ss"), tsduration.ToString("hh\\:mm\\:ss"), Errors));
			return;
		}

		#endregion

		#region EVENTS

		private static void Progress(string message)
		{
			if(OnProgress != null)
			{
				OnProgress(null, new SyncEventArgs() { Message = message });
			}
		}

		private static void Error(string message)
		{
			if (OnError != null)
			{
				OnError(null, new SyncEventArgs() { Message = message });
			}
		}

		private static void Cancelled(string message)
		{
			if (OnCancelled != null)
			{
				OnCancelled(null, new SyncEventArgs() { Message = message });
			}
		}

		private static void Complete(string message)
		{
			if (OnComplete != null)
			{
				OnComplete(null, new SyncEventArgs() { Message = message });
			}
		}

		private static void MTApi_OnError(object sender, MTEventArgs e)
		{
			Continue = false;
			Errors++;
			Progress("An Exception was thrown - check log for details. Stopping Sync");
			Complete("Sync failed");
		}

		#endregion

		#region SUPPLIERS / VENDOR

		private static void SageSuppliersToMineralTreeVendors(string companyid, string sessiontoken, DateTime? lastsynctime)
		{
			Progress("Start Syncing Suppliers/Vendors...");
			// GET ALL THE SAGE SUPPLIERS AND CREATE A LIST OF CORRESPONDING VENDORS TO SYNC WITH MINERAL TREE
			List<SageSupplier> suppliers = null;
			if(lastsynctime.HasValue)
			{
				suppliers = Sage200Api.GetSuppliersModifiedAfter(lastsynctime.Value);
				Progress("Performing delta supplier syncing");
			}
			else
			{
				suppliers = Sage200Api.GetSuppliers();
				Progress("Performing full supplier sync");
			}
			
			Progress(string.Format("Loaded {0} Suppliers from Sage", suppliers.Count()));

			foreach (SageSupplier supplier in suppliers)
			{
				// DOES THE VENDOR ALREADY EXIST? SAGE PRIMARY KEY == MT EXTERNAL ID
				Vendor found = MTApi.GetVendorByExternalID(companyid, sessiontoken, supplier.PrimaryKey.DbValue.ToString());
				VendorRoot vendorroot = Mapper.SageSupplierToMTVendor(supplier);

				// VENDOR DEFAULT - TO BE TESTED WHEN VENDOR DEFAULTS ARE ENABLED IN THE API
				// NEED TO UNCOMMENT THE VENDORCOMPANYDEFAULT PROPERTY IN THE VENDOR OBJECT

				/*
				// CREATE/UPDATE VENDOR TERMS FIRST - MT STORES TERMS AS SEPARATE OBJECT TO VENDOR
				Term term = MTApi.GetTermByExternalID(companyid, sessiontoken, supplier.PrimaryKey.DbValue.ToString());
				TermRoot termroot = Mapper.SagePaymentTermsToMTTerms(supplier);

				if(term == null)
				{
					// CREATE
					term = MTApi.CreateTerm(companyid, termroot, sessiontoken);
					if (!Continue) { return; }
					Progress(string.Format("Supplier: {0}, Term {0} does not exist in MT - creating", supplier.Name, termroot.term.name));
				}
				else
				{
					// UPDATE
					termroot.term.id = term.id;
					term = MTApi.UpdateTerm(termroot, sessiontoken);
					if (!Continue) { return; }
					Progress(string.Format("Supplier: {0}, Term {0} already exists in MT - updating", supplier.Name, termroot.term.name));
				}

				// SET THE TERM ID IN THE VENDOR DEFAULTS
				vendorroot.vendor.vendorCompanyDefault.defaultTermsId = term.id;

				// DEFAULT NOMINAL CODE / EXPENSE ACCOUNT
				NominalCode nominalcode = Sage200Api.GetNominalCodeByAccountNumber(supplier.DefaultNominalAccountNumber);
				if (nominalcode != null)
				{
					GlAccount glaccount = MTApi.GetGlAccountByExternalID(companyid, sessiontoken, nominalcode.PrimaryKey.DbValue.ToString());
					if (glaccount != null)
					{
						vendorroot.vendor.vendorCompanyDefault.defaultExpenseAccountId = glaccount.id;
					}
				}

				// CLASSIFICATION / VAT RATE
				Classification classification = MTApi.GetClassificationByExternalID(companyid, sessiontoken, supplier.DefaultTaxCode.PrimaryKey.DbValue.ToString());
				if (classification != null)
				{
					vendorroot.vendor.vendorCompanyDefault.defaultClassId = classification.id;
				}
				//
				*/

				if (found == null)
				{
					// CREATE
					MTApi.CreateVendor(companyid, vendorroot, sessiontoken);
					if (!Continue) { return; }
					Progress(string.Format("Vendor {0} does not exist in MT - creating", supplier.Name));
				}
				else
				{
					// UPDATE
					vendorroot.vendor.id = found.id; // MAY NOT BE NEEDED
					MTApi.UpdateVendor(found.id, vendorroot, sessiontoken);
					if (!Continue) { return; }
					Progress(string.Format("Vendor {0} already exists in MT - updating", supplier.Name));
				}
			}

			Progress("Finish Syncing Suppliers/Vendors");
			return;
		}

		#endregion

		#region BANK ACCOUNTS/PAYMENT METHODS

		private static void SageBankAccountsToMineralTreePaymentMethods(string companyname, string companyid, string sessiontoken, DateTime? lastsynctime)
		{
			Progress("Start Syncing Bank Accounts/Payment Methods...");
			// GET ALL THE SAGE BANK ACCOUNTS TO SYNC WITH MINERAL TREE
			List<Bank> banks = null;

			if (lastsynctime.HasValue)
			{
				banks = Sage200Api.GetBanksModifiedAfter(lastsynctime.Value);
				Progress("Performing delta bank sync");
			}
			else
			{
				banks = Sage200Api.GetBanks();
				Progress("Performing full bank sync");
			}

			Progress(string.Format("Loaded {0} Banks from Sage", banks.Count()));

			foreach (Bank bank in banks)
			{
				// DOES THE BANK ALREADY EXIST? SAGE PRIMARY KEY == MT EXTERNAL ID
				PaymentMethod found = MTApi.GetPaymentMethodByExternalID(companyname, sessiontoken, bank.PrimaryKey.DbValue.ToString());
				PaymentMethodRoot paymentmethodroot = Mapper.SageBankAccountToMTPaymentMethod(bank);

				if (found == null)
				{
					// CREATE
					PaymentMethod newpaymentmethod = MTApi.CreatePaymentMethod(companyid, paymentmethodroot, sessiontoken);
					if (!Continue) { return; }
					Progress(string.Format("Payment Method {0} ({1}) does not exist in MT - creating", bank.Name, bank.BankAccount.BaseCurrencyBalance));
				}
				else
				{
					// UPDATE
					paymentmethodroot.paymentMethod.id = found.id;
					paymentmethodroot.paymentMethod.externalId = ""; // CAN'T UPDATE WITH AN EXTERNALID
					MTApi.UpdatePaymentMethod(paymentmethodroot, sessiontoken);
					if (!Continue) { return; }
					Progress(string.Format("PaymentMethod {0} ({1}) already exists in MT - updating", bank.Name, bank.BankAccount.BaseCurrencyBalance));
				}
			}

			Progress("Finish Syncing Bank Accounts/Payment Methods");
			return;
		}

		#endregion

		#region DEPARTMENTS

		private static void SageDepartmentsToMineralTreeDepartments(string companyid, string sessiontoken, DateTime? lastsynctime)
		{
			Progress("Start Syncing Departments...");
			// GET ALL THE SAGE DEPARTMENTS TO SYNC WITH MINERAL TREE
			List<SageDepartment> departments = null;

			if (lastsynctime.HasValue)
			{
				// DELTA SYNC
				departments = Sage200Api.GetDepartmentsModifiedAfter(lastsynctime.Value);
			}
			else
			{
				// FULL SYNC
				departments = Sage200Api.GetDepartments();
			}

			Progress(string.Format("Loaded {0} Departments from Sage", departments.Count()));

			foreach (SageDepartment department in departments)
			{
				if (department.Name == null || department.Name.Length == 0) { continue; } // SKIP DEPARTMENTS WITH NO NAME AS MT DOESN'T ALLOW DEPARTMENTS WITH NO NAME
				// DOES THE DEPARTMENT ALREADY EXIST? SAGE PRIMARY KEY == MT EXTERNAL ID
				Department found = MTApi.GetDepartmentByExternalID(companyid, sessiontoken, department.PrimaryKey.DbValue.ToString());
				DepartmentRoot departmentroot = Mapper.SageDepartmentToMTDepartment(department);

				if (found == null)
				{
					// CREATE
					Department newdepartment = MTApi.CreateDepartment(companyid, departmentroot, sessiontoken);
					if (!Continue) { return; }
					Progress(string.Format("Department {0} does not exist in MT - creating", department.Name));
				}
				else
				{
					// UPDATE
					departmentroot.department.id = found.id;
					departmentroot.department.externalId = ""; // CAN'T UPDATE WITH AN EXTERNALID
					MTApi.UpdateDepartment(departmentroot, sessiontoken);
					if (!Continue) { return; }
					Progress(string.Format("Department {0} already exists in MT - updating", department.Name));
				}
			}

			Progress("Finish Syncing Departments");
			return;
		}

		#endregion

		#region STOCKITEMS / ITEMS

		private static void SageStockItemsToMineralTreeItems(string companyid, string sessiontoken, DateTime? lastsynctime)
		{
			// GET ALL THE SAGE STOCKITEMS TO SYNC WITH MINERAL TREE
			Progress("Start Syncing Stock Items...");
			// GET ALL THE SAGE DEPARTMENTS TO SYNC WITH MINERAL TREE
			List<StockItem> stockitems = null;

			if (lastsynctime.HasValue)
			{
				stockitems = Sage200Api.GetStockItemsModifiedAfter(lastsynctime.Value);
				Progress("Performing delta stockitem sync");
			}
			else
			{
				stockitems = Sage200Api.GetStockItems();
				Progress("Performing full supplier sync");
			}

			Progress(string.Format("Loaded {0} Stock Items from Sage", stockitems.Count()));

			foreach (StockItem stockitem in stockitems)
			{
				// DOES THE STOCKITEM ALREADY EXIST? SAGE PRIMARY KEY == MT EXTERNAL ID
				Item found = MTApi.GetItemByExternalID(companyid, sessiontoken, stockitem.PrimaryKey.DbValue.ToString());
				ItemRoot itemroot = Mapper.SageStockItemToMTItem(stockitem);

				if (found == null)
				{
					// CREATE
					Item newitem = MTApi.CreateItem(companyid, itemroot, sessiontoken);
					if (!Continue) { return; }
					Progress(string.Format("Item {0} does not exist in MT - creating", stockitem.Name));
				}
				else
				{
					// UPDATE
					itemroot.item.id = found.id;
					itemroot.item.externalId = ""; // CAN'T UPDATE WITH AN EXTERNALID
					MTApi.UpdateItem(itemroot, sessiontoken);
					if (!Continue) { return; }
					Progress(string.Format("Item {0} already exists in MT - updating", stockitem.Name));
				}
			}

			Progress("Finish Syncing Stock Items");
			return;
		}

		#endregion

		#region VAT RATES / CLASSIFICATION

		private static void SageVatRatesToMineralTreeClasses(string companyid, string sessiontoken, DateTime? lastsynctime)
		{
			Progress("Start Syncing Vat Rates...");
			// GET ALL THE SAGE VAT RATES TO SYNC WITH MINERAL TREE
			List<TaxCode> vatrates = null;

			if (lastsynctime.HasValue)
			{
				vatrates = Sage200Api.GetVatRatesModifiedAfter(lastsynctime.Value);
				Progress("Performing delta classification/vat rate sync");
			}
			else
			{
				vatrates = Sage200Api.GetVatRates();
				Progress("Performing full classification/vat rate sync");
			}

			Progress(string.Format("Loaded {0} Vat Rates from Sage", vatrates.Count()));
			
			foreach (TaxCode taxcode in vatrates)
			{
				// DOES THE VAT RATE ALREADY EXIST? SAGE PRIMARY KEY == MT EXTERNAL ID
				Classification found = MTApi.GetClassificationByExternalID(companyid, sessiontoken, taxcode.PrimaryKey.DbValue.ToString());
				ClassificationRoot classificationroot = Mapper.SageTaxCodeToMTClassification(taxcode);

				if (found == null)
				{
					// CREATE
					Classification newclassification = MTApi.CreateClassification(companyid, classificationroot, sessiontoken);
					if (!Continue) { return; }
					Progress(string.Format("Vat Rate {0} does not exist in MT - creating", taxcode.Name));
				}
				else
				{
					classificationroot.classification.id = found.id;
					classificationroot.classification.externalId = ""; // CAN'T UPDATE WITH AN EXTERNALID
					MTApi.UpdateClassification(classificationroot, sessiontoken);
					if (!Continue) { return; }
					Progress(string.Format("Vat Rate {0} already exists in MT - updating", taxcode.Name));
				}
			}

			Progress("Finish Syncing Vat Rates");
		}

		#endregion

		#region NOMINAL CODES / GLACOUUNTS

		private static void SageNominalCodesToMineralTreeGLAccounts(string companyid, string sessiontoken, DateTime? lastsynctime)
		{
			Progress("Start Syncing GLCodes...");
			// GET ALL THE SAGE NOMINAL CODES TO SYNC WITH MINERAL TREE
			List<NominalCode> nominalcodes = null;

			if (lastsynctime.HasValue)
			{
				// DELTA SYNC
				nominalcodes = Sage200Api.GetNominalCodesModifiedAfter(lastsynctime.Value);
				Progress("Performing delta gl code sync");
			}
			else
			{
				// FULL SYNC
				nominalcodes = Sage200Api.GetNominalCodes();
				Progress("Performing full gl code sync");
			}

			Progress(string.Format("Loaded {0} Nominal Codes from Sage", nominalcodes.Count()));

			foreach (NominalCode nominalcode in nominalcodes)
			{
				// DOES THE NOMINAL CODE ALREADY EXIST? SAGE PRIMARY KEY == MT EXTERNAL ID
				GlAccount found = MTApi.GetGlAccountByExternalID(companyid, sessiontoken, nominalcode.PrimaryKey.DbValue.ToString());
				GlAccountRoot glaccountroot = Mapper.SageNominalCodeToMTGlAccount(nominalcode);

				if (found == null)
				{
					// CREATE
					GlAccount newglaccount = MTApi.CreateGlAccount(companyid, glaccountroot, sessiontoken);
					if (!Continue) { return; }
					Progress(string.Format("GL Account {0} does not exist in MT - creating", nominalcode.Name));
				}
				else
				{
					glaccountroot.glAccount.id = found.id;
					glaccountroot.glAccount.externalId = ""; // CAN'T UPDATE WITH AN EXTERNALID
					MTApi.UpdateGlAccount(glaccountroot, sessiontoken);
					if (!Continue) { return; }
					Progress(string.Format("GL Account {0} already exists in MT - updating", nominalcode.Name));
				}
			}

			Progress("Finish Syncing GL Accounts");
		}

		#endregion

		#region COST CENTRES - LOCATIONS

		private static void SageCostCentresToMineralTreeLocations(string companyid, string sessiontoken, DateTime? lastsynctime)
		{
			Progress("Start Syncing Cost Centres/Locations...");
			// GET ALL THE SAGE COST CENTRES TO SYNC WITH MINERAL TREE
			List<CostCentre> costcentres = null;

			if (lastsynctime.HasValue)
			{
				// DELTA SYNC
				costcentres = Sage200Api.GetCostCentresModifiedAfter(lastsynctime.Value);
				Progress("Performing delta cost centre/location sync");
			}
			else
			{
				// FULL SYNC
				costcentres = Sage200Api.GetCostCentres();
				Progress("Performing full cost centre/location sync");
			}

			Progress(string.Format("Loaded {0} Cost Centres from Sage", costcentres.Count()));

			foreach (CostCentre costcentre in costcentres)
			{
				// DOES THE NOMINAL CODE ALREADY EXIST? SAGE PRIMARY KEY == MT EXTERNAL ID
				Location found = MTApi.GetLocationByExternalID(companyid, sessiontoken, costcentre.PrimaryKey.DbValue.ToString());
				LocationRoot locationroot = Mapper.SageCostCentreToMTLocation(costcentre);

				if (found == null)
				{
					// CREATE
					Location newlocation = MTApi.CreateLocation(companyid, locationroot, sessiontoken);
					if (!Continue) { return; }
					Progress(string.Format("Location {0} does not exist in MT - creating", costcentre.Name));
				}
				else
				{
					// UPDATE
					locationroot.location.id = found.id;
					locationroot.location.externalId = ""; // CAN'T UPDATE WITH AN EXTERNALID
					MTApi.UpdateLocation(locationroot, sessiontoken);
					if (!Continue) { return; }
					Progress(string.Format("Location {0} already exists in MT - updating", costcentre.Name));
				}
			}

			Progress("Finish Syncing Cost Centres/Locations");
			return;
		}

		#endregion

		#region PURCHASE ORDERS

		private static void SageLivePurchaseOrdersToMineralTreePurchaseOrders(string companyid, string sessiontoken)
		{
			Progress("Start Syncing Sage Live Purchase Orders...");
			// GET ALL THE SAGE PURCHASE ORDERS TO SYNC WITH MINERAL TREE
			List<POPOrder> orders = Sage200Api.GetLivePurchaseOrders(); // LIVE
			Progress(string.Format("Loaded {0} Live Purchase Orders from Sage", orders.Count()));

			// PUSH/UPDATE LIVE ORDERS
			foreach (POPOrder order in orders)
			{
				// DOES IT ALREADY EXIST IN MT?
				PurchaseOrder found = MTApi.GetPurchaseOrderByExternalID(companyid, sessiontoken, order.PrimaryKey.DbValue.ToString());

				if (order.DocumentStatus == Sage.Accounting.OrderProcessing.DocumentStatusEnum.EnumDocumentStatusLive)
				{
					if (found == null)
					{
						// NO, CREATE IT
						PurchaseOrderRoot poroot = Mapper.SagePurchaseOrderToMTPurchaseOrder(companyid, order, sessiontoken);
						PurchaseOrder newpurchaseorder = MTApi.CreatePurchaseOrder(companyid, poroot, sessiontoken);
						if (!Continue) { return; }
						Progress(string.Format("PO {0} does not exist in MT - creating it", order.DocumentNo));
					}
					else
					{
						// YES, CHECK STATUS
						if (found.state.ToLower() == "closed")
						{
							// UPDATE SAGE
							order.DocumentStatus = Sage.Accounting.OrderProcessing.DocumentStatusEnum.EnumDocumentStatusComplete;
							order.Update();
							Progress("PO already exists - state is closed so updating Sage state to complete");
						}
						else
						{
							PurchaseOrderRoot po = Mapper.SagePurchaseOrderToMTPurchaseOrder(companyid, order, sessiontoken);
							po.purchaseOrder.externalId = "";
							po.purchaseOrder.id = found.id;
							MTApi.UpdatePurchaseOrder(po, sessiontoken);
							if (!Continue) { return; }
							Progress("PO already exists - state is not closed so updating");
						}
					}
				}
			}

			Progress("Finish Syncing Sage Live Purchase Orders");
			Progress("Start Syncing MT PendingBilling Purchase Orders");

			// UPDATE ANY MT PO'S THAT HAVE BEEN CANCELED IN SAGE
			List<PurchaseOrder> pendingbilling = MTApi.GetPendingBillingPurchaseOrders(companyid, sessiontoken);
			if (!Continue) { return; }
			Progress(string.Format("Loaded {0} PendingBilling Purchase Orders from MT", pendingbilling.Count()));
			foreach (PurchaseOrder mtpo in pendingbilling)
			{
				// CHECK THE STATE IS STILL VALID IN SAGE
				Progress(string.Format("Processing PO {0}", mtpo.externalId));
				POPOrder sagepo = Sage200Api.GetPurchaseOrderByPrimaryKey(mtpo.externalId);

				if (sagepo.DocumentStatus != Sage.Accounting.OrderProcessing.DocumentStatusEnum.EnumDocumentStatusLive)
				{
					// NOT LIVE SO HAS BEEN CANCELLED IN SAGE, UPDATE MT
					PurchaseOrderRoot root = new PurchaseOrderRoot();
					root.purchaseOrder = mtpo;
					root.purchaseOrder.state = "Cancelled";
					MTApi.UpdatePurchaseOrder(root, sessiontoken);
					if (!Continue) { return; }
					Progress("Sage state is not live - updating to cancelled in MT");
				}
				else
				{
					Progress("Sage state is live - no update required");
				}
			}

			// TODO????
			// UPDATE SAGE WITH CLOSED PO'S FROM MT.

			Progress("Finish Syncing MT PendingBilling Purchase Orders");
		}

		#endregion

		#region HISTORICAL INVOICES/BILLS

		private static void SageHistoricalInvoicesToMineralTreeBills(string companyid, string sessiontoken, DateTime from)
		{
			Progress("Start Loading Historical Invoices...");
			List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry> invoices = Sage200Api.GetHistoricalInvoices(from);
			Progress(string.Format("Loaded {0} Historical Invoices from Sage", invoices.Count()));

			foreach (Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry invoice in invoices)
			{
				// DOES IT ALREADY EXIST?
				Bill bill = MTApi.GetBillByExternalID(companyid, sessiontoken, invoice.PrimaryKey.DbValue.ToString());
				if (bill == null)
				{
					BillRoot billroot = Mapper.SageInvoiceToMTBill(companyid, invoice, sessiontoken);
					if(billroot != null)
					{
						MTApi.CreateBill(companyid, billroot, sessiontoken);
					}
					else
					{
						Progress(string.Format("Mapping Failed for Invoice: {0}, Supplier: {1}", invoice.InstrumentNo, invoice.Supplier.Name));
					}
						
					if (!Continue) { return; }

					Progress(string.Format("Invoice {0} does not exist in MT - creating", invoice.InstrumentNo));
				}
				else
				{
					// NO NEED TO UPDATE THE HISTORIAL RECORDS
					Progress(string.Format("Invoice {0} already exists in MT", invoice.InstrumentNo));
				}
			}

			Progress("End Loading Historical Invoices");

			return;
		}

		#endregion

		#region INVOICES/BILLS

		private static void NewSageInvoicesToMineralTreeBills(string companyid, string sessiontoken)
		{
			Progress("Start Syncing new Sage Invoices to Mineral Tree...");
			List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry> invoices = Sage200Api.GetNewInvoices(SyncSettings.StartDate);
			Progress(string.Format("Loaded {0} new Invoices from Sage", invoices.Count()));

			foreach (Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry entry in invoices)
			{
				// DOES THE BILL ALREADY EXIST IN MT??
				Bill found = MTApi.GetBillByExternalID(companyid, sessiontoken, entry.PrimaryKey.DbValue.ToString());

				if (found == null)
				{
					// CREATE IT
					BillRoot billroot = Mapper.SageInvoiceToMTBill(companyid, entry, sessiontoken);
					MTApi.CreateBill(companyid, billroot, sessiontoken);
					if (!Continue) { return; }
					Progress(string.Format("Invoice {0} does not exist in MT - creating", entry.InstrumentNo));
				}
				else
				{
					// TODO - DO WE NEED TO UPDATE?????
					Progress(string.Format("Invoice {0} already exists in MT - no action needed", entry.InstrumentNo));
				}
			}

			Progress("Finish Syncing new Sage Invoices to Mineral Tree");
		}

		private static void NewMineralTreeBillsToSageInvoices(string companyid, string sessiontoken)
		{
			Progress("Start Syncing new Mineral Tree Bills to Sage...");
			// GET THE UNPROCESSED BILLS FROM MT
			List<Bill> bills = MTApi.GetNewBillsWithStatusOpenOrPendingSettlement(companyid, sessiontoken);
			Progress(string.Format("Loaded {0} new Bills from Mineral Tree", bills.Count()));
			string newinvoiceid = "";

			foreach (Bill bill in bills)
			{
				newinvoiceid = "";
				// NEW BILL SO CREATE THE INVOICE IN SAGE
				VendorRoot vendorroot = MTApi.GetVendorByID(companyid, bill.vendor.id, sessiontoken);
				// CHECK THIS INVOICE NUMBER DOESN'T ALREADY EXIST FOR THIS SUPPLIER
				Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry entry = Sage200Api.GetInvoiceByInvoiceNumberAndSupplierPrimaryKey(vendorroot.vendor.externalId, bill.invoiceNumber);

				if (entry == null)
				{
					List<LineItem> lineitems = new List<LineItem>();
					foreach (CompanyItem item in bill.items)
					{
						if (item.glAccount != null && item.classification != null)
						{
							lineitems.Add(new LineItem()
							{
								GLAccountID = item.glAccount.externalId,
								ClassificationID = item.classification.externalId,
								Description = item.description,
								NetAmount = PriceConverter.ToDecimal(item.netAmount.amount, item.netAmount.precision),
								TaxAmount = PriceConverter.ToDecimal(item.taxAmount.amount, item.taxAmount.precision)
							});
						}
					}

					if(lineitems.Count() != bill.items.Count())
					{
						// MISSING GLACCOUNT OR CLASSIFICATION (VAT)
						Progress(string.Format("Missing GL Account or Classification (VAT Rate) for invoice: {0}, cannot create", bill.invoiceNumber));
						continue;
					}

					newinvoiceid = Sage200Api.CreateInvoice(vendorroot.vendor.externalId, bill.invoiceNumber, bill.transactionDate, PriceConverter.ToDecimal(bill.amount.amount, bill.amount.precision), PriceConverter.ToDecimal(bill.totalTaxAmount.amount, bill.totalTaxAmount.precision), lineitems);
					Progress(string.Format("Invoice: {0} does not exist in Sage - creating", bill.invoiceNumber));
				}
				else
				{
					// IT HAS ALREADY BEEN CREATED BUT NOT UPDATED IN MT
					newinvoiceid = entry.PrimaryKey.DbValue.ToString();
					Progress(string.Format("Invoice: {0} already exists but has not been updated in MT", bill.invoiceNumber));
				}

				if (newinvoiceid.Length > 0)
				{
					// UPDATE THE MT BILL EXTERNAL ID WITH THE PRIMARY KEY FROM SAGE
					Bill update = new Bill()
					{
						id = bill.id,
						externalId = newinvoiceid
					};
					BillRoot billroot = new BillRoot();
					billroot.bill = update;
					MTApi.UpdateBill(billroot, sessiontoken);
					if (!Continue) { return; }
					Progress(string.Format("Invoice: {0} updating external id in MT", bill.invoiceNumber));
				}
				else
				{
					Progress(string.Format("Invoice: {0} could not create in Sage", bill.invoiceNumber));
				}
			}

			Progress("Finish Syncing new Mineral Tree Bills to Sage");
		}

		#endregion

		#region PAYMENTS

		private static void NewMineralTreePaymentsToSagePayments(string companyid, string sessiontoken)
		{
			Progress("Start Syncing new Mineral Tree Payments to Sage...");
			// GET THE UNPROCESSED PAYMENTS FROM MT
			List<Payment> payments = MTApi.GetPayments(companyid, sessiontoken);
			Progress(string.Format("Loaded {0} new Payments from Mineral Tree", payments.Count()));

			foreach (Payment payment in payments)
			{
				// GRAB THE FIRST BILL INVOICE NUMBER TO USE AS A PAYMENT REFERENCE - NOTE THIS MUST BE REPLICATED IN MT
				string paymentreference = "PAY-" + payment.bills[0].invoiceNumber;
				//
				SageSupplier supplier = Sage200Api.GetSupplierByPrimaryKey(payment.vendor.externalId);
				decimal paymentamount = PriceConverter.ToDecimal(payment.amount.amount, payment.amount.precision);

				if (supplier == null)
				{
					Progress(string.Format("Supplier {0} could not be found", supplier.Name));
					continue;
				}
				else
				{
					Progress(string.Format("Creating Payment for supplier {0}, amount {1}...", supplier.Name, paymentamount));
				}

				string id = Sage200Api.CreatePayment(supplier, payment.paymentMethod.externalId, payment.transactionDate, paymentamount, 0, paymentreference);

				// UPDATE THE MT PAYMENT EXTERNAL ID WITH THE PRIMARY KEY FROM SAGE
				if (id.Length > 0)
				{
					// ALLOCATE THE PAYMENTS TO THE CORRECT INVOICES
					Sage.Accounting.PurchaseLedger.PurchaseBankPaymentPosting sagepayment = Sage200Api.GetPaymentByPrimaryKey(id);
					foreach (Bill bill in payment.bills)
					{
						decimal amount = Utils.PriceConverter.ToDecimal(bill.appliedPaymentAmount.amount, bill.appliedPaymentAmount.precision);
						Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry invoice = Sage200Api.GetInvoiceByInvoiceNumber(bill.invoiceNumber);

						if(invoice == null)
						{
							Progress(string.Format("Could not find invoice number {0}, skipping", bill.invoiceNumber));
							continue;
						}

						Sage.Accounting.PurchaseLedger.PurchaseAllocationAdjustment allocation = null;
						try
						{
							allocation = Sage.Accounting.PurchaseLedger.PurchaseAllocationAdjustmentFactory.Factory.CreateNew();
							allocation.Warnings += new Sage.Common.DataAccess.BusinessObject.WarningHandler(PurchaseAllocationsAdjustment_Warnings);

							allocation.Supplier = supplier;
							allocation.AllocationDate = Sage.Common.Clock.Today;

							// FIND THE INVOICE
							Sage.ObjectStore.Filter filter = new Sage.ObjectStore.Filter(Sage.Accounting.TradeLedger.PostedTradingAccountEntry.FIELD_INSTRUMENTNO, invoice.InstrumentNo);
							allocation.DebitEntries.Query.Filters.Add(filter);
							allocation.DebitEntries.Find();
							allocation.ResetDebitAllocationHandler(allocation.PurchaseDebitEntries);

							// FIND THE PAYMENT
							Sage.ObjectStore.Filter filter1 = new Sage.ObjectStore.Filter(Sage.Accounting.TradeLedger.PostedTradingAccountEntry.FIELD_INSTRUMENTNO, paymentreference);
							allocation.CreditEntries.Query.Filters.Add(filter1);
							allocation.CreditEntries.Find();
							allocation.ResetCreditAllocationHandler(allocation.PurchaseCreditEntries);

							Sage.Accounting.TradeLedger.TradingAllocationEntryView debitentry = allocation.DebitEntries.First;
							Sage.Accounting.TradeLedger.TradingAllocationEntryView creditentry = allocation.CreditEntries.First;

							if (debitentry != null && creditentry != null)
							{
								allocation.DebitEntries.First.AllocateThisTime = amount;
								allocation.CreditEntries.First.AllocateThisTime = amount;
								allocation.Validate();
								allocation.Allocate();
								Progress(string.Format("Allocated Payment of {0} to Invoice {1}", amount, bill.invoiceNumber));
							}
							else
							{
								Progress(string.Format("Payment Allocation to {0} Failed: Could not get Debit/Credit Entry", supplier.Name));
							}
						}
						catch(Exception ex)
						{
							Progress(string.Format("Payment Allocation exception ({0})", ex.Message));
						}
						finally
						{
							if (allocation != null)
							{
								allocation.Warnings -= new Sage.Common.DataAccess.BusinessObject.WarningHandler(PurchaseAllocationsAdjustment_Warnings);
							}
						}

						// UPDATE THE EXTERNAL ID IN MT - IF THE ALLOCATIONS HAVE FAILED THEN THEY WILL HAVE TO BE CORRECTED BY HAND
						Payment update = new Payment() { id = payment.id, externalId = id };
						PaymentRoot paymentroot = new PaymentRoot() { payment = update };
						MTApi.UpdatePayment(paymentroot, sessiontoken);
						if (!Continue) { return; }
					}
				}
				else
				{
					Progress("Payment creation failed");
				}
			}

			Progress("Finish Syncing new Mineral Tree Payments to Sage");
		}

		private static void PurchaseAllocationsAdjustment_Warnings(System.Object sender,Sage.Common.DataAccess.WarningArgs args)
		{
			try
			{
				Sage.Accounting.PurchaseLedger.PurchaseAllocationAdjustment allocation = (Sage.Accounting.PurchaseLedger.PurchaseAllocationAdjustment) sender;
				// Check if the Warning is a Sage exception
				if (args.ExceptionFailureIndicator.FailureException is Sage.Accounting.Exceptions.SageAccountingException)
				{
					Sage.Accounting.Exceptions.SageAccountingException sageAccountingException = args.ExceptionFailureIndicator.FailureException as Sage.Accounting.Exceptions.SageAccountingException;

					// If the credit total is greater than the debit total we reduce the credit total.
					// If the credit total is less than the debit total we create a credit discount 
					// for the difference.
					if (sageAccountingException is Sage.Accounting.Exceptions.Ex10279Exception)
					{
						allocation.TradingAllocationAdjustmentHelper.ReduceCreditAllocations();
					}
					else if (sageAccountingException is Sage.Accounting.Exceptions.Ex10280Exception)
					{
						allocation.GenerateDiscountInstrument();
					}
				}
			}
			catch (System.Exception ex)
			{
				//System.Diagnostics.Debug.WriteLine(ex.Message);
			}
		}


		#endregion

		#region CREDIT NOTES

		private static void SageCreditNotesToMineralTreeCredit(string companyid, string sessiontoken)
		{
			Progress("Start Syncing Credit Notes...");
			// GET ALL THE SAGE CREDIT NOTES TO SYNC WITH MINERAL TREE
			List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry> creditnotes = Sage200Api.GetCreditNotes();
			Progress(string.Format("Loaded {0} Credit Notes from Sage", creditnotes.Count()));

			foreach (Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry creditnote in creditnotes)
			{
				// DOES THE CREDITNOTE ALREADY EXIST? SAGE PRIMARY KEY == MT EXTERNAL ID
				Credit found = MTApi.GetCreditByExternalID(companyid, sessiontoken, creditnote.PrimaryKey.DbValue.ToString());

				if (found == null)
				{
					CreditRoot creditroot = Mapper.SageCreditNoteToMTCredit(companyid, creditnote, sessiontoken);
					Bill bill = MTApi.GetBillByInvoiceNumber(companyid, sessiontoken, creditnote.SecondReferenceNo);
					if (bill != null)
					{
						// CREATE
						Credit newcredit = MTApi.CreateCredit(companyid, creditroot, sessiontoken);
						if (!Continue) { return; }
						// NOW CREATE THE BILLCREDIT
						BillCreditRoot billcreditroot = Mapper.SageCreditNoteToMTBillCredit(companyid, creditnote, sessiontoken);
						billcreditroot.billCredit.credit = new ObjID() { id = newcredit.id };
						billcreditroot.billCredit.bill = new ObjID() { id = bill.id };
						MTApi.CreateBillCredit(companyid, billcreditroot, sessiontoken);
						if (!Continue) { return; }
						Progress(string.Format("Credit {0} does not exist in MT - creating", creditnote.SecondReferenceNo));

						// ALLOCATE CREDIT NOTE TO INVOICE IN SAGE
						Sage.Accounting.PurchaseLedger.PurchaseAllocationAdjustment allocation = null;
						try
						{
							allocation = Sage.Accounting.PurchaseLedger.PurchaseAllocationAdjustmentFactory.Factory.CreateNew();
							allocation.Warnings += new Sage.Common.DataAccess.BusinessObject.WarningHandler(PurchaseAllocationsAdjustment_Warnings);

							allocation.Supplier = creditnote.Supplier;
							allocation.AllocationDate = Sage.Common.Clock.Today;

							// FIND THE INVOICE
							Sage.ObjectStore.Filter filter = new Sage.ObjectStore.Filter(Sage.Accounting.TradeLedger.PostedTradingAccountEntry.FIELD_INSTRUMENTNO, creditnote.SecondReferenceNo);
							allocation.DebitEntries.Query.Filters.Add(filter);
							allocation.DebitEntries.Find();
							allocation.ResetDebitAllocationHandler(allocation.PurchaseDebitEntries);

							// FIND THE CREDIT NOTE
							Sage.ObjectStore.Filter filter1 = new Sage.ObjectStore.Filter(Sage.Accounting.TradeLedger.PostedTradingAccountEntry.FIELD_INSTRUMENTNO, creditnote.InstrumentNo);
							allocation.CreditEntries.Query.Filters.Add(filter1);
							allocation.CreditEntries.Find();
							allocation.ResetCreditAllocationHandler(allocation.PurchaseCreditEntries);

							Sage.Accounting.TradeLedger.TradingAllocationEntryView debitentry = allocation.DebitEntries.First;
							Sage.Accounting.TradeLedger.TradingAllocationEntryView creditentry = allocation.CreditEntries.First;

							if (debitentry != null && creditentry != null)
							{
								allocation.DebitEntries.First.AllocateThisTime = Math.Abs(creditnote.CoreDocumentGrossValue);
								allocation.CreditEntries.First.AllocateThisTime = Math.Abs(creditnote.CoreDocumentGrossValue);
								allocation.Validate();
								allocation.Allocate();
								Progress(string.Format("Allocated Credit of {0} to Invoice {1}", creditnote.CoreDocumentGrossValue, creditnote.SecondReferenceNo));
							}
							else
							{
								Progress(string.Format("Credit Allocation to {0} Failed: Could not get Debit/Credit Entry", creditnote.Supplier.Name));
							}
						}
						catch (Exception ex)
						{
							Progress(string.Format("Credit Allocation exception ({0})", ex.Message));
						}
						finally
						{
							if (allocation != null)
							{
								allocation.Warnings -= new Sage.Common.DataAccess.BusinessObject.WarningHandler(PurchaseAllocationsAdjustment_Warnings);
							}
						}
						// END ALLOCATION
					}
					else
					{
						Progress(string.Format("Could not load bill - invoice number:{0}", creditnote.SecondReferenceNo));
					}
				}
				else
				{
					// UPDATE ???? 
					//creditroot.credit.id = found.id;
					//creditroot.credit.externalId = ""; // CAN'T UPDATE WITH AN EXTERNALID
					//MTApi.UpdateCredit(creditroot, sessiontoken);
					//if (!Continue) { return; }
					Progress(string.Format("Credit {0} already exists in MT - no action", creditnote.SecondReferenceNo));
				}
			}

			Progress("Finish Syncing Credit Notes");
		}

		#endregion
	}
}
