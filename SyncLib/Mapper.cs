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
using Utils;

namespace SyncLib
{
	/// <summary>
	/// Performs object mapping between sage and mineral tree
	/// </summary>
	public static class Mapper
	{
		#region VENDOR

		public static VendorRoot SageSupplierToMTVendor(SageSupplier supplier, string prefix)
		{
			VendorRoot vendorroot = new VendorRoot();
			Vendor vendor = new Vendor();


			vendor.id = "";
			vendor.externalId = ExternalIDFormatter.AppendPrefix(supplier.PrimaryKey.DbValue.ToString(), prefix);
			vendor.form1099Enabled = false;
			vendor.name = supplier.Name;
			vendor.active = true; // NO MATCHING FIELD IN SAGE - CLOSEST IS ON HOLD???

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

			vendor.customerAccount = supplier.SupplierBanks.PrimaryBank.BankAccountReference;
			//vendor.primarySubsidiary = null;// new PrimarySubsidiary() {id = "1234" };
			vendor.taxId = "";
			vendor.vatNumber = supplier.TaxRegistrationCode;
			
			/*
			VENDOR COMPANY DEFAULT IS NOT CURRENTLY SUPPORTED BUT MT - EVEN THOUGH ITS IN THE DOCUMENTATION!!!!

			vendor.vendorCompanyDefault = new VendorCompanyDefault()
			{
				defaultExpenseAccountId = null
			};

			GlAccount glaccount = MTReferenceData.FindGlAccountByAccountNumber(supplier.DefaultNominalAccountNumber);
			if (glaccount != null)
			{
				vendor.vendorCompanyDefault.defaultExpenseAccountId = glaccount.id;
			}
			*/

			vendorroot.vendor = vendor;
			return vendorroot;
		}

		#endregion

		#region DEPARTMENT

		public static DepartmentRoot SageDepartmentToMTDepartment(SageDepartment sagedepartment, string prefix)
		{
			DepartmentRoot departmentroot = new DepartmentRoot();
			Department department = new Department();

			department.id = "";
			department.externalId = ExternalIDFormatter.AppendPrefix(sagedepartment.PrimaryKey.DbValue.ToString(), prefix);
			department.name = sagedepartment.Name;
			department.active = true; // NO MATCHING FIELD IN SAGE

			departmentroot.department = department;
			return departmentroot;
		}

		#endregion

		#region STOCKITEM / ITEM

		public static ItemRoot SageStockItemToMTItem(SageStockItem sagestockitem, string prefix)
		{
			ItemRoot itemroot = new ItemRoot();
			Item item = new Item();

			item.id = "";
			item.active = sagestockitem.StockItemStatus == Sage.Accounting.Stock.StockItemStatusEnum.EnumStockItemStatusTypeActive ? true : false;
			
			item.externalId = ExternalIDFormatter.AppendPrefix(sagestockitem.PrimaryKey.DbValue.ToString(), prefix);
			item.name = sagestockitem.Name;
			item.type = "INVENTORY"; // ???
 
			Cost cost = new Cost()
			{
				amount = PriceConverter.FromDecimal(sagestockitem.PriceBands[0].StockItemPrices[0].Price, 2),
				precision = 2
			};
			item.cost = cost;

			Residual residual = new Residual()
			{
				amount = 0 // ???
			};
			item.residual = residual;

			itemroot.item = item;
			return itemroot;
		}

		#endregion

		#region NOMINAL CODE / GL CODE

		public static GlAccountRoot SageNominalCodeToMTGlAccount(SageNominalCode sagenominalcode, string prefix)
		{
			GlAccountRoot glaccountroot = new GlAccountRoot();
			GlAccount glaccount = new GlAccount();

			glaccount.id = "";
			glaccount.externalId = ExternalIDFormatter.AppendPrefix(sagenominalcode.PrimaryKey.DbValue.ToString(), prefix);
			glaccount.subsidiaries = null;
			glaccount.departmentRequired = false;
			glaccount.locationRequired = false;
			glaccount.projectRequired = false;
			glaccount.customerRequired = false;
			glaccount.vendorRequired = false;
			glaccount.employeeRequired = false;
			glaccount.itemRequired = false;
			glaccount.classRequired = false;
			glaccount.ledgerType = "EXPENSE_ACCOUNT";
			glaccount.accountNumber = sagenominalcode.Reference;
			glaccount.name = sagenominalcode.Name;
			glaccount.active = true; // NO MATCHING FIELD IN SAGE

			glaccountroot.glAccount = glaccount;
			return glaccountroot;
		}

		#endregion

		#region COST CENTRE / LOCATION

		public static LocationRoot SageCostCentreToMTLocation(SageCostCentre sagecostcentre, string prefix)
		{
			LocationRoot locationroot = new LocationRoot();
			Location location = new Location();

			location.id = "";
			location.externalId = ExternalIDFormatter.AppendPrefix(sagecostcentre.PrimaryKey.DbValue.ToString(), prefix);
			location.subsidiaries = null;
			location.name = sagecostcentre.Name;
			location.active = true; // NO MATCHING FIELD IN SAGE

			locationroot.location = location;
			return locationroot;
		}

		#endregion

		#region PAYMENT TERMS

		public static TermRoot SagePaymentTermsToMTTerms(Tuple<decimal, int, int> terms, string prefix)
		{
			TermRoot termroot = new TermRoot();
			Term term = new Term();

			term.id = "";
			term.subsidiaries = null;
			term.discountPercent = terms.Item1;
			term.discountDays = terms.Item2;
			term.dueDays = terms.Item3;
			term.externalId = ExternalIDFormatter.AppendPrefix(string.Format("{0}-{1}-{2}", terms.Item1, terms.Item2, terms.Item3), prefix);
			term.name = string.Format("Due Days: {0}", terms.Item3);
			term.active = true;

			termroot.term = term;
			return termroot;
		}

		#endregion

		#region BANK ACCOUNT / PAYMENT METHOD

		public static PaymentMethodRoot SageBankAccountToMTPaymentMethod(Bank bank, string prefix)
		{
			PaymentMethodRoot methodroot = new PaymentMethodRoot();
			PaymentMethod method = new PaymentMethod();

			method.id = "";
			method.type = "ACH";
			method.externalId = ExternalIDFormatter.AppendPrefix(bank.PrimaryKey.DbValue.ToString(), prefix);
			method.active = true; // NO MATCHING FIELD IN SAGEs

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

		public static PurchaseOrderRoot SagePurchaseOrderToMTPurchaseOrder(POPOrder poporder, string prefix)
		{
			PurchaseOrderRoot purchaseorderroot = new PurchaseOrderRoot();
			PurchaseOrder purchaseorder = new PurchaseOrder();

			purchaseorder.id = "";
			purchaseorder.externalId = ExternalIDFormatter.AppendPrefix(poporder.PrimaryKey.DbValue.ToString(), prefix);

			// GET THE VENDOR ID FROM MINERAL TREE
			Vendor vendor = MTReferenceData.FindVendorByExternalID(ExternalIDFormatter.AppendPrefix(poporder.Supplier.PrimaryKey.DbValue.ToString(), prefix));
			if (vendor == null) { return null; }
			purchaseorder.vendor = new ObjID() { id = vendor.id };

			purchaseorder.dueDate = poporder.DocumentDate.ToString("yyyy-MM-dd");
			purchaseorder.poNumber = poporder.DocumentNo;
			purchaseorder.memo = "";
			purchaseorder.state = "PendingBilling";
			purchaseorder.poType = "Standard";

			// PURCHASE ORDER ITEMS
			List<PurchaseOrderItem> items = new List<PurchaseOrderItem>();
			int linenumber = 1;
			foreach (Sage.Accounting.POP.POPOrderReturnLine line in poporder.Lines)
			{
				PurchaseOrderItem item = new PurchaseOrderItem();

				// 
				// FIGURE OUT THE MT ID OF THE LINE ITEM
				SageStockItem ssi = SageApi.GetStockItemByCode(line.ItemCode);
				if (ssi != null)
				{
					Item mtitem = MTReferenceData.FindItemByExternalID(ExternalIDFormatter.AppendPrefix(ssi.PrimaryKey.DbValue.ToString(), prefix));
					if (mtitem != null)
					{
						item.companyItem = new ObjID() { id = mtitem.id };
					}
				}
				//

				item.name = line.LineDescription;
				item.quantity = new Quantity() { value = PriceConverter.FromDecimal(line.LineQuantity, 2), precision = 2 };
				item.quantityReceived = new Quantity() { value = 1, precision = 0 }; // no value in sage
				item.billedQuantity = new Quantity() { value = PriceConverter.FromDecimal(line.LineQuantity, 2), precision = 2 };

				item.cost = new Cost()
				{
					amount = PriceConverter.FromDecimal(line.UnitBuyingPrice, 2),
					precision = 0
				};

				item.amountDue = new Amount()
				{
					amount = PriceConverter.FromDecimal(line.LineTotalValue, 2),
					precision = 0
				};

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

		public static BillRoot SageInvoiceToMTBill(PostedPurchaseAccountEntry invoice, string prefix)
		{
			BillRoot billroot = new BillRoot();
			Bill bill = new Bill();

			bill.id = "";
			bill.externalId = ExternalIDFormatter.AppendPrefix(invoice.PrimaryKey.DbValue.ToString(), prefix);
			// term, classification, etc
			
			bill.dueDate = invoice.DueDate.ToString("yyyy-MM-dd");
			bill.transactionDate = invoice.InstrumentDate.ToString("yyyy-MM-dd"); //??
			bill.invoiceNumber = invoice.InstrumentNo;
			bill.amount = new Amount()
			{
				amount = PriceConverter.FromDecimal(invoice.CoreDocumentNetValue, 2),
				precision = 2
			};
			bill.balance = new Amount() { amount = 0 }; // ???
			bill.totalTaxAmount = new Amount()
			{
				amount = PriceConverter.FromDecimal(invoice.CoreDocumentTaxValue, 2),
				precision = 2
			};
			if (invoice.MemoNotes.Count > 0)
				bill.memo = invoice.MemoNotes[0].Note; // JUST USE THE FIRST MEMO
			else
				bill.memo = "";
			bill.poNumber = ""; // ??
			bill.state = EnumMapper.SageDocumentStatusEnumToMTState(invoice.DocumentStatus);

			// GET THE VENDOR ID FROM MINERAL TREE
			Vendor vendor = MTReferenceData.FindVendorByExternalID(ExternalIDFormatter.AppendPrefix(invoice.Supplier.PrimaryKey.DbValue.ToString(), prefix));
			if (vendor == null) { return null; }
			bill.vendor = new ObjID() { id = vendor.id };

			bill.expenses = null;
			bill.items = null;

			billroot.bill = bill;
			return billroot;
		}

		#endregion

		#region CREDIT NOTES

		public static CreditRoot SageCreditNoteToMTCredit(PostedPurchaseAccountEntry creditnote, string prefix)
		{
			CreditRoot creditroot = new CreditRoot();
			Credit credit = new Credit();

			credit.id = "";
			credit.externalId = ExternalIDFormatter.AppendPrefix(creditnote.PrimaryKey.DbValue.ToString(), prefix);
			credit.creditNumber = creditnote.InstrumentNo;
			credit.transactionDate = creditnote.InstrumentDate.ToString("yyyy-MM-dd");

			// GET THE VENDOR ID FROM MINERAL TREE
			Vendor vendor = MTReferenceData.FindVendorByExternalID(ExternalIDFormatter.AppendPrefix(creditnote.Supplier.PrimaryKey.DbValue.ToString(), prefix));
			if (vendor == null) { return null; }
			credit.vendor = new ObjID() { id = vendor.id };

			credit.amount = new Amount()
			{
				amount = PriceConverter.FromDecimal(creditnote.CoreDocumentGrossValue, 2),
				precision = 2
			};
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