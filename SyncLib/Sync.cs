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

			Progress(string.Format("Finding company to sync to: {0}", SyncSettings.CompanyNameToSync));
			Company found = companies.Find(o => o.name == SyncSettings.CompanyNameToSync);
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

			if (!SageApi.Connect())
			{
				Progress("Failed to connect to Sage - stopping");
				Complete("Sync Failed");
				return;
			}

			Progress("Connected to Sage OK");
			Progress(string.Format("Syncing to Company: {0}", found.name));

			//SageSuppliersToMineralTreeVendors(found.id, sessiontoken);
			//SageBankAccountsToMineralTreePaymentMethods(found.id, sessiontoken); // no update
			//SageDepartmentsToMineralTreeDepartments(found.id, sessiontoken);
			//SageStockItemsToMineralTreeItems(found.id, sessiontoken);
			//SageNominalCodesToMineralTreeGLAccounts(found.id, sessiontoken); // no update
			//SageCostCentresToMineralTreeLocations(found.id, sessiontoken);
			//SagePaymentTermsToMineralTreeTerms(found.id, sessiontoken); // no update

			//SageLivePurchaseOrdersToMineralTreePurchaseOrders(found.id, sessiontoken);
			//SageInvoicesToMineralTreeBills(found.id, sessiontoken);
			NewMineralTreeBillsToSageInvoices(found.id, sessiontoken);
			//SageCreditNotesToMineralTreeCredit(found.id, sessiontoken); // no update
			
			// TODO, PAYMENTS, INVOICES

			DateTime dtfinish = DateTime.Now;
			TimeSpan tsduration = dtfinish - dtstart;
			
			Progress(string.Format("End Sync at {0} (Duration: {1}) - {2} error(s)", dtfinish.ToString("yyyy-MM-dd HH:mm:ss"), tsduration.ToString("hh\\:mm\\:ss"), Errors));
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

		public static void SageSuppliersToMineralTreeVendors(string companyid, string sessiontoken)
		{
			Progress("Start Syncing Suppliers/Vendors...");
			// GET ALL THE SAGE SUPPLIERS AND CREATE A LIST OF CORRESPONDING VENDORS TO SYNC WITH MINERAL TREE
			List<SageSupplier> suppliers = SageApi.GetSuppliers();
			Progress(string.Format("Loaded {0} Suppliers from Sage", suppliers.Count()));

			foreach (SageSupplier supplier in suppliers)
			{
				// DOES THE VENDOR ALREADY EXIST? SAGE SOURCE REFERENCE == MT EXTERNAL ID
				Vendor found = MTReferenceData.FindVendorByExternalID(supplier.SourceReference);
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

		public static void SageDepartmentsToMineralTreeDepartments(string companyid, string sessiontoken)
		{
			Progress("Start Syncing Departments...");
			// GET ALL THE SAGE DEPARTMENTS TO SYNC WITH MINERAL TREE
			List<SageDepartment> departments = SageApi.GetDepartments();
			Progress(string.Format("Loaded {0} Departments from Sage", departments.Count()));

			foreach (SageDepartment department in departments)
			{
				Progress(string.Format("Processing Department: {0}", department.Name));
				// DOES THE DEPARTMENT ALREADY EXIST? SAGE CODE == MT EXTERNAL ID
				Department found = MTReferenceData.FindDepartmentByExternalID(department.Code);
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

		#region ITEMS

		public static void SageStockItemsToMineralTreeItems(string companyid, string sessiontoken)
		{
			// GET ALL THE SAGE STOCKITEMS TO SYNC WITH MINERAL TREE
			Progress("Start Syncing Stock Items...");
			// GET ALL THE SAGE DEPARTMENTS TO SYNC WITH MINERAL TREE
			List<StockItem> stockitems = SageApi.GetStockItems();
			Progress(string.Format("Loaded {0} Stock Items from Sage", stockitems.Count()));

			foreach (StockItem stockitem in stockitems)
			{
				Progress(string.Format("Processing Stock Item: {0}", stockitem.Name));
				// DOES THE STOCKITEM ALREADY EXIST? SAGE CODE == MT EXTERNAL ID
				Item found = MTReferenceData.FindItemByExternalID(stockitem.Code);
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

		#region NOMINAL CODES / GLACOUUNTS

		public static void SageNominalCodesToMineralTreeGLAccounts(string companyid, string sessiontoken)
		{
			Progress("Start Syncing GLCodes...");
			// GET ALL THE SAGE NOMINAL CODES TO SYNC WITH MINERAL TREE
			List<NominalCode> nominalcodes = SageApi.GetNominalCodes();
			Progress(string.Format("Loaded {0} Nominal Codes from Sage", nominalcodes.Count()));

			foreach (NominalCode nominalcode in nominalcodes)
			{
				Progress(string.Format("Processing Nominal Code: {0}", nominalcode.Name));
				// DOES THE NOMINAL CODE ALREADY EXIST? SAGE REFERENCE == MT EXTERNAL ID
				GlAccount found = MTReferenceData.FindGlAccountByExternalID(nominalcode.Reference);
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
					// UPDATE - TODO: CHECK FOR CHANGES BEFORE UPDATING - AN UPDATE MAY NOT BE NEEDED
					glaccountroot.glAccount.id = found.id;
					glaccountroot.glAccount.externalId = ""; // CAN'T UPDATE WITH AN EXTERNALID
					MTApi.UpdateGlAccount(glaccountroot, sessiontoken);
					if (!Continue) { return; }
					Progress(string.Format("GL Account {0} already exists in MT - updating", nominalcode.Name));
				}
			}

			Progress("Finish Syncing GL Accounts");
			return;
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
				Progress(string.Format("Processing Cost Centre: {0}", costcentre.Name));
				// DOES THE NOMINAL CODE ALREADY EXIST? SAGE CODE == MT EXTERNAL ID
				Location found = MTReferenceData.FindLocationByExternalID(costcentre.Code);
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

		public static void SageLivePurchaseOrdersToMineralTreePurchaseOrders(string companyid, string sessiontoken)
		{
			Progress("Start Syncing Sage Live Purchase Orders...");
			//purchaseorder.externalId = poporder.DocumentNo;
			// GET ALL THE SAGE PURCHASE ORDERS TO SYNC WITH MINERAL TREE
			List<POPOrder> orders = SageApi.GetLivePurchaseOrders(); // LIVE
			Progress(string.Format("Loaded {0} Live Purchase Orders from Sage", orders.Count()));

			// PUSH/UPDATE LIVE ORDERS
			foreach (POPOrder order in orders)
			{
				Progress(string.Format("Processing PO {0}", order.DocumentNo));
				// DOES IT ALREADY EXIST IN MT?
				PurchaseOrder found = MTApi.GetPurchaseOrderByExternalID(companyid, sessiontoken, order.DocumentNo);

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
				POPOrder sagepo = SageApi.GetPurchaseOrderByDocumentNo(mtpo.externalId);

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

		#region INVOICES/BILLS

		public static bool SageInvoicesToMineralTreeBills(string companyid, string sessiontoken)
		{
			List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry> invoices = SageApi.GetPurchaseInvoices();

			//foreach (Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry invoice in invoices)
			//{
				BillRoot billroot = Mapper.SageInvoiceToMTBill(invoices[3]);
				MTApi.CreateBill(companyid, billroot, sessiontoken);
			//}

			return true;
		}

		public static bool NewMineralTreeBillsToSageInvoices(string companyid, string sessiontoken)
		{
			// GET THE UNPROCESSED BILLS FROM MT
			List<Bill> bills = MTApi.GetBillsWithNoExternalID(companyid, sessiontoken);

			foreach(Bill bill in bills)
			{
				// CREATE THE INVOICE IN SAGE
				//Vendor vendor = MTReferenceData.FindVendorByID(bill.vendor.id);
				//SageApi.CreateInvoice(vendor.externalId, bill.invoiceNumber, bill.transactionDate, bill.amount.amount, bill.totalTaxAmount.amount);
				// UPDATE THE MT BILL EXTERNAL ID WITH THE INSTRUMENT NO FROM SAGE

				bill.externalId = bill.invoiceNumber;
				BillRoot billroot = new BillRoot() { bill = bill };
				MTApi.UpdateBill(billroot, sessiontoken);
			}

			return true;
		}

		#endregion

		#region CREDIT NOTES

		public static void SageCreditNotesToMineralTreeCredit(string companyid, string sessiontoken)
		{
			// GET ALL THE SAGE STOCKITEMS TO SYNC WITH MINERAL TREE
			Progress("Start Syncing Credit Notes...");
			// GET ALL THE SAGE CREDIT NOTES TO SYNC WITH MINERAL TREE
			List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry> creditnotes = SageApi.GetCreditNotes();
			Progress(string.Format("Loaded {0} Credit Notes from Sage", creditnotes.Count()));

			foreach (Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry creditnote in creditnotes)
			{
				Progress(string.Format("Processing Credit Note: {0}", creditnote.InstrumentNo));
				// DOES THE CREDITNOTE ALREADY EXIST? SAGE INSTRUMENTNO == MT EXTERNAL ID
				Credit found = MTApi.GetCreditByExternalID(companyid, sessiontoken, creditnote.InstrumentNo);
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
			return;
		}

		#endregion
	}
}
