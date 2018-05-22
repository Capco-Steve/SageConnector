using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Sage.Common.Data;
using SageLib.Objects;
using Utils;
using Supplier = Sage.Accounting.PurchaseLedger.Supplier;
using Bank = Sage.Accounting.CashBook.Bank;
using Department = Sage.Accounting.SystemManager.Department;
using Application = Sage.Accounting.Application;
using StockItem = Sage.Accounting.Stock.StockItem;
using NominalCode = Sage.Accounting.NominalLedger.NominalCode;
using CostCentre = Sage.Accounting.SystemManager.CostCentre;
using POPOrder = Sage.Accounting.POP.POPOrder;
using NominalAnalysisItem = Sage.Accounting.TradeLedger.NominalAnalysisItem;
using TaxAnalysisItem = Sage.Accounting.TradeLedger.TaxAnalysisItem;
using TaxCode = Sage.Accounting.TaxModule.TaxCode;

namespace SageLib
{
    public static class Sage200Api
    {
        private static Application application = null;

		#region CONNECT

		public static bool Connect(string companyname)
        {
            bool result = true;
            try
            {
                application = new Application();
				application.Connect();
				foreach(Sage.Accounting.Company company in application.Companies)
				{
					if(company.Name == companyname)
					{
						application.ActiveCompany = company;
					}
				}

				result = application.ActiveCompany == null ? false : true;
            }
            catch(Exception ex)
            {
                // TODO - LOGGING
                result = false;
            }
            return result;
        }

		#endregion

		#region SUPPLIERS

		public static List<Supplier> GetSuppliers()
        {
            List<Supplier> list = new List<Supplier>();
            if (application != null)
            {
                Sage.Accounting.PurchaseLedger.Suppliers suppliers = Sage.Accounting.PurchaseLedger.SuppliersFactory.Factory.CreateNew();
				list = suppliers.GetList().Cast<Supplier>().ToList();
			}
			return list;
        }

		public static List<Supplier> GetSuppliersModifiedAfter(DateTime dt)
		{
			List<Supplier> list = new List<Supplier>();
			if (application != null)
			{
				Sage.Accounting.PurchaseLedger.Suppliers suppliers = Sage.Accounting.PurchaseLedger.SuppliersFactory.Factory.CreateNew();
				Sage.ObjectStore.Query query = new Sage.ObjectStore.Query();
				Sage.ObjectStore.Filter filter = new Sage.ObjectStore.Filter(Sage.Accounting.PurchaseLedger.Supplier.FIELD_DATETIMEUPDATED, FilterOperator.GreaterThanOrEqual, dt);
				query.Filters.Add(filter);

				suppliers.Find(query);

				list = suppliers.GetList().Cast<Supplier>().ToList();
			}
			return list;
		}

		public static Supplier GetSupplierByPrimaryKey(string key)
		{
			Supplier supplier = null;
			if (application != null)
			{
				Sage.Common.Data.DbKey dbkey = new Sage.Common.Data.DbKey(Convert.ToInt32(key));
				supplier = Sage.Accounting.PurchaseLedger.SupplierFactory.Factory.Fetch(dbkey);
				
			}
			return supplier;
		}

		#endregion

		#region BANKS

		public static List<Bank> GetBanks()
		{
			List<Bank> list = null;
			if (application != null)
			{
				Sage.Accounting.CashBook.Banks banks = Sage.Accounting.CashBook.BanksFactory.Factory.CreateNew();
				list = banks.GetList().Cast<Bank>().ToList();
			}
			return list;
		}

		public static List<Bank> GetBanksModifiedAfter(DateTime dt)
		{
			List<Bank> list = null;
			if (application != null)
			{
				Sage.Accounting.CashBook.Banks banks = Sage.Accounting.CashBook.BanksFactory.Factory.CreateNew();
				Sage.ObjectStore.Query query = new Sage.ObjectStore.Query();
				Sage.ObjectStore.Filter filter = new Sage.ObjectStore.Filter(Sage.Accounting.CashBook.Bank.FIELD_DATETIMEUPDATED, FilterOperator.GreaterThanOrEqual, dt);
				query.Filters.Add(filter);

				banks.Find(query);

				list = banks.GetList().Cast<Bank>().ToList();
			}
			return list;
		}

		public static Bank GetBankByPrimaryKey(string key)
		{
			Bank bank = null;
			if (application != null)
			{
				Sage.Common.Data.DbKey dbkey = new Sage.Common.Data.DbKey(Convert.ToInt32(key));
				bank = Sage.Accounting.CashBook.BankFactory.Factory.Fetch(dbkey);

			}
			return bank;
		}

		#endregion

		#region DEPARTMENTS

		public static List<Department> GetDepartments()
		{
			List<Department> list = null;
			if (application != null)
			{
				Sage.Accounting.SystemManager.Departments departments = Sage.Accounting.SystemManager.DepartmentsFactory.Factory.CreateNew();
				list = departments.GetList().Cast<Department>().ToList();
			}
			return list;
		}

		public static List<Department> GetDepartmentsModifiedAfter(DateTime dt)
		{
			List<Department> list = new List<Department>();
			if (application != null)
			{
				Sage.Accounting.SystemManager.Departments departments = Sage.Accounting.SystemManager.DepartmentsFactory.Factory.CreateNew();
				List<Department> temp = departments.GetList().Cast<Department>().ToList();

				foreach(Department department in temp)
				{
					if(department.DateTimeUpdated > dt)
					{
						list.Add(department);
					}
				}
			}
			return list;
		}

		public static Department GetDepartmentByPrimaryKey(string key)
		{
			Department department = null;
			if (application != null)
			{
				Sage.Common.Data.DbKey dbkey = new Sage.Common.Data.DbKey(Convert.ToInt32(key));
				department = Sage.Accounting.SystemManager.DepartmentFactory.Factory.Fetch(dbkey);
			}
			return department;
		}

		#endregion

		#region STOCK ITEMS

		public static List<StockItem> GetStockItems()
		{
			List<StockItem> list = null;
			if (application != null)
			{
				Sage.Accounting.Stock.StockItems stockitems = new Sage.Accounting.Stock.StockItems();
				list = stockitems.GetList().Cast<StockItem>().ToList();
			}
			return list;
		}

		public static List<StockItem> GetStockItemsModifiedAfter(DateTime dt)
		{
			List<StockItem> list = null;
			if (application != null)
			{
				Sage.Accounting.Stock.StockItems stockitems = new Sage.Accounting.Stock.StockItems();
				Sage.ObjectStore.Query query = new Sage.ObjectStore.Query();
				Sage.ObjectStore.Filter filter = new Sage.ObjectStore.Filter(Sage.Accounting.Stock.StockItem.FIELD_DATETIMEUPDATED, FilterOperator.GreaterThanOrEqual, dt);
				query.Filters.Add(filter);

				stockitems.Find(query);

				list = stockitems.GetList().Cast<StockItem>().ToList();
			}
			return list;
		}

		public static StockItem GetStockItemByCode(string code)
		{
			if (application != null)
			{
				Sage.Accounting.Stock.StockItems stockitems = new Sage.Accounting.Stock.StockItems();
				List<StockItem> list = stockitems.GetList().Cast<StockItem>().ToList();

				return list.Find(stock => stock.Code == code);
			}
			return null;
		}

		#endregion

		#region NOMINAL CODES / GL CODES

		public static List<NominalCode> GetNominalCodes()
		{
			List<NominalCode> list = null;
			if (application != null)
			{
				Sage.Accounting.NominalLedger.NominalCodes codes = Sage.Accounting.NominalLedger.NominalCodesFactory.Factory.CreateNew();
				list = codes.GetList().Cast<NominalCode>().ToList();
			}

			return list;
		}

		public static List<NominalCode> GetNominalCodesModifiedAfter(DateTime dt)
		{
			List<NominalCode> list = new List<NominalCode>();
			if (application != null)
			{
				Sage.Accounting.NominalLedger.NominalCodes codes = Sage.Accounting.NominalLedger.NominalCodesFactory.Factory.CreateNew();
				List<NominalCode> temp = codes.GetList().Cast<NominalCode>().ToList();

				foreach(NominalCode code in temp)
				{
					if(code.DateTimeUpdated > dt)
					{
						list.Add(code);
					}
				}

			}

			return list;
		}

		public static NominalCode GetNominalCodeByPrimaryKey(string key)
		{
			NominalCode nominalcode = null;
			if (application != null)
			{
				Sage.Common.Data.DbKey dbkey = new Sage.Common.Data.DbKey(Convert.ToInt32(key));
				nominalcode = Sage.Accounting.NominalLedger.NominalCodeFactory.Factory.Fetch(dbkey);
			}

			return nominalcode;
		}

		public static NominalCode GetNominalCodeByAccountNumber(string accountnumber)
		{
			NominalCode nominalcode = null;
			if (application != null)
			{
				Sage.Accounting.NominalLedger.NominalCodes codes = Sage.Accounting.NominalLedger.NominalCodesFactory.Factory.FetchWithAccountNumber(accountnumber);

				nominalcode = codes.First;
			}
			return nominalcode;
		}

		#endregion

		#region COST CENTRES - LOCATIONS

		public static List<CostCentre> GetCostCentres()
		{
			List<CostCentre> list = null;
			if (application != null)
			{
				Sage.Accounting.SystemManager.CostCentres costcentres = Sage.Accounting.SystemManager.CostCentresFactory.Factory.CreateNew();
				list = costcentres.GetList().Cast<CostCentre>().ToList();
			}

			return list;
		}

		public static List<CostCentre> GetCostCentresModifiedAfter(DateTime dt)
		{
			List<CostCentre> list = new List<CostCentre>();
			if (application != null)
			{
				Sage.Accounting.SystemManager.CostCentres costcentres = Sage.Accounting.SystemManager.CostCentresFactory.Factory.CreateNew();
				List<CostCentre> temp = costcentres.GetList().Cast<CostCentre>().ToList();

				foreach(CostCentre costcentre in temp)
				{
					if(costcentre.DateTimeUpdated > dt)
					{
						list.Add(costcentre);
					}
				}
			}

			return list;
		}

		#endregion

		#region PURCHASE ORDERS

		public static List<POPOrder> GetLivePurchaseOrders()
		{
			List<POPOrder> list = new List<POPOrder>();
			if (application != null)
			{
				Sage.Accounting.POP.POPOrders poporders = new Sage.Accounting.POP.POPOrders();
				List<POPOrder> allorders = poporders.GetList().Cast<POPOrder>().ToList();

				foreach(POPOrder order in allorders)
				{
					if(order.DocumentStatus == Sage.Accounting.OrderProcessing.DocumentStatusEnum.EnumDocumentStatusLive)
					{
						list.Add(order);
					}
				}
			}

			return list;
		}

		public static POPOrder GetPurchaseOrderByPrimaryKey(string key)
		{
			POPOrder poporder = null;
			if (application != null)
			{
				Sage.Common.Data.DbKey dbkey = new Sage.Common.Data.DbKey(Convert.ToInt32(key));
				poporder = Sage.Accounting.POP.POPOrder.Fetch(key);
			}

			return poporder;
		}

		#endregion

		#region INVOICES

		public static List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry> GetHistoricalInvoices(DateTime from)
		{
			List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry> results = new List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry>();
			if (application != null)
			{
				// PURCHASE INVOICES
				Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntries invoices = Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntriesFactory.Factory.CreateNew();
				Sage.ObjectStore.Query query = new Sage.ObjectStore.Query();
				Sage.ObjectStore.Filter filter = new Sage.ObjectStore.Filter(Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry.FIELD_ENTRYTYPE, Sage.Accounting.TradingAccountEntryTypeEnum.TradingAccountEntryTypeInvoice);
				query.Filters.Add(filter);
				Sage.ObjectStore.Filter filter1 = new Sage.ObjectStore.Filter(Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry.FIELD_INSTRUMENTDATE, FilterOperator.GreaterThanOrEqual, from);
				query.Filters.Add(filter1);

				invoices.Find(query);

				// FILTER THE DATA - ONLY UNPAID AND PART PAID INVOICES REQUIRED
				List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry> list = invoices.GetList().Cast<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry>().ToList();
				foreach (Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry entry in list)
				{
					if ((entry.DocumentStatus == Sage.Accounting.AllocationStatusEnum.DocumentStatusBlank || entry.DocumentStatus == Sage.Accounting.AllocationStatusEnum.DocumentStatusPart) && entry.InstrumentNo.Length > 0)
					{
						results.Add(entry);
					}
				}
			}

			return results;
		}

		public static List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry> GetNewInvoices(DateTime from)
		{
			List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry> results = new List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry>();
			if (application != null)
			{
				// PURCHASE INVOICES
				//Sage.Accounting.PurchaseLedger.PurchaseInvoicePostingFactory.Factory.Fetch()
				Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntries invoices = Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntriesFactory.Factory.CreateNew();
				Sage.ObjectStore.Query query = new Sage.ObjectStore.Query();

				Sage.ObjectStore.Filter filter = new Sage.ObjectStore.Filter(Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry.FIELD_ENTRYTYPE, Sage.Accounting.TradingAccountEntryTypeEnum.TradingAccountEntryTypeInvoice);
				query.Filters.Add(filter);

				Sage.ObjectStore.Filter filter1 = new Sage.ObjectStore.Filter(Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry.FIELD_INSTRUMENTDATE, FilterOperator.GreaterThanOrEqual, from);
				query.Filters.Add(filter1);

				invoices.Find(query);

				List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry> list = invoices.GetList().Cast<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry>().ToList();
				foreach(Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry entry in list)
				{
					if ((entry.DocumentStatus == Sage.Accounting.AllocationStatusEnum.DocumentStatusBlank || entry.DocumentStatus == Sage.Accounting.AllocationStatusEnum.DocumentStatusPart) && entry.InstrumentNo.Length > 0)
					{
						results.Add(entry);
					}
				}
			}

			return results;
		}

		public static Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry GetInvoiceByPrimaryKey(string id)
		{
			Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry entry = null;
			if (application != null)
			{
				Sage.Common.Data.DbKey key = new Sage.Common.Data.DbKey(Convert.ToInt32(id));
				entry = Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntryFactory.Factory.Fetch(key);
			}

			return entry;
		}

		public static Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry GetInvoiceByInvoiceNumber(string invoicenumber)
		{
			Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry result = null;
			if (application != null)
			{
				// PURCHASE INVOICES
				Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntries invoices = Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntriesFactory.Factory.CreateNew();
				Sage.ObjectStore.Query query = new Sage.ObjectStore.Query();

				Sage.ObjectStore.Filter filter = new Sage.ObjectStore.Filter(Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry.FIELD_ENTRYTYPE, Sage.Accounting.TradingAccountEntryTypeEnum.TradingAccountEntryTypeInvoice);
				query.Filters.Add(filter);

				Sage.ObjectStore.Filter filter1 = new Sage.ObjectStore.Filter(Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry.FIELD_INSTRUMENTNO, invoicenumber);
				query.Filters.Add(filter1);

				invoices.Find(query);

				result = invoices.First;
			}

			return result;
		}

		public static Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry GetInvoiceByInvoiceNumberAndSupplierPrimaryKey(string id, string invoicenumber)
		{
			Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry result = null;
			if (application != null)
			{
				try
				{
					// PURCHASE INVOICES
					Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntries invoices = Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntriesFactory.Factory.CreateNew();
					Sage.ObjectStore.Query query = new Sage.ObjectStore.Query();

					Sage.ObjectStore.Filter filter = new Sage.ObjectStore.Filter(Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry.FIELD_ENTRYTYPE, Sage.Accounting.TradingAccountEntryTypeEnum.TradingAccountEntryTypeInvoice);
					query.Filters.Add(filter);

					Sage.ObjectStore.Filter filter1 = new Sage.ObjectStore.Filter(Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry.FIELD_INSTRUMENTNO, invoicenumber);
					query.Filters.Add(filter1);

					Sage.Common.Data.DbKey key = new Sage.Common.Data.DbKey(Convert.ToInt32(id));
					Sage.ObjectStore.Filter filter2 = new Sage.ObjectStore.Filter(Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry.FIELD_SUPPLIERACCOUNTDBKEY, key);
					query.Filters.Add(filter2);

					invoices.Find(query);

					result = invoices.First;
				}
				catch(Exception ex)
				{
					result = null;
					Logger.WriteLog(ex);
				}
			}

			return result;
		}

		public static string CreateInvoice(string supplierid, string invoicenumber, string invoicedate, decimal amount, decimal taxamount, List<LineItem> lineitems)
		{
			string newid = "";
			try
			{
				Sage.Accounting.PurchaseLedger.PurchaseInvoiceInstrument invoice = Sage.Accounting.PurchaseLedger.PurchaseInvoiceInstrumentFactory.Factory.CreateNew();

				invoice.SuppressExceedsCreditLimitException = true;
				invoice.Supplier = Sage200Api.GetSupplierByPrimaryKey(supplierid);
				invoice.InstrumentNo = invoicenumber;

				invoice.InstrumentDate = DateTime.ParseExact(invoicedate, "MM/dd/yyyy hh:mm:ss", CultureInfo.InvariantCulture);

				invoice.NetValue = amount - taxamount;
				invoice.TaxValue = taxamount;

				invoice.Authorised = Sage.Accounting.AuthorisationTypeEnum.AuthorisationTypeNotRequired;

				// ADD IN THE LINE ITEMS - GLACCOUNT, DEPARTMENT, CLASSIFICATION (VAT Rate), LINE AMOUNT
				NominalAnalysisItem nominal = (Sage.Accounting.TradeLedger.NominalAnalysisItem) invoice.NominalAnalysisItems[0];
				TaxAnalysisItem tax = (Sage.Accounting.TradeLedger.TaxAnalysisItem)invoice.TaxAnalysisItems[0];

				bool first = true;

				foreach(LineItem lineitem in lineitems)
				{
					if(first)
					{
						first = false;
					}
					else
					{
						nominal = (NominalAnalysisItem)invoice.NominalAnalysisItems.AddNew();
						tax = (TaxAnalysisItem)invoice.TaxAnalysisItems.AddNew();
					}

					// CREATE THE NOMINAL ANALYSIS
					NominalCode nominalcode = Sage200Api.GetNominalCodeByPrimaryKey(lineitem.GLAccountID);
					nominal.NominalSpecification = nominalcode.NominalSpecification;
					nominal.Narrative = lineitem.Description;
					nominal.Amount = lineitem.NetAmount;

					// CREATE THE VAT ANALYSIS
					TaxCode taxcode = Sage200Api.GetVatRateByPrimaryKey(lineitem.ClassificationID);
					tax.TaxCode = taxcode;
					tax.Goods = lineitem.NetAmount;
					tax.TaxAmount = lineitem.TaxAmount;
				}

				//
				invoice.Validate();
				invoice.Update(); // NO WAY TO TELL IF THIS SUCCEEDS!
				newid = invoice.ActualPostedAccountEntry.PrimaryKey.DbValue.ToString();

			}
			catch(Exception ex)
			{
				newid = "";
				Logger.WriteLog(ex);
			}

			return newid;
		}

		public static bool UpdateInvoice(string invoiceid, string newsupplierid, string newinvoicenumber, string newinvoicedate, decimal newamount, decimal newtaxamount)
		{
			bool result = true;
			try
			{
				Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry invoice = Sage200Api.GetInvoiceByPrimaryKey(invoiceid);

				// THESE ARE THE ONLY WRITABLE FIELDS
				invoice.InstrumentNo = newinvoicenumber;
				invoice.InstrumentDate = DateTime.ParseExact(newinvoicedate, "MM/dd/yyyy hh:mm:ss", CultureInfo.InvariantCulture);

				// THESE FIELDS ARE READ ONLY
				//invoice.Supplier = SageApi.GetSupplierByPrimaryKey(supplierid);
				//invoice.NetValue = amount;
				//invoice.TaxValue = taxamount;
				//invoice.Authorised = Sage.Accounting.AuthorisationTypeEnum.AuthorisationTypeNotRequired;

				invoice.Validate();
				invoice.Update(); // NO WAY TO TELL IF THIS SUCCEEDS!

			}
			catch (Exception ex)
			{
				result = false;
			}

			return result;
		}

		#endregion

		#region PAYMENTS

		public static string CreatePayment(Supplier supplier, string bankid, string paymentdate, decimal netamount, decimal taxamount, string paymentreference)
		{
			string newid = "";
			try
			{
				if (Sage.Accounting.CashBook.CashBookModuleFactory.Factory.Fetch().Enabled)
				{
					Sage.Accounting.PurchaseLedger.PurchaseBankPaymentInstrument payment = Sage.Accounting.PurchaseLedger.PurchaseBankPaymentInstrumentFactory.Factory.CreateNew();
					payment.SuppressExceedsCreditLimitException = true;

					// SET THE BANK
					payment.Bank = Sage200Api.GetBankByPrimaryKey(bankid);

					// SET THE SUPPLIER
					payment.Supplier = supplier;

					// REFERENCE
					payment.InstrumentNo = paymentreference;

					// SET THE RECEIPT DATE
					payment.InstrumentDate = DateTime.ParseExact(paymentdate, "MM/dd/yyyy hh:mm:ss", CultureInfo.InvariantCulture);

					// SET THE AMOUNTS
					payment.NetValue = netamount;
					payment.TaxValue = taxamount;

					payment.Validate();

					payment.Update(); // NO WAY TO KNOW IF THIS SUCCEEDS OR NOT!
					newid = payment.ActualPostedAccountEntry.PrimaryKey.DbValue.ToString();
				}
				else
				{
					return "";
				}
			}
			catch(Exception ex)
			{
				newid = "";
				Logger.WriteLog(ex);
			}

			return newid;
		}

		public static Sage.Accounting.PurchaseLedger.PurchaseBankPaymentPosting GetPaymentByPrimaryKey(string id)
		{
			Sage.Accounting.PurchaseLedger.PurchaseBankPaymentPosting payment = null;
			if (application != null)
			{
				Sage.Common.Data.DbKey key = new Sage.Common.Data.DbKey(Convert.ToInt32(id));
				payment = Sage.Accounting.PurchaseLedger.PurchaseBankPaymentPostingFactory.Factory.Fetch(key);
			}

			return payment;
		}

		#endregion

		#region CREDIT NOTES

		public static List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry> GetCreditNotes()
		{
			List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry> list = new List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry>();
			if (application != null)
			{
				Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntries creditnotes = Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntriesFactory.Factory.CreateNew();
				Sage.ObjectStore.Query query = new Sage.ObjectStore.Query();
				Sage.ObjectStore.Filter filter = new Sage.ObjectStore.Filter(Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry.FIELD_ENTRYTYPE, Sage.Accounting.TradingAccountEntryTypeEnum.TradingAccountEntryTypeCreditNote);
				query.Filters.Add(filter);

				creditnotes.Find(query);

				foreach(Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry entry in creditnotes.GetList().Cast<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry>().ToList())
				{
					if(entry.DocumentStatus == Sage.Accounting.AllocationStatusEnum.DocumentStatusBlank && entry.SecondReferenceNo.Length > 0)
					{
						list.Add(entry);
					}
				}
			}

			return list;
		}

		#endregion

		#region VAT RATES / CLASS

		public static List<TaxCode> GetVatRates()
		{
			List<TaxCode> list = null;
			if (application != null)
			{
				Sage.Accounting.TaxModule.TaxCodes codes = Sage.Accounting.TaxModule.TaxCodesFactory.Factory.CreateNew();
				list = codes.GetList().Cast<Sage.Accounting.TaxModule.TaxCode>().ToList();
			}

			return list;
		}

		public static List<TaxCode> GetVatRatesModifiedAfter(DateTime dt)
		{
			List<TaxCode> list = new List<TaxCode>();
			if (application != null)
			{
				Sage.Accounting.TaxModule.TaxCodes codes = Sage.Accounting.TaxModule.TaxCodesFactory.Factory.CreateNew();
				List<TaxCode> temp = codes.GetList().Cast<Sage.Accounting.TaxModule.TaxCode>().ToList();

				foreach (TaxCode code in temp)
				{
					if(code.DateTimeUpdated > dt)
					{
						list.Add(code);
					}
				}
			}

			return list;
		}

		public static TaxCode GetVatRateByPrimaryKey(string key)
		{
			TaxCode taxcode = null;
			if (application != null)
			{
				Sage.Common.Data.DbKey dbkey = new Sage.Common.Data.DbKey(Convert.ToInt32(key));
				taxcode = Sage.Accounting.TaxModule.TaxCodeFactory.Factory.Fetch(dbkey);
			}

			return taxcode;
		}

		#endregion
	}
}
