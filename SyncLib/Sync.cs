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
    public static class Sync
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

		public static void SyncAll(CancellationToken token, bool fullsync, bool enablehttplogging)
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

			if (!SageApi.Connect(SyncSettings.SageCompanyNameToSync))
			{
				Progress("Failed to connect to Sage - stopping");
				Complete("Sync Failed");
				return;
			}

			Progress("Connected to Sage OK");

			Progress("Loading Reference Data from Mineral Tree...");
			MTReferenceData.LoadReferenceData(found.id, sessiontoken, fullsync);
			if (!CanContinue(token)) { return; }

			Progress(string.Format("Loaded {0} Vendors", MTReferenceData.GetVendorCount()));
			Progress(string.Format("Loaded {0} Items", MTReferenceData.GetItemCount()));

			if (fullsync)
			{
				Progress(string.Format("Loaded {0} Departments", MTReferenceData.GetDepartmentCount()));
				Progress(string.Format("Loaded {0} Gl Accounts", MTReferenceData.GetGlAccountCount()));
				Progress(string.Format("Loaded {0} Locations", MTReferenceData.GetLocationCount()));
				Progress(string.Format("Loaded {0} Payment Methods", MTReferenceData.GetPaymentMethodCount()));
				Progress(string.Format("Loaded {0} Vat Rates", MTReferenceData.GetClassificationCount()));
			}

			Progress(string.Format("Syncing to Company: {0}", found.name));

			if (fullsync)
			{
				/*
				SageVatRatesToMineralTreeClasses(found.id, sessiontoken);
				if (!CanContinue(token)) { return; }
				SageNominalCodesToMineralTreeGLAccounts(found.id, sessiontoken);
				if (!CanContinue(token)) { return; }
				SageSuppliersToMineralTreeVendors(found.id, sessiontoken);
				if (!CanContinue(token)) { return; }
				SageStockItemsToMineralTreeItems(found.id, sessiontoken);
				if (!CanContinue(token)) { return; }
				SageBankAccountsToMineralTreePaymentMethods(found.id, sessiontoken);
				if (!CanContinue(token)) { return; }
				/*

				/* */
				//SageDepartmentsToMineralTreeDepartments(found.id, sessiontoken);
				//if (!CanContinue(token)) { return; }
				//SageCostCentresToMineralTreeLocations(found.id, sessiontoken);
				//if (!CanContinue(token)) { return; }
				// PAYMENT TERMS NOT FULLY IMPLEMENTED IN OPEN ACCOUNTING SYSTEM PLUS THERE IS NO WAY TO SEARCH FOR TERMS SO THIS THIS CODE HAS BEEN
				// COMMENTED OUT UNTIL MINERAL TREE HAS FINISHED THE IMPLEMENTATION.
				//SagePaymentTermsToMineralTreeTerms(found.id, sessiontoken);   // no update - moved to the historical invoice sync as it can only be run once
				//if (!CanContinue(token)) { return; }
			}
			
			//SageLivePurchaseOrdersToMineralTreePurchaseOrders(found.id, sessiontoken);
			//if (!CanContinue(token)) { return; }
			NewSageInvoicesToMineralTreeBills(found.id, sessiontoken);
			if (!CanContinue(token)) { return; }
			//NewMineralTreeBillsToSageInvoices(found.id, sessiontoken);
			//if (!CanContinue(token)) { return; }
			//NewMineralTreePaymentsToSagePayments(found.id, sessiontoken);
			//if (!CanContinue(token)) { return; }
			//SageCreditNotesToMineralTreeCredit(found.id, sessiontoken);
			//if (!CanContinue(token)) { return; }
			
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
			
			SageApi.Connect(SyncSettings.SageCompanyNameToSync);
			sb.AppendFormat("{0}", SageApi.GetHistoricalInvoices(from).Count());
			return sb.ToString();
		}

		public static void LoadHistoricalInvoices(DateTime from, bool enablehttplogging)
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

			Progress("Loading Reference Data from Mineral Tree...");
			MTReferenceData.LoadReferenceData(found.id, sessiontoken, false);
			if (!Continue == true) { return; }

			Progress(string.Format("Loaded {0} Vendors", MTReferenceData.GetVendorCount()));
			Progress(string.Format("Loaded {0} Items", MTReferenceData.GetItemCount()));

			Progress(string.Format("Loaded {0} Departments", MTReferenceData.GetDepartmentCount()));
			Progress(string.Format("Loaded {0} Gl Accounts", MTReferenceData.GetGlAccountCount()));
			Progress(string.Format("Loaded {0} Locations", MTReferenceData.GetLocationCount()));
			Progress(string.Format("Loaded {0} Payment Methods", MTReferenceData.GetPaymentMethodCount()));

			if (!SageApi.Connect(SyncSettings.SageCompanyNameToSync))
			{
				Progress("Failed to connect to Sage - stopping");
				Complete("Load Failed");
				return;
			}

			Progress("Connected to Sage OK");
			Progress(string.Format("Loading Invoices from {0} to Company: {1}", SyncSettings.SageCompanyNameToSync, found.name));

			// VENDORS MUST BE SYNCED FIRST BECAUSE INVOICES HAVE A RELATIONSHIP WITH VENDORS
			if (Continue) { SageSuppliersToMineralTreeVendors(found.id, sessiontoken); }
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

		public static void SageSuppliersToMineralTreeVendors(string companyid, string sessiontoken)
		{
			Progress("Start Syncing Suppliers/Vendors...");
			// GET ALL THE SAGE SUPPLIERS AND CREATE A LIST OF CORRESPONDING VENDORS TO SYNC WITH MINERAL TREE
			List<SageSupplier> suppliers = SageApi.GetSuppliers();
			Progress(string.Format("Loaded {0} Suppliers from Sage", suppliers.Count()));

			//SageSupplier supplier = suppliers[46]; -- BBB RocketSpace
			foreach (SageSupplier supplier in suppliers)
			{
				// DOES THE VENDOR ALREADY EXIST? SAGE PRIMARY KEY == MT EXTERNAL ID
				Vendor found = MTReferenceData.FindVendorByExternalID(supplier.PrimaryKey.DbValue.ToString());
				VendorRoot vendorroot = Mapper.SageSupplierToMTVendor(supplier);

				if (found == null)
				{
					// CREATE
					Vendor newvendor = MTApi.CreateVendor(companyid, vendorroot, sessiontoken);
					if (!Continue) { return; }
					MTReferenceData.AddVendor(newvendor);
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

		public static void SageBankAccountsToMineralTreePaymentMethods(string companyid, string sessiontoken)
		{
			Progress("Start Syncing Bank Accounts/Payment Methods...");
			// GET ALL THE SAGE BANK ACCOUNTS TO SYNC WITH MINERAL TREE
			List<Bank> banks = SageApi.GetBanks();
			Progress(string.Format("Loaded {0} Banks from Sage", banks.Count()));

			foreach (Bank bank in banks)
			{
				// DOES THE BANK ALREADY EXIST? SAGE PRIMARY KEY == MT EXTERNAL ID
				PaymentMethod found = MTReferenceData.FindPaymentMethodByExternalID(bank.PrimaryKey.DbValue.ToString());
				PaymentMethodRoot paymentmethodroot = Mapper.SageBankAccountToMTPaymentMethod(bank);

				if (found == null)
				{
					// CREATE
					PaymentMethod newpaymentmethod = MTApi.CreatePaymentMethod(companyid, paymentmethodroot, sessiontoken);
					if (!Continue) { return; }
					MTReferenceData.AddPaymentMethod(newpaymentmethod);
					Progress(string.Format("Payment Method {0} does not exist in MT - creating", bank.Name));
				}
				else
				{
					// UPDATE
					if (Compare.Same(bank, found))
					{
						Progress(string.Format("Payment Method {0} already exists in MT - no update required", bank.Name));
					}
					else
					{
						paymentmethodroot.paymentMethod.id = found.id;
						paymentmethodroot.paymentMethod.externalId = ""; // CAN'T UPDATE WITH AN EXTERNALID
						MTApi.UpdatePaymentMethod(paymentmethodroot, sessiontoken);
						if (!Continue) { return; }
						Progress(string.Format("PaymentMethod {0} already exists in MT - updating", bank.Name));
					}
				}
			}

			Progress("Finish Syncing Cost Centres/Locations");
			return;
		}

		#endregion

		#region DEPARTMENTS

		public static void SageDepartmentsToMineralTreeDepartments(string companyid, string sessiontoken)
		{
			Progress("Start Syncing Departments...");
			// GET ALL THE SAGE DEPARTMENTS TO SYNC WITH MINERAL TREE
			List<SageDepartment> departments = SageApi.GetDepartments();
			Progress(string.Format("Loaded {0} Departments from Sage", departments.Count()));

			foreach (SageDepartment department in departments)
			{
				if (department.Name == null || department.Name.Length == 0) { continue; } // SKIP DEPARTMENTS WITH NO NAME AS MT DOESN'T ALLOW DEPARTMENTS WITH NO NAME
				// DOES THE DEPARTMENT ALREADY EXIST? SAGE PRIMARY KEY == MT EXTERNAL ID
				Department found = MTReferenceData.FindDepartmentByExternalID(department.PrimaryKey.DbValue.ToString());
				DepartmentRoot departmentroot = Mapper.SageDepartmentToMTDepartment(department);

				if (found == null)
				{
					// CREATE
					Department newdepartment = MTApi.CreateDepartment(companyid, departmentroot, sessiontoken);
					if (!Continue) { return; }
					MTReferenceData.AddDepartment(newdepartment);
					Progress(string.Format("Department {0} does not exist in MT - creating", department.Name));
				}
				else
				{
					// UPDATE
					if (Compare.Same(department, found))
					{
						Progress(string.Format("Department {0} already exists in MT - no update required", department.Name));
					}
					else
					{
						departmentroot.department.id = found.id;
						departmentroot.department.externalId = ""; // CAN'T UPDATE WITH AN EXTERNALID
						MTApi.UpdateDepartment(departmentroot, sessiontoken);
						if (!Continue) { return; }
						Progress(string.Format("Department {0} already exists in MT - updating", department.Name));
					}
				}
			}

			Progress("Finish Syncing Departments");
			return;
		}

		#endregion

		#region STOCKITEMS / ITEMS

		public static void SageStockItemsToMineralTreeItems(string companyid, string sessiontoken)
		{
			// GET ALL THE SAGE STOCKITEMS TO SYNC WITH MINERAL TREE
			Progress("Start Syncing Stock Items...");
			// GET ALL THE SAGE DEPARTMENTS TO SYNC WITH MINERAL TREE
			List<StockItem> stockitems = SageApi.GetStockItems();
			Progress(string.Format("Loaded {0} Stock Items from Sage", stockitems.Count()));

			foreach (StockItem stockitem in stockitems)
			{
				// DOES THE STOCKITEM ALREADY EXIST? SAGE PRIMARY KEY == MT EXTERNAL ID
				Item found = MTReferenceData.FindItemByExternalID(stockitem.PrimaryKey.DbValue.ToString());
				ItemRoot itemroot = Mapper.SageStockItemToMTItem(stockitem);

				if (found == null)
				{
					// CREATE
					Item newitem = MTApi.CreateItem(companyid, itemroot, sessiontoken);
					if (!Continue) { return; }
					MTReferenceData.AddItem(newitem);
					Progress(string.Format("Item {0} does not exist in MT - creating", stockitem.Name));
				}
				else
				{
					// UPDATE
					if (Compare.Same(stockitem, found))
					{
						Progress(string.Format("Item {0} already exists in MT - no update required", stockitem.Name));
					}
					else
					{
						itemroot.item.id = found.id;
						itemroot.item.externalId = ""; // CAN'T UPDATE WITH AN EXTERNALID
						MTApi.UpdateItem(itemroot, sessiontoken);
						if (!Continue) { return; }
						Progress(string.Format("Item {0} already exists in MT - updating", stockitem.Name));
					}
				}
			}

			Progress("Finish Syncing Stock Items");
			return;
		}

		#endregion

		#region VAT RATES / CLASSIFICATION

		public static void SageVatRatesToMineralTreeClasses(string companyid, string sessiontoken)
		{
			Progress("Start Syncing Vat Rates...");
			// GET ALL THE SAGE VAT RATES TO SYNC WITH MINERAL TREE
			List<TaxCode> vatrates = SageApi.GetVatRates();
			Progress(string.Format("Loaded {0} Vat Rates from Sage", vatrates.Count()));
			
			foreach (TaxCode taxcode in vatrates)
			{
				// DOES THE VAT RATE ALREADY EXIST? SAGE PRIMARY KEY == MT EXTERNAL ID
				Classification found = MTReferenceData.FindClassificationByExternalID(taxcode.PrimaryKey.DbValue.ToString());
				ClassificationRoot classificationroot = Mapper.SageTaxCodeToMTClassification(taxcode);

				if (found == null)
				{
					// CREATE
					Classification newclassification = MTApi.CreateClassification(companyid, classificationroot, sessiontoken);
					if (!Continue) { return; }
					MTReferenceData.AddClassification(newclassification);
					Progress(string.Format("Vat Rate {0} does not exist in MT - creating", taxcode.Name));
				}
				else
				{
					if (Compare.Same(taxcode, found))
					{
						Progress(string.Format("Vat Rate {0} already exists in MT - no update required", taxcode.Name));
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
			}

			Progress("Finish Syncing Vat Rates");
		}

		#endregion

		#region NOMINAL CODES / GLACOUUNTS

		public static void SageNominalCodesToMineralTreeGLAccounts(string companyid, string sessiontoken)
		{
			Progress("Start Syncing GLCodes...");
			// GET ALL THE SAGE NOMINAL CODES TO SYNC WITH MINERAL TREE
			List<NominalCode> nominalcodes = SageApi.GetNominalCodes();
			Progress(string.Format("Loaded {0} Nominal Codes from Sage", nominalcodes.Count()));

			foreach (NominalCode nominalcode in nominalcodes)
			{
				// DOES THE NOMINAL CODE ALREADY EXIST? SAGE PRIMARY KEY == MT EXTERNAL ID
				GlAccount found = MTReferenceData.FindGlAccountByExternalID(nominalcode.PrimaryKey.DbValue.ToString());
				GlAccountRoot glaccountroot = Mapper.SageNominalCodeToMTGlAccount(nominalcode);

				if (found == null)
				{
					// CREATE
					GlAccount newglaccount = MTApi.CreateGlAccount(companyid, glaccountroot, sessiontoken);
					if (!Continue) { return; }
					MTReferenceData.AddGlAccount(newglaccount);
					Progress(string.Format("GL Account {0} does not exist in MT - creating", nominalcode.Name));
				}
				else
				{
					if (Compare.Same(nominalcode, found))
					{
						Progress(string.Format("GL Account {0} already exists in MT - no update required", nominalcode.Name));
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
			}

			Progress("Finish Syncing GL Accounts");
		}

		#endregion

		#region COST CENTRES - LOCATIONS

		public static void SageCostCentresToMineralTreeLocations(string companyid, string sessiontoken)
		{
			Progress("Start Syncing Cost Centres/Locations...");
			// GET ALL THE SAGE COST CENTRES TO SYNC WITH MINERAL TREE
			List<CostCentre> costcentres = SageApi.GetCostCentres();
			Progress(string.Format("Loaded {0} Cost Centres from Sage", costcentres.Count()));

			foreach (CostCentre costcentre in costcentres)
			{
				// DOES THE NOMINAL CODE ALREADY EXIST? SAGE PRIMARY KEY == MT EXTERNAL ID
				Location found = MTReferenceData.FindLocationByExternalID(costcentre.PrimaryKey.DbValue.ToString());
				LocationRoot locationroot = Mapper.SageCostCentreToMTLocation(costcentre);

				if (found == null)
				{
					// CREATE
					Location newlocation = MTApi.CreateLocation(companyid, locationroot, sessiontoken);
					if (!Continue) { return; }
					MTReferenceData.AddLocation(newlocation);
					Progress(string.Format("Location {0} does not exist in MT - creating", costcentre.Name));
				}
				else
				{
					// UPDATE
					if (Compare.Same(costcentre, found))
					{
						Progress(string.Format("Location {0} already exists in MT - no update required", costcentre.Name));
					}
					else
					{
						locationroot.location.id = found.id;
						locationroot.location.externalId = ""; // CAN'T UPDATE WITH AN EXTERNALID
						MTApi.UpdateLocation(locationroot, sessiontoken);
						if (!Continue) { return; }
						Progress(string.Format("Location {0} already exists in MT - updating", costcentre.Name));
					}
				}
			}

			Progress("Finish Syncing Cost Centres/Locations");
			return;
		}

		#endregion

		#region PAYMENT TERMS

		public static void SagePaymentTermsToMineralTreeTerms(string companyid, string sessiontoken)
		{
			// GET ALL THE SAGE PAYMENT TERMS TO SYNC WITH MINERAL TREE
			List<Tuple<decimal, int, int>> terms = SageApi.GetPaymentTerms();

			foreach (Tuple<decimal, int, int> term in terms)
			{
				TermRoot termroot = Mapper.SagePaymentTermsToMTTerms(term);
				MTApi.CreateTerm(companyid, termroot, sessiontoken);
				if (!Continue) { return; }
			}

			return;
		}

		#endregion

		#region PURCHASE ORDERS

		public static void SageLivePurchaseOrdersToMineralTreePurchaseOrders(string companyid, string sessiontoken)
		{
			Progress("Start Syncing Sage Live Purchase Orders...");
			// GET ALL THE SAGE PURCHASE ORDERS TO SYNC WITH MINERAL TREE
			List<POPOrder> orders = SageApi.GetLivePurchaseOrders(); // LIVE
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
						PurchaseOrderRoot poroot = Mapper.SagePurchaseOrderToMTPurchaseOrder(order);
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
							PurchaseOrderRoot po = Mapper.SagePurchaseOrderToMTPurchaseOrder(order);
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
				POPOrder sagepo = SageApi.GetPurchaseOrderByPrimaryKey(mtpo.externalId);

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

		public static void SageHistoricalInvoicesToMineralTreeBills(string companyid, string sessiontoken, DateTime from)
		{
			Progress("Start Loading Historical Invoices...");
			List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry> invoices = SageApi.GetHistoricalInvoices(from);
			Progress(string.Format("Loaded {0} Historical Invoices from Sage", invoices.Count()));

			foreach (Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry invoice in invoices)
			{
				// DOES IT ALREADY EXIST?
				Bill bill = MTApi.GetBillByExternalID(companyid, sessiontoken, invoice.PrimaryKey.DbValue.ToString());
				if (bill == null)
				{
					BillRoot billroot = Mapper.SageInvoiceToMTBill(invoice);
					MTApi.CreateBill(companyid, billroot, sessiontoken);
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

		public static void NewSageInvoicesToMineralTreeBills(string companyid, string sessiontoken)
		{
			Progress("Start Syncing new Sage Invoices to Mineral Tree...");
			List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry> invoices = SageApi.GetNewInvoices(SyncSettings.StartDate);
			Progress(string.Format("Loaded {0} new Invoices from Sage", invoices.Count()));

			foreach (Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry entry in invoices)
			{
				// DOES THE BILL ALREADY EXIST IN MT??
				Bill found = MTApi.GetBillByExternalID(companyid, sessiontoken, entry.PrimaryKey.DbValue.ToString());

				if (found == null)
				{
					// CREATE IT
					BillRoot billroot = Mapper.SageInvoiceToMTBill(entry);
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

		public static void NewMineralTreeBillsToSageInvoices(string companyid, string sessiontoken)
		{
			Progress("Start Syncing new Mineral Tree Bills to Sage...");
			// GET THE UNPROCESSED BILLS FROM MT
			List<Bill> bills = MTApi.GetNewBillsWithStatusOpenOrPendingSettlement(companyid, sessiontoken);
			Progress(string.Format("Loaded {0} new Bills from Mineral Tree", bills.Count()));

			foreach (Bill bill in bills)
			{
				// NEW BILL SO CREATE THE INVOICE IN SAGE
				Vendor vendor = MTReferenceData.FindVendorByID(bill.vendor.id);

				List<LineItem> lineitems = new List<LineItem>();
				foreach(CompanyItem item in bill.items)
				{
					if(item.glAccount != null && item.classification != null && item.department != null)
					{
						lineitems.Add(new LineItem()
						{
							GLAccountID = item.glAccount.externalId,
							DepartmentID = item.department.externalId,
							ClassificationID = item.classification.externalId,
							Description = item.description,
							NetAmount = PriceConverter.ToDecimal(item.netAmount.amount, item.netAmount.precision),
							TaxAmount = PriceConverter.ToDecimal(item.taxAmount.amount, item.taxAmount.precision)
						});
					}
				}

				string id = SageApi.CreateInvoice(vendor.externalId,
					bill.invoiceNumber,
					bill.transactionDate,
					PriceConverter.ToDecimal(bill.amount.amount, bill.amount.precision),
					PriceConverter.ToDecimal(bill.totalTaxAmount.amount, bill.totalTaxAmount.precision),
					lineitems);

				if (id.Length > 0)
				{
					// UPDATE THE MT BILL EXTERNAL ID WITH THE PRIMARY KEY FROM SAGE
					Bill update = new Bill()
					{
						id = bill.id,
						externalId = id
					};
					BillRoot billroot = new BillRoot();
					billroot.bill = update;
					MTApi.UpdateBill(billroot, sessiontoken);
					if (!Continue) { return; }
					Progress(string.Format("Bill {0} does not exist in Sage - creating", bill.invoiceNumber));
				}
				else
				{
					Progress(string.Format("Bill {0} could not create in Sage", bill.invoiceNumber));
				}
				
			}

			Progress("Finish Syncing new Mineral Tree Bills to Sage");
		}

		#endregion

		#region PAYMENTS

		public static void NewMineralTreePaymentsToSagePayments(string companyid, string sessiontoken)
		{
			Progress("Start Syncing new Mineral Tree Payments to Sage...");
			// GET THE UNPROCESSED PAYMENTS FROM MT
			List<Payment> payments = MTApi.GetPayments(companyid, sessiontoken);
			Progress(string.Format("Loaded {0} new Payments from Mineral Tree", payments.Count()));

			foreach (Payment payment in payments)
			{
				SageSupplier supplier = SageApi.GetSupplierByPrimaryKey(payment.vendor.externalId);
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
				string id = SageApi.CreatePayment(supplier, payment.paymentMethod.externalId, payment.transactionDate, paymentamount, 0);

				// UPDATE THE MT PAYMENT EXTERNAL ID WITH THE PRIMARY KEY FROM SAGE
				if (id.Length > 0)
				{
					// ALLOCATE THE PAYMENTS TO THE CORRECT INVOICES
					Sage.Accounting.PurchaseLedger.PurchaseBankPaymentPosting sagepayment = SageApi.GetPaymentByPrimaryKey(id);
					foreach (Bill bill in payment.bills)
					{
						decimal amount = Utils.PriceConverter.ToDecimal(bill.appliedPaymentAmount.amount, bill.appliedPaymentAmount.precision);
						Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry invoice = SageApi.GetInvoiceByInvoiceNumber(bill.invoiceNumber);

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

							Sage.ObjectStore.Filter filter = new Sage.ObjectStore.Filter(Sage.Accounting.TradeLedger.PostedTradingAccountEntry.FIELD_INSTRUMENTNO, invoice.InstrumentNo);

							allocation.DebitEntries.Query.Filters.Add(filter);
							allocation.DebitEntries.Find();

							allocation.ResetDebitAllocationHandler(allocation.PurchaseDebitEntries);

							Sage.Accounting.TradeLedger.TradingAllocationEntryView debitentry = allocation.DebitEntries.First;

							if (debitentry != null)
							{
								allocation.DebitEntries.First.AllocateThisTime = amount;
								allocation.Validate();
								allocation.Allocate();
								Progress(string.Format("Allocated Payment of {0} to Invoice {1}", amount, bill.invoiceNumber));
							}
							else
							{
								Progress(string.Format("Allocation to {0} Failed: Could not get Debit Entry", supplier.Name));
							}

							// EVERYTHING WORKED SO UPDATE THE EXTERNAL ID IN MT
							Payment update = new Payment(){id = payment.id, externalId = id};
							PaymentRoot paymentroot = new PaymentRoot() { payment = update };
							MTApi.UpdatePayment(paymentroot, sessiontoken);
							if (!Continue) { return; }
						}
						catch(Exception ex)
						{
							Error(ex.ToString());
							Progress(string.Format("Allocation exception ({0}) - check log file for more details", ex.Message));
						}
						finally
						{
							if (allocation != null)
							{
								allocation.Warnings -= new Sage.Common.DataAccess.BusinessObject.WarningHandler(PurchaseAllocationsAdjustment_Warnings);
							}
						}
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

		public static void SageCreditNotesToMineralTreeCredit(string companyid, string sessiontoken)
		{
			Progress("Start Syncing Credit Notes...");
			// GET ALL THE SAGE CREDIT NOTES TO SYNC WITH MINERAL TREE
			List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry> creditnotes = SageApi.GetCreditNotes();
			Progress(string.Format("Loaded {0} Credit Notes from Sage", creditnotes.Count()));

			foreach (Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry creditnote in creditnotes)
			{
				// DOES THE CREDITNOTE ALREADY EXIST? SAGE PRIMARY KEY == MT EXTERNAL ID
				Credit found = MTApi.GetCreditByExternalID(companyid, sessiontoken, creditnote.PrimaryKey.DbValue.ToString());
				CreditRoot creditroot = Mapper.SageCreditNoteToMTCredit(creditnote);

				if (found == null)
				{
					// CREATE
					Credit newcredit = MTApi.CreateCredit(companyid, creditroot, sessiontoken);
					if (!Continue) { return; }
					Progress(string.Format("Credit {0} does not exist in MT - creating", creditnote.InstrumentNo));
				}
				else
				{
					// UPDATE
					creditroot.credit.id = found.id;
					creditroot.credit.externalId = ""; // CAN'T UPDATE WITH AN EXTERNALID
					MTApi.UpdateCredit(creditroot, sessiontoken);
					if (!Continue) { return; }
					Progress(string.Format("Credit {0} already exists in MT - updating", creditnote.InstrumentNo));
				}
			}

			Progress("Finish Syncing Credit Notes");
		}

		#endregion
	}
}
