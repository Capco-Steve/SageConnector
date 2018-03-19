using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Sage.Common.Data;
using Supplier = Sage.Accounting.PurchaseLedger.Supplier;
using Bank = Sage.Accounting.CashBook.Bank;
using Department = Sage.Accounting.SystemManager.Department;
using Application = Sage.Accounting.Application;
using StockItem = Sage.Accounting.Stock.StockItem;
using NominalCode = Sage.Accounting.NominalLedger.NominalCode;
using CostCentre = Sage.Accounting.SystemManager.CostCentre;
using POPOrder = Sage.Accounting.POP.POPOrder;

namespace SageLib
{
    public static class SageApi
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

		#endregion

		#region PAYMENT TERMS

		public static List<Tuple<decimal, int, int>> GetPaymentTerms()
		{
			/*
				PAYMENT TERMS ARE STORED AGAINST SUPPLIERS IN SAGE BUT AS SEPARATE OBJECTS IN MT SO WE NEED TO BUILD A UNIQUE LIST FROM SAGE
			*/
			List<Tuple<decimal, int, int>> distinctlist = new List<Tuple<decimal, int, int>>(); ;
			if (application != null)
			{
				Sage.Accounting.PurchaseLedger.Suppliers suppliers = Sage.Accounting.PurchaseLedger.SuppliersFactory.Factory.CreateNew();
				List<Tuple<decimal, int, int>> paymentterms = new List<Tuple<decimal, int, int>>();

				foreach(Supplier supplier in suppliers.GetList().Cast<Supplier>().ToList())
				{
					paymentterms.Add(new Tuple<decimal, int, int>(supplier.EarlySettlementDiscountPercent, supplier.EarlySettlementDiscountDays, supplier.PaymentTermsDays));
				}

				var distinct = paymentterms.Select(item => new { item.Item1, item.Item2, item.Item3 }).Distinct();

				foreach (var c in distinct)
				{
					distinctlist.Add(new Tuple<decimal, int, int>(c.Item1, c.Item2, c.Item3));
				}
			}
			return distinctlist;
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
			List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry> list = new List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry>();
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

				list = invoices.GetList().Cast<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry>().ToList();
			}

			return list;
		}

		public static List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry> GetNewInvoices(DateTime from)
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

				List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry> list = invoices.GetList().Cast<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry>().ToList();
				foreach(Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry entry in list)
				{
					if(entry.DocumentStatus == Sage.Accounting.AllocationStatusEnum.DocumentStatusBlank)
					{
						results.Add(entry);
					}
				}
			}

			return results;
		}

		public static Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry GetPurchaseInvoiceByPrimaryKey(string id)
		{
			Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry entry = null;
			if (application != null)
			{
				Sage.Common.Data.DbKey key = new Sage.Common.Data.DbKey(Convert.ToInt32(id));
				Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry e = Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntryFactory.Factory.Fetch(key);
			}

			return entry;
		}

		public static string CreateInvoice(string supplierid, string invoicenumber, string invoicedate, decimal amount, decimal taxamount)
		{
			string newid = "";
			try
			{
				Sage.Accounting.PurchaseLedger.PurchaseInvoiceInstrument invoice = Sage.Accounting.PurchaseLedger.PurchaseInvoiceInstrumentFactory.Factory.CreateNew();

				invoice.SuppressExceedsCreditLimitException = true;
				invoice.Supplier = SageApi.GetSupplierByPrimaryKey(supplierid);
				invoice.InstrumentNo = invoicenumber;

				invoice.InstrumentDate = DateTime.ParseExact(invoicedate, "MM/dd/yyyy hh:mm:ss", CultureInfo.InvariantCulture);

				invoice.NetValue = amount;
				invoice.TaxValue = taxamount;

				invoice.Authorised = Sage.Accounting.AuthorisationTypeEnum.AuthorisationTypeNotRequired;

				invoice.Validate();
				invoice.Update(); // NO WAY TO TELL IF THIS SUCCEEDS!
				newid = invoice.ActualPostedAccountEntry.PrimaryKey.DbValue.ToString();

			}
			catch(Exception ex)
			{
				newid = "";
			}

			return newid;
		}

		#endregion

		#region PAYMENTS

		public static string CreatePayment(string supplierid, string bankid, string invoiceid, string paymentdate, decimal amount, decimal taxamount)
		{
			string newid = "";
			try
			{
				if (Sage.Accounting.CashBook.CashBookModuleFactory.Factory.Fetch().Enabled)
				{
					Sage.Accounting.PurchaseLedger.PurchaseBankPaymentInstrument payment = Sage.Accounting.PurchaseLedger.PurchaseBankPaymentInstrumentFactory.Factory.CreateNew();
					payment.SuppressExceedsCreditLimitException = true;

					// SET THE BANK
					payment.Bank = SageApi.GetBankByPrimaryKey(bankid);

					// SET THE SUPPLIER
					payment.Supplier = SageApi.GetSupplierByPrimaryKey(supplierid);

					// SET THE RECEIPT DATE
					payment.InstrumentDate = DateTime.ParseExact(paymentdate, "MM/dd/yyyy hh:mm:ss", CultureInfo.InvariantCulture);

					// SET THE TRANSACTION REFERENCES
					Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry invoice = SageApi.GetPurchaseInvoiceByPrimaryKey(invoiceid);
					if(invoice != null)
					{
						payment.InstrumentNo = invoice.InstrumentNo; // ??
					}
					payment.SecondReferenceNo = ""; // ??

					// SET THE AMOUNTS
					payment.NetValue = amount;
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
			}

			return newid;
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

				list = creditnotes.GetList().Cast<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry>().ToList();
			}

			return list;
		}

		#endregion
	}
}
