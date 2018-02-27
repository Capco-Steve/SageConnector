using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTLib;
using MTLib.Objects;
using SageLib;
using Sage.Accounting;
using SageSupplier = Sage.Accounting.PurchaseLedger.Supplier;
using SageDepartment = Sage.Accounting.SystemManager.Department;
using SageStockItem = Sage.Accounting.Stock.StockItem;
using SageNominalCode = Sage.Accounting.NominalLedger.NominalCode;
using SageCostCentre = Sage.Accounting.SystemManager.CostCentre;
using Bank = Sage.Accounting.CashBook.Bank;
using POPOrder = Sage.Accounting.POP.POPOrder;
using PostedPurchaseAccountEntry = Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry;

namespace SyncLib
{
	/// <summary>
	/// Performs object mapping between sage and mineral tree
	/// </summary>
	public static class Mapper
	{
		#region VENDOR

		public static VendorRoot SageSupplierToMTVendor(SageSupplier supplier)
		{
			VendorRoot vendorroot = new VendorRoot();
			Vendor vendor = new Vendor();

			vendor.id = "";
			vendor.form1099Enabled = false;
			vendor.externalId = supplier.SourceReference;
			vendor.name = supplier.Name;
			vendor.active = true; // TODO - MIGHT CHANGE WHEN WE FIGURE OUT THE MATCHING FIELD

			vendor.address = new Address
			{
				name = "",
				address1 = supplier.MainAddress.AddressLine1,
				address2 = supplier.MainAddress.AddressLine2,
				address3 = "",
				address4 = "",
				postalCode = supplier.MainAddress.PostCode,
				town = supplier.MainAddress.AddressLine3,
				ctrySubDivision = supplier.MainAddress.AddressLine4,
				country = supplier.MainAddress.Country
			};

			vendor.legalName = supplier.Name;
			vendor.vendorType = "CORPORATION";

			vendor.phones = new List<Phone>()
			{
				new Phone { number = supplier.MainTelephone, isFax = false },
				new Phone { number = supplier.MainFax, isFax = true }
			};

			vendor.fundingMethods = new List<FundingMethod>()
			{
				new FundingMethod()
				{
					type = "ACH",
					bankAccount = new BankAccount()
					{
						accountNumber = supplier.SupplierBanks.PrimaryBank.BankAccountReference,
						routingNumber = supplier.SupplierBanks.PrimaryBank.BankSortCode
					}
				}
			};

			vendor.emails = new List<string>() { supplier.Contacts[0].Emails[0].Value };

			if (supplier.Memos.Count > 0)
				vendor.memo = supplier.Memos[0].Note; // JUST USE THE FIRST MEMO
			else
				vendor.memo = "";

			vendor.customerAccount = "";
			//vendor.primarySubsidiary = null;// new PrimarySubsidiary() {id = "1234" };
			vendor.taxId = "";
			vendor.vatNumber = supplier.TaxRegistrationCode;
			/*vendor.vendorCompanyDefault = null; new VendorCompanyDefault() {defaultClassId="1",
																		defaultCustomerId="1",
																		defaultDepartmentId="1",
																		defaultEmployeeId="1",
																		defaultItemId="1",
																		defaultLocationId="1",
																		defaultProjectId="1",
																		defaultTermsId="1",
																		defaultDebitAccountId="1",
																		defaultApAccountId="1",
																		defaultExpenseAccountId="1"};*/

			vendorroot.vendor = vendor;
			return vendorroot;
		}

		#endregion

		#region DEPARTMENT

		public static DepartmentRoot SageDepartmentToMTDepartment(SageDepartment sagedepartment)
		{
			DepartmentRoot departmentroot = new DepartmentRoot();
			Department department = new Department();

			department.id = "";
			//department.subsidiaries
			department.externalId = sagedepartment.Code;
			department.name = sagedepartment.Name;
			department.active = true;

			departmentroot.department = department;
			return departmentroot;
		}

		#endregion

		#region STOCKITEM / ITEM

		public static ItemRoot SageStockItemToMTItem(SageStockItem sagestockitem)
		{
			ItemRoot itemroot = new ItemRoot();
			Item item = new Item();

			item.id = "";
			item.type = "INVENTORY"; //???

			Cost cost = new Cost()
			{
				amount = sagestockitem.PriceBands[0].StockItemPrices[0].Price,
				precision = 2
			};
			item.cost = cost;

			Residual residual = new Residual()
			{
				amount = 0 // ???
			};
			item.residual = residual;

			item.name = sagestockitem.Name;
			item.active = true;
			item.externalId = sagestockitem.Code;

			itemroot.item = item;
			return itemroot;
		}

		#endregion

		#region NOMINAL CODE / GL CODE

		public static GlAccountRoot SageNominalCodeToMTGlAccount(SageNominalCode sagenominalcode)
		{
			GlAccountRoot glaccountroot = new GlAccountRoot();
			GlAccount glaccount = new GlAccount();

			glaccount.id = "";
			glaccount.subsidiaries = null;
			glaccount.departmentRequired = false;
			glaccount.locationRequired = false;
			glaccount.projectRequired = false;
			glaccount.customerRequired = false;
			glaccount.vendorRequired = false;
			glaccount.employeeRequired = false;
			glaccount.itemRequired = false;
			glaccount.classRequired = false;
			glaccount.ledgerType = "ACCOUNT"; // ????
			glaccount.accountNumber = sagenominalcode.Reference;
			glaccount.externalId = sagenominalcode.Reference;
			glaccount.name = sagenominalcode.Name;
			glaccount.active = true;

			glaccountroot.glAccount = glaccount;
			return glaccountroot;
		}

		#endregion

		#region COST CENTRE / LOCATION

		public static LocationRoot SageCostCentreToMTLocation(SageCostCentre sagecostcentre)
		{
			LocationRoot locationroot = new LocationRoot();
			Location location = new Location();

			location.id = "";
			location.subsidiaries = null;
			location.externalId = sagecostcentre.Code;
			location.name = sagecostcentre.Name;
			location.active = true;

			locationroot.location = location;
			return locationroot;
		}

		#endregion

		#region PAYMENT TERMS

		public static TermRoot SagePaymentTermsToMTTerms(Tuple<decimal, int, int> terms)
		{
			TermRoot termroot = new TermRoot();
			Term term = new Term();

			term.id = "";
			term.subsidiaries = null;
			term.discountPercent = terms.Item1;
			term.discountDays = terms.Item2;
			term.dueDays = terms.Item3;
			term.externalId = string.Format("{0}-{1}-{2}", terms.Item1, terms.Item2, terms.Item3);
			term.name = string.Format("Due Days: {0}", terms.Item3);
			term.active = true;

			termroot.term = term;
			return termroot;
		}

		#endregion

		#region PAYMENT METHOD

		public static PaymentMethodRoot SageBankAccountToMTPaymentMethod(Bank bank)
		{
			PaymentMethodRoot methodroot = new PaymentMethodRoot();
			PaymentMethod method = new PaymentMethod();

			method.id = "";
			method.type = "ACH";
			method.externalId = bank.Reference;//??
			method.active = true;

			BankAccount bankaccount = new BankAccount()
			{
				name = bank.Name,
				accountNumber = bank.BankAccount.BankAccountNumber,
				accountBalance = new AccountBalance()
				{
					availableBalance = new AvailableBalance()
					{
						amount = bank.BankAccount.Balance
					}
				}
			};

			method.bankAccount = bankaccount;
		
			methodroot.paymentMethod = method;
			return methodroot;
		}

		#endregion

		#region PURCHASE ORDER

		public static PurchaseOrderRoot SagePurchaseOrderToMTPurchaseOrder(POPOrder poporder)
		{
			PurchaseOrderRoot purchaseorderroot = new PurchaseOrderRoot();
			PurchaseOrder purchaseorder = new PurchaseOrder();

			purchaseorder.externalId = poporder.DocumentNo;

			// GET THE VENDOR ID FROM MINERAL TREE
			Vendor vendor = MTReferenceData.FindVendorByExternalID(poporder.Supplier.SourceReference);
			if (vendor == null) { return null; }
			purchaseorder.vendor = new ObjID() { id = vendor.id };
			// NEED TO ALSO OBTAIN DEPARTMENT, LOCATION, SUBSIDIARY AND TERMS.
			// WHEN MINERAL TREE PROVIDE API ACCESS

			purchaseorder.dueDate = poporder.DocumentDate.ToString("yyyy-MM-dd");
			purchaseorder.poNumber = poporder.DocumentNo;
			purchaseorder.memo = "";
			purchaseorder.state = "PendingBilling";
			purchaseorder.poType = "Standard"; // ??
			//purchaseorder.expenses = null; // ??

			// PURCHASE ORDER ITEMS
			List<PurchaseOrderItem> items = new List<PurchaseOrderItem>();
			int linenumber = 1;
			foreach(Sage.Accounting.POP.POPOrderReturnLine line in poporder.Lines)
			{
				PurchaseOrderItem item = new PurchaseOrderItem();

				//item.companyItem = i think this is item object
				//item.classification = 
				//item.department = 
				//item.location = 
				//item.glAccount = 
				item.name = "Test"; // line.LineDescription;
				item.quantity = new Quantity() { value = line.LineQuantity, precision = 2 };
				item.quantityReceived = new Quantity() { value = 0, precision = 2 }; // no value in sage
				item.billedQuantity = new Quantity() { value = line.LineQuantity, precision = 2 };
				item.cost = new Cost() { amount = line.UnitBuyingPrice, precision = 2 };
				item.amountDue = new Amount() { amount = line.LineTotalValue };
				item.lineNumber = linenumber; // no value in sage
				item.closed = false; // no value in sage
				item.description = line.ItemDescription;
				item.poItemStatus = "New";

				items.Add(item);
				linenumber++;
			}
			purchaseorder.items = items;


			purchaseorderroot.purchaseOrder = purchaseorder;
			return purchaseorderroot;
		}

		#endregion

		#region INVOICE/BILL

		public static BillRoot SageInvoiceToMTBill(PostedPurchaseAccountEntry invoice)
		{
			BillRoot billroot = new BillRoot();
			Bill bill = new Bill();

			bill.id = "";
			bill.externalId = "";// invoice.InstrumentNo;
			// term, classification, etc
			
			bill.dueDate = invoice.DueDate.ToString("yyyy-MM-dd");
			bill.transactionDate = invoice.InstrumentDate.ToString("yyyy-MM-dd"); //??
			bill.invoiceNumber = invoice.InstrumentNo;
			bill.amount = new Amount() { amount = invoice.CoreDocumentNetValue };	// ???
			bill.balance = new Amount() { amount = 2 }; // ???
			bill.totalTaxAmount = new Amount() { amount = invoice.CoreDocumentTaxValue }; // ??
			if (invoice.MemoNotes.Count > 0)
				bill.memo = invoice.MemoNotes[0].Note; // JUST USE THE FIRST MEMO
			else
				bill.memo = "";
			bill.poNumber = ""; // ??
			bill.state = "Open"; // ??

			// GET THE VENDOR ID FROM MINERAL TREE
			Vendor vendor = MTReferenceData.FindVendorByExternalID(invoice.Supplier.SourceReference);
			if (vendor == null) { return null; }
			bill.vendor = new ObjID() { id = vendor.id };

			bill.expenses = null;
			bill.items = null;

			billroot.bill = bill;
			return billroot;
		}

		#endregion

		#region CREDIT NOTES

		public static CreditRoot SageCreditNoteToMTCredit(PostedPurchaseAccountEntry creditnote)
		{
			CreditRoot creditroot = new CreditRoot();
			Credit credit = new Credit();

			credit.id = "";
			credit.creditNumber = creditnote.InstrumentNo;
			credit.transactionDate = creditnote.InstrumentDate.ToString("yyyy-MM-dd"); ;

			// GET THE VENDOR ID FROM MINERAL TREE
			Vendor vendor = MTReferenceData.FindVendorByExternalID(creditnote.Supplier.SourceReference);
			if (vendor == null) { return null; }
			credit.vendor = new ObjID() { id = vendor.id };

			credit.amount = new Amount() { amount = creditnote.CoreDocumentGrossValue }; // ???
			credit.externalId = creditnote.InstrumentNo;
			credit.status = "Open";
			if (creditnote.MemoNotes.Count > 0)
				credit.memo = creditnote.MemoNotes[0].Note; // JUST USE THE FIRST MEMO
			else
				credit.memo = "";

			creditroot.credit = credit;
			return creditroot;
		}

		#endregion

	}
}