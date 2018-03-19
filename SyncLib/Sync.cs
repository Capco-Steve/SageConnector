using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTLib;
using MTLib.Objects;
using SageLib;
using Utils;
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
		public static event EventHandler<SyncEventArgs> OnError;
		public static event EventHandler<SyncEventArgs> OnComplete;

		private static int Errors = 0;
		private static bool Continue = true;

		static Sync()
		{
			MTApi.OnError += MTApi_OnError;
		}

		#region SYNC

		public static void SyncAll()
		{
			Errors = 0;
			Continue = true;
			DateTime dtstart = DateTime.Now;

			Progress(string.Format("Start Sync at {0}", dtstart.ToString("yyyy-MM-dd HH:mm:ss")));
			// GET SESSION TOKEN
			Progress("Getting Session Token...");
			string sessiontoken = MTApi.GetSessionToken();
			if(!Continue == true){ return;  }
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
			if (!Continue == true) { return; }
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
				Progress(string.Format("Could not locate company to sync - check app settings - stopping"));
				Complete("Sync Failed");
				return;
			}

			Progress(string.Format("Found Company: {0}, Company ID: {1}", found.name, found.id));

			Progress("Loading Reference Data from Mineral Tree...");
			MTReferenceData.LoadReferenceData(found.id, sessiontoken);
			if (!Continue == true) { return; }

			Progress(string.Format("Loaded {0} Vendors", MTReferenceData.GetVendorCount()));
			Progress(string.Format("Loaded {0} Departments", MTReferenceData.GetDepartmentCount()));
			Progress(string.Format("Loaded {0} Items", MTReferenceData.GetItemCount()));
			Progress(string.Format("Loaded {0} Gl Accounts", MTReferenceData.GetGlAccountCount()));
			Progress(string.Format("Loaded {0} Locations", MTReferenceData.GetLocationCount()));
			Progress(string.Format("Loaded {0} Payment Methods", MTReferenceData.GetPaymentMethodCount()));

			foreach (SageElement sagecompany in SyncSettings.SageCompaniesToSync)
			{
				if (!SageApi.Connect(sagecompany.Name))
				{
					Progress("Failed to connect to Sage - stopping");
					Complete("Sync Failed");
					return;
				}

				Progress("Connected to Sage OK");
				Progress(string.Format("Syncing to Company: {0}", found.name));

				if (Continue) { SageNominalCodesToMineralTreeGLAccounts(found.id, sessiontoken, sagecompany.Prefix); }
				if (Continue) { SageSuppliersToMineralTreeVendors(found.id, sessiontoken, sagecompany.Prefix); }
				if (Continue) { SageBankAccountsToMineralTreePaymentMethods(found.id, sessiontoken, sagecompany.Prefix); }
				if (Continue) { SageDepartmentsToMineralTreeDepartments(found.id, sessiontoken, sagecompany.Prefix); }
				if (Continue) { SageStockItemsToMineralTreeItems(found.id, sessiontoken, sagecompany.Prefix); }
				if (Continue) { SageCostCentresToMineralTreeLocations(found.id, sessiontoken, sagecompany.Prefix); }
				//if (Continue) { SagePaymentTermsToMineralTreeTerms(found.id, sessiontoken, sagecompany.Prefix); }	// no update
				if (Continue) { SageLivePurchaseOrdersToMineralTreePurchaseOrders(found.id, sessiontoken, sagecompany.Prefix); }
				if (Continue) { NewSageInvoicesToMineralTreeBills(found.id, sessiontoken, sagecompany.Prefix); }
				if (Continue) { NewMineralTreeBillsToSageInvoices(found.id, sessiontoken, sagecompany.Prefix); }
				if (Continue) { NewMineralTreePaymentsToSagePayments(found.id, sessiontoken, sagecompany.Prefix); }
				if (Continue) { SageCreditNotesToMineralTreeCredit(found.id, sessiontoken, sagecompany.Prefix); }
			}

			DateTime dtfinish = DateTime.Now;
			TimeSpan tsduration = dtfinish - dtstart;
			
			Progress(string.Format("End Sync at {0} (Duration: {1}) - {2} error(s)", dtfinish.ToString("yyyy-MM-dd HH:mm:ss"), tsduration.ToString("hh\\:mm\\:ss"), Errors));
			return;
		}

		#endregion

		#region HISTORICAL INVOICES UPLOAD

		public static string GetHistoricalInvoiceCount(DateTime from)
		{
			StringBuilder sb = new StringBuilder();
			foreach (SageElement sagecompany in SyncSettings.SageCompaniesToSync)
			{
				SageApi.Connect(sagecompany.Name);
				sb.AppendFormat("Company: {0}, Invoices: {1}\r\n", sagecompany.Name, SageApi.GetHistoricalInvoices(from).Count());
			}
			return sb.ToString();
		}

		public static void LoadHistoricalInvoices(DateTime from)
		{
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
			MTReferenceData.LoadReferenceData(found.id, sessiontoken);
			if (!Continue == true) { return; }

			Progress(string.Format("Loaded {0} Vendors", MTReferenceData.GetVendorCount()));
			Progress(string.Format("Loaded {0} Departments", MTReferenceData.GetDepartmentCount()));
			Progress(string.Format("Loaded {0} Items", MTReferenceData.GetItemCount()));
			Progress(string.Format("Loaded {0} Gl Accounts", MTReferenceData.GetGlAccountCount()));
			Progress(string.Format("Loaded {0} Locations", MTReferenceData.GetLocationCount()));
			Progress(string.Format("Loaded {0} Payment Methods", MTReferenceData.GetPaymentMethodCount()));

			foreach (SageElement sagecompany in SyncSettings.SageCompaniesToSync)
			{

				if (!SageApi.Connect(sagecompany.Name))
				{
					Progress("Failed to connect to Sage - stopping");
					Complete("Load Failed");
					return;
				}

				Progress("Connected to Sage OK");
				Progress(string.Format("Loading Invoices from {0} to Company: {1}", sagecompany.Name, found.name));

				// VENDORS MUST BE SYNCED FIRST BECAUSE INVOICES HAVE A RELATIONSHIP WITH VENDORS
				if (Continue) { SageSuppliersToMineralTreeVendors(found.id, sessiontoken, sagecompany.Prefix); }
				if (Continue) { SageHistoricalInvoicesToMineralTreeBills(found.id, sessiontoken, from, sagecompany.Prefix); };
			}
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

		public static void SageSuppliersToMineralTreeVendors(string companyid, string sessiontoken, string prefix)
		{
			Progress("Start Syncing Suppliers/Vendors...");
			// GET ALL THE SAGE SUPPLIERS AND CREATE A LIST OF CORRESPONDING VENDORS TO SYNC WITH MINERAL TREE
			List<SageSupplier> suppliers = SageApi.GetSuppliers();
			Progress(string.Format("Loaded {0} Suppliers from Sage", suppliers.Count()));

			//SageSupplier supplier = suppliers[46]; -- BBB RocketSpace
			foreach (SageSupplier supplier in suppliers)
			{
				// DOES THE VENDOR ALREADY EXIST? SAGE PRIMARY KEY == MT EXTERNAL ID
				Vendor found = MTReferenceData.FindVendorByExternalID(ExternalIDFormatter.AppendPrefix(supplier.PrimaryKey.DbValue.ToString(), prefix));
				VendorRoot vendorroot = Mapper.SageSupplierToMTVendor(supplier, prefix);

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

		public static void SageBankAccountsToMineralTreePaymentMethods(string companyid, string sessiontoken, string prefix)
		{
			Progress("Start Syncing Bank Accounts/Payment Methods...");
			// GET ALL THE SAGE BANK ACCOUNTS TO SYNC WITH MINERAL TREE
			List<Bank> banks = SageApi.GetBanks();
			Progress(string.Format("Loaded {0} Banks from Sage", banks.Count()));

			foreach (Bank bank in banks)
			{
				// DOES THE BANK ALREADY EXIST? SAGE PRIMARY KEY == MT EXTERNAL ID
				PaymentMethod found = MTReferenceData.FindPaymentMethodByExternalID(ExternalIDFormatter.AppendPrefix(bank.PrimaryKey.DbValue.ToString(), prefix));
				PaymentMethodRoot paymentmethodroot = Mapper.SageBankAccountToMTPaymentMethod(bank, prefix);

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

		public static void SageDepartmentsToMineralTreeDepartments(string companyid, string sessiontoken, string prefix)
		{
			Progress("Start Syncing Departments...");
			// GET ALL THE SAGE DEPARTMENTS TO SYNC WITH MINERAL TREE
			List<SageDepartment> departments = SageApi.GetDepartments();
			Progress(string.Format("Loaded {0} Departments from Sage", departments.Count()));

			foreach (SageDepartment department in departments)
			{
				// DOES THE DEPARTMENT ALREADY EXIST? SAGE PRIMARY KEY == MT EXTERNAL ID
				Department found = MTReferenceData.FindDepartmentByExternalID(ExternalIDFormatter.AppendPrefix(department.PrimaryKey.DbValue.ToString(), prefix));
				DepartmentRoot departmentroot = Mapper.SageDepartmentToMTDepartment(department, prefix);

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

		public static void SageStockItemsToMineralTreeItems(string companyid, string sessiontoken, string prefix)
		{
			// GET ALL THE SAGE STOCKITEMS TO SYNC WITH MINERAL TREE
			Progress("Start Syncing Stock Items...");
			// GET ALL THE SAGE DEPARTMENTS TO SYNC WITH MINERAL TREE
			List<StockItem> stockitems = SageApi.GetStockItems();
			Progress(string.Format("Loaded {0} Stock Items from Sage", stockitems.Count()));

			foreach (StockItem stockitem in stockitems)
			{
				// DOES THE STOCKITEM ALREADY EXIST? SAGE PRIMARY KEY == MT EXTERNAL ID
				Item found = MTReferenceData.FindItemByExternalID(ExternalIDFormatter.AppendPrefix(stockitem.PrimaryKey.DbValue.ToString(), prefix));
				ItemRoot itemroot = Mapper.SageStockItemToMTItem(stockitem, prefix);

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

		#region NOMINAL CODES / GLACOUUNTS

		public static void SageNominalCodesToMineralTreeGLAccounts(string companyid, string sessiontoken, string prefix)
		{
			Progress("Start Syncing GLCodes...");
			// GET ALL THE SAGE NOMINAL CODES TO SYNC WITH MINERAL TREE
			List<NominalCode> nominalcodes = SageApi.GetNominalCodes();
			Progress(string.Format("Loaded {0} Nominal Codes from Sage", nominalcodes.Count()));

			foreach (NominalCode nominalcode in nominalcodes)
			{
				// DOES THE NOMINAL CODE ALREADY EXIST? SAGE PRIMARY KEY == MT EXTERNAL ID
				GlAccount found = MTReferenceData.FindGlAccountByExternalID(ExternalIDFormatter.AppendPrefix(nominalcode.PrimaryKey.DbValue.ToString(), prefix));
				GlAccountRoot glaccountroot = Mapper.SageNominalCodeToMTGlAccount(nominalcode, prefix);

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
			return;
		}

		#endregion

		#region COST CENTRES - LOCATIONS

		public static void SageCostCentresToMineralTreeLocations(string companyid, string sessiontoken, string prefix)
		{
			Progress("Start Syncing Cost Centres/Locations...");
			// GET ALL THE SAGE COST CENTRES TO SYNC WITH MINERAL TREE
			List<CostCentre> costcentres = SageApi.GetCostCentres();
			Progress(string.Format("Loaded {0} Cost Centres from Sage", costcentres.Count()));

			foreach (CostCentre costcentre in costcentres)
			{
				// DOES THE NOMINAL CODE ALREADY EXIST? SAGE PRIMARY KEY == MT EXTERNAL ID
				Location found = MTReferenceData.FindLocationByExternalID(ExternalIDFormatter.AppendPrefix(costcentre.PrimaryKey.DbValue.ToString(), prefix));
				LocationRoot locationroot = Mapper.SageCostCentreToMTLocation(costcentre, prefix);

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

		public static void SagePaymentTermsToMineralTreeTerms(string companyid, string sessiontoken, string prefix)
		{
			// GET ALL THE SAGE PAYMENT TERMS TO SYNC WITH MINERAL TREE
			List<Tuple<decimal, int, int>> terms = SageApi.GetPaymentTerms();

			foreach (Tuple<decimal, int, int> term in terms)
			{
				TermRoot termroot = Mapper.SagePaymentTermsToMTTerms(term, prefix);
				MTApi.CreateTerm(companyid, termroot, sessiontoken);
				if (!Continue) { return; }
			}

			return;
		}

		#endregion

		#region PURCHASE ORDERS

		public static void SageLivePurchaseOrdersToMineralTreePurchaseOrders(string companyid, string sessiontoken, string prefix)
		{
			Progress("Start Syncing Sage Live Purchase Orders...");
			// GET ALL THE SAGE PURCHASE ORDERS TO SYNC WITH MINERAL TREE
			List<POPOrder> orders = SageApi.GetLivePurchaseOrders(); // LIVE
			Progress(string.Format("Loaded {0} Live Purchase Orders from Sage", orders.Count()));

			// PUSH/UPDATE LIVE ORDERS
			foreach (POPOrder order in orders)
			{
				// DOES IT ALREADY EXIST IN MT?
				PurchaseOrder found = MTApi.GetPurchaseOrderByExternalID(companyid, sessiontoken, ExternalIDFormatter.AppendPrefix(order.PrimaryKey.DbValue.ToString(), prefix));

				if (order.DocumentStatus == Sage.Accounting.OrderProcessing.DocumentStatusEnum.EnumDocumentStatusLive)
				{
					if (found == null)
					{
						// NO, CREATE IT
						PurchaseOrderRoot poroot = Mapper.SagePurchaseOrderToMTPurchaseOrder(order, prefix);
						PurchaseOrder newpurchaseorder = MTApi.CreatePurchaseOrder(companyid, poroot, sessiontoken);
						if (!Continue) { return; }
						Progress(string.Format("PO {0} does not exist in MT - creating it", order.DocumentNo));
					}
					else
					{
						Progress(string.Format("PO already exists, state: {0}", found.state));
						// YES, CHECK STATUS
						if (found.state.ToLower() == "closed")
						{
							// UPDATE SAGE
							order.DocumentStatus = Sage.Accounting.OrderProcessing.DocumentStatusEnum.EnumDocumentStatusComplete;
							order.Update();
							Progress("State is closed so updating Sage state to complete");
						}
						else
						{
							Progress("State is not closed so no update required");
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

			Progress("Finish Syncing MT PendingBilling Purchase Orders");

			return;
		}

		#endregion

		#region HISTORICAL INVOICES/BILLS

		public static void SageHistoricalInvoicesToMineralTreeBills(string companyid, string sessiontoken, DateTime from, string prefix)
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
					BillRoot billroot = Mapper.SageInvoiceToMTBill(invoice, prefix);
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

		public static void NewSageInvoicesToMineralTreeBills(string companyid, string sessiontoken, string prefix)
		{
			List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry> invoices = SageApi.GetNewInvoices(SyncSettings.StartDate);

			foreach (Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry entry in invoices)
			{
				// DOES THE BILL ALREADY EXIST IN MT??
				Bill found = MTApi.GetBillByExternalID(companyid, sessiontoken, ExternalIDFormatter.AppendPrefix(entry.PrimaryKey.DbValue.ToString(), prefix));

				if (found == null)
				{
					// CREATE IT
					BillRoot billroot = Mapper.SageInvoiceToMTBill(entry, prefix);
					MTApi.CreateBill(companyid, billroot, sessiontoken);
					if (!Continue) { return; }
				}
			}

			return;
		}

		public static void NewMineralTreeBillsToSageInvoices(string companyid, string sessiontoken, string prefix)
		{
			// GET THE UNPROCESSED BILLS FROM MT
			List<Bill> bills = MTApi.GetBillsWithNoExternalID(companyid, sessiontoken);

			foreach (Bill bill in bills)
			{
				// CREATE THE INVOICE IN SAGE
				Vendor vendor = MTReferenceData.FindVendorByID(bill.vendor.id);
				string id = SageApi.CreateInvoice(ExternalIDFormatter.RemovePrefix(vendor.externalId, prefix), 
					bill.invoiceNumber, 
					bill.transactionDate, 
					PriceConverter.ToDecimal(bill.amount.amount, bill.amount.precision),
					PriceConverter.ToDecimal(bill.totalTaxAmount.amount, bill.totalTaxAmount.precision));

				if (id.Length > 0)
				{
					// UPDATE THE MT BILL EXTERNAL ID WITH THE PRIMARY KEY FROM SAGE
					Bill update = new Bill()
					{
						id = bill.id,
						externalId = ExternalIDFormatter.AppendPrefix(id, prefix)
					};
					BillRoot billroot = new BillRoot();
					billroot.bill = update;
					MTApi.UpdateBill(billroot, sessiontoken);
					if (!Continue) { return; }
				}
			}

			return;
		}

		#endregion

		#region PAYMENTS

		public static void NewMineralTreePaymentsToSagePayments(string companyid, string sessiontoken, string prefix)
		{
			// GET THE UNPROCESSED PAYMENTS FROM MT
			List<Payment> payments = MTApi.GetPayments(companyid, sessiontoken);

			foreach (Payment payment in payments)
			{
				if (payment.status == "Approved") // ONLY PROCESS APPROVED PAYMENTS
				{
					// CREATE THE PAYMENT IN SAGE - TODO: ERROR CHECKING
					string invoiceid = "";
					if (payment.bills.Count() > 0)
						invoiceid = ExternalIDFormatter.RemovePrefix(payment.bills[0].externalId, prefix);

					// BILLS, FUNDING METHODS???
					// PAYMENT ALLOCATION????

					string id = SageApi.CreatePayment(ExternalIDFormatter.RemovePrefix(payment.vendor.externalId, prefix),
						ExternalIDFormatter.RemovePrefix(payment.paymentMethod.externalId, prefix),
						invoiceid,
						payment.transactionDate,
						PriceConverter.ToDecimal(payment.amount.amount, 2),
						0);

					// UPDATE THE MT PAYMENT EXTERNAL ID WITH THE PRIMARY KEY FROM SAGE
					if (id.Length > 0)
					{
						Payment update = new Payment()
						{
							id = payment.id,
							externalId = ExternalIDFormatter.AppendPrefix(id, prefix)
						};
						PaymentRoot paymentroot = new PaymentRoot();
						paymentroot.payment = update;
						MTApi.UpdatePayment(paymentroot, sessiontoken);
						if (!Continue) { return; }
					}
				}
			}

			return;
		}

		#endregion

		#region CREDIT NOTES

		public static void SageCreditNotesToMineralTreeCredit(string companyid, string sessiontoken, string prefix)
		{
			Progress("Start Syncing Credit Notes...");
			// GET ALL THE SAGE CREDIT NOTES TO SYNC WITH MINERAL TREE
			List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry> creditnotes = SageApi.GetCreditNotes();
			Progress(string.Format("Loaded {0} Credit Notes from Sage", creditnotes.Count()));

			foreach (Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry creditnote in creditnotes)
			{
				// DOES THE CREDITNOTE ALREADY EXIST? SAGE PRIMARY KEY == MT EXTERNAL ID
				Credit found = MTApi.GetCreditByExternalID(companyid, sessiontoken, ExternalIDFormatter.AppendPrefix(creditnote.PrimaryKey.DbValue.ToString(), prefix));
				CreditRoot creditroot = Mapper.SageCreditNoteToMTCredit(creditnote, prefix);

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
			return;
		}

		#endregion
	}
}
