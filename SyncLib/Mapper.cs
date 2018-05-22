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
using TaxCode = Sage.Accounting.TaxModule.TaxCode;
using TaxAnalysisItem = Sage.Accounting.TradeLedger.TaxAnalysisItem;
using NominalAnalysisItem = Sage.Accounting.TradeLedger.NominalAnalysisItem;
using Utils;

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
			vendor.externalId = supplier.PrimaryKey.DbValue.ToString();
			vendor.form1099Enabled = false;
			vendor.name = supplier.Name;
			vendor.active = !supplier.OnHold;

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

			Sage.Accounting.SystemManager.SYSTraderContactRole role = Sage.Accounting.SystemManager.SYSTraderContactRoleFactory.Factory.FetchSendRemittanceToRole();
			Sage.Accounting.TradeLedger.TraderContact contact = supplier.GetPreferredContact(role);

			vendor.remittanceEmails = null;
			vendor.remittanceEnabled = false;

			if (contact.Emails[0].Value.Length > 0)
			{
				vendor.remittanceEmails = new List<string>() { contact.Emails[0].Value };
				vendor.remittanceEnabled = true;
			}

			vendor.memo = "";
			if (supplier.Memos.Count > 0)
				vendor.memo = supplier.Memos[0].Note; // JUST USE THE FIRST MEMO

			vendor.customerAccount = supplier.SupplierBanks.PrimaryBank.BankAccountReference;
			vendor.taxId = "";
			vendor.vatNumber = supplier.TaxRegistrationCode;

			// VENDOR COMPANY DEFAULT IS NOT CURRENTLY SUPPORTED BY MT - EVEN THOUGH ITS IN THE DOCUMENTATION!!!!
			//vendor.vendorCompanyDefault = new VendorCompanyDefault();

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
			department.externalId = sagedepartment.PrimaryKey.DbValue.ToString();
			department.name = sagedepartment.Name;
			department.active = true; // NO MATCHING FIELD IN SAGE

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
			item.active = sagestockitem.StockItemStatus == Sage.Accounting.Stock.StockItemStatusEnum.EnumStockItemStatusTypeActive ? true : false;
			
			item.externalId = sagestockitem.PrimaryKey.DbValue.ToString();
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

		public static GlAccountRoot SageNominalCodeToMTGlAccount(SageNominalCode sagenominalcode)
		{
			GlAccountRoot glaccountroot = new GlAccountRoot();
			GlAccount glaccount = new GlAccount();

			glaccount.id = "";
			glaccount.externalId = sagenominalcode.PrimaryKey.DbValue.ToString();
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

		public static LocationRoot SageCostCentreToMTLocation(SageCostCentre sagecostcentre)
		{
			LocationRoot locationroot = new LocationRoot();
			Location location = new Location();

			location.id = "";
			location.externalId = sagecostcentre.PrimaryKey.DbValue.ToString();
			location.subsidiaries = null;
			location.name = sagecostcentre.Name;
			location.active = true; // NO MATCHING FIELD IN SAGE

			locationroot.location = location;
			return locationroot;
		}

		#endregion

		#region PAYMENT TERMS

		// NEEDS TO BE CHECKED AGAINST API WHEN DOCS AVAILABLE
		public static TermRoot SagePaymentTermsToMTTerms(SageSupplier supplier)
		{
			TermRoot termroot = new TermRoot();
			Term term = new Term();

			term.id = "";
			term.subsidiaries = null;
			term.discountPercent = supplier.EarlySettlementDiscountPercent;
			term.discountDays = supplier.EarlySettlementDiscountDays;
			term.dueDays = supplier.PaymentTermsDays;
			term.externalId = supplier.PrimaryKey.DbValue.ToString();
			term.name = string.Format("Due Days: {0}, Discount Days: {0}, Discount Percent: {0}", supplier.PaymentTermsDays, supplier.EarlySettlementDiscountDays, supplier.EarlySettlementDiscountPercent);
			term.active = true; // ??

			termroot.term = term;
			return termroot;
		}
		/*
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
		*/

		#endregion

		#region BANK ACCOUNT / PAYMENT METHOD

		public static PaymentMethodRoot SageBankAccountToMTPaymentMethod(Bank bank)
		{
			PaymentMethodRoot methodroot = new PaymentMethodRoot();
			PaymentMethod method = new PaymentMethod();

			method.id = "";
			method.type = "ACH";
			method.externalId = bank.PrimaryKey.DbValue.ToString();
			method.active = true; // NO MATCHING FIELD IN SAGE

			BankAccount bankaccount = new BankAccount()
			{
				name = bank.Name,
				accountNumber = bank.BankAccount.BankAccountNumber,
				accountBalance = new AccountBalance()
				{
					availableBalance = new AvailableBalance()
					{
						amount = PriceConverter.FromDecimal(bank.BankAccount.BaseCurrencyBalance, 2)
					}
				}
			};

			method.bankAccount = bankaccount;
		
			methodroot.paymentMethod = method;
			return methodroot;
		}

		#endregion

		#region PURCHASE ORDER

		public static PurchaseOrderRoot SagePurchaseOrderToMTPurchaseOrder(string companyid, POPOrder poporder, string sessiontoken)
		{
			PurchaseOrderRoot purchaseorderroot = new PurchaseOrderRoot();
			PurchaseOrder purchaseorder = new PurchaseOrder();

			purchaseorder.id = "";
			purchaseorder.externalId = poporder.PrimaryKey.DbValue.ToString();

			// GET THE VENDOR ID FROM MINERAL TREE
			Vendor vendor = MTApi.GetVendorByExternalID(companyid, sessiontoken, poporder.Supplier.PrimaryKey.DbValue.ToString());
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
				SageStockItem ssi = Sage200Api.GetStockItemByCode(line.ItemCode);
				if (ssi != null)
				{
					Item mtitem = MTApi.GetItemByExternalID(companyid, sessiontoken, ssi.PrimaryKey.DbValue.ToString());
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
					precision = 2
				};

				item.amountDue = new Amount()
				{
					amount = PriceConverter.FromDecimal(line.LineTotalValue, 2),
					precision = 2
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

		public static BillRoot SageInvoiceToMTBill(string companyid, PostedPurchaseAccountEntry invoice, string sessiontoken)
		{
			BillRoot billroot = new BillRoot();
			Bill bill = new Bill();

			bill.id = "";
			bill.externalId = invoice.PrimaryKey.DbValue.ToString();
			
			bill.dueDate = invoice.DueDate.ToString("yyyy-MM-dd");
			bill.transactionDate = invoice.InstrumentDate.ToString("yyyy-MM-dd"); //??
			bill.invoiceNumber = invoice.InstrumentNo; // THIS IS REFERENCE IN SAGE UI (INVOICE SCREEN)

			bill.amount = new Amount()
			{
				amount = PriceConverter.FromDecimal(invoice.CoreDocumentGrossValue, 2),
				precision = 2
			};

			bill.balance = new Amount() { amount = 0 }; // ???

			bill.totalTaxAmount = new Amount()
			{
				amount = PriceConverter.FromDecimal(invoice.CoreDocumentTaxValue, 2),
				precision = 2
			};

			// NOT POSSIBLE TO ADD LINE ITEMS BECAUSE SAGE SPLITS TRANSACTIONS INTO 2, VAT AND GLCODE/GOODSAMOUNT AND THEY ARE NOT RELATED
			// NOMINAL ANALYSIS
			/*
			bill.items = new List<CompanyItem>();
			
			int count = invoice.TradingTransactionDrillDown.NominalEntries.Count();
			foreach (Sage.Accounting.NominalLedger.NominalAccountEntryView view in invoice.TradingTransactionDrillDown.NominalEntries)
			{
				CompanyItem ci = new CompanyItem();
				ci.glAccount = MTReferenceData.FindGlAccountByAccountNumber(view.AccountNumber);
				ci.netAmount = new Amount() { amount = PriceConverter.FromDecimal(view.GoodsValueInBaseCurrency, 2), precision = 2 };
				ci.taxAmount = new Amount() { amount = 0, precision = 2 };

				bill.items.Add(ci);

				// CREATE THE NOMINAL ANALYSIS
				
				//NominalCode nominalcode = SageApi.GetNominalCodeByPrimaryKey(lineitem.GLAccountID);
				//nominal.NominalSpecification = nominalcode.NominalSpecification;
				//nominal.Narrative = lineitem.Description;
				//nominal.Amount = lineitem.NetAmount;

				// CREATE THE VAT ANALYSIS
				//TaxCode taxcode = SageApi.GetVatRateByPrimaryKey(lineitem.ClassificationID);
				//tax.TaxCode = taxcode;
				//tax.Goods = lineitem.NetAmount;
				//tax.TaxAmount = lineitem.TaxAmount;	
			}
			
			// TAX/VAT ANALYSIS
			/*
			foreach (Sage.Accounting.TaxModule.Views.TaxAccountEntryView view in invoice.TradingTransactionDrillDown.TaxEntries)
			{
				CompanyItem ci = new CompanyItem();
				//view.GoodsAmount;
				//view.TaxAmount;
				//view.TaxRate;
				//view.
			}
			//
			*/
			if (invoice.MemoNotes.Count > 0)
				bill.memo = invoice.MemoNotes[0].Note; // JUST USE THE FIRST MEMO
			else
				bill.memo = "";
			bill.poNumber = ""; // ??
			bill.state = EnumMapper.SageDocumentStatusEnumToMTState(invoice.DocumentStatus);

			// GET THE VENDOR ID FROM MINERAL TREE
			Vendor vendor = MTApi.GetVendorByExternalID(companyid, sessiontoken, invoice.Supplier.PrimaryKey.DbValue.ToString());
			if (vendor == null) { return null; }
			bill.vendor = new ObjID() { id = vendor.id };

			bill.expenses = null;
			bill.items = null;

			billroot.bill = bill;
			return billroot;
		}

		#endregion

		#region CREDIT NOTES

		public static CreditRoot SageCreditNoteToMTCredit(string companyid, PostedPurchaseAccountEntry creditnote, string sessiontoken)
		{
			CreditRoot creditroot = new CreditRoot();
			Credit credit = new Credit();

			credit.id = "";
			credit.externalId = creditnote.PrimaryKey.DbValue.ToString();
			credit.creditNumber = creditnote.SecondReferenceNo; // THIS WILL BE THE INVOICE NUMBER
			credit.transactionDate = creditnote.InstrumentDate.ToString("yyyy-MM-dd");

			// GET THE VENDOR ID FROM MINERAL TREE
			Vendor vendor = MTApi.GetVendorByExternalID(companyid, sessiontoken, creditnote.Supplier.PrimaryKey.DbValue.ToString());
			if (vendor == null) { return null; }
			credit.vendor = new ObjID() { id = vendor.id };

			credit.amount = new Amount()
			{
				amount = PriceConverter.FromDecimal(Math.Abs(creditnote.DocumentGrossValue), 2),
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

		public static BillCreditRoot SageCreditNoteToMTBillCredit(string companyid, PostedPurchaseAccountEntry creditnote, string sessiontoken)
		{
			BillCreditRoot billcreditroot = new BillCreditRoot();
			BillCredit billcredit = new BillCredit();

			billcredit.transactionDate = creditnote.InstrumentDate.ToString("yyyy-MM-dd");

			billcredit.amountApplied = new Amount()
			{
				amount = PriceConverter.FromDecimal(Math.Abs(creditnote.DocumentGrossValue), 2),
				precision = 2
			};

			billcreditroot.billCredit = billcredit;
			return billcreditroot;
		}

		#endregion

		#region CLASSIFICATION / VAT RATE

		public static ClassificationRoot SageTaxCodeToMTClassification(TaxCode taxcode)
		{
			ClassificationRoot classificationroot = new ClassificationRoot();
			Classification classification = new Classification();

			classification.id = "";
			classification.externalId = taxcode.PrimaryKey.DbValue.ToString();

			classification.name = string.Format("{0}-{1} ({2})", taxcode.Code, taxcode.Name, taxcode.TaxRate);

			classification.active = true;

			classificationroot.classification = classification;
			return classificationroot;
		}

		#endregion
	}
}