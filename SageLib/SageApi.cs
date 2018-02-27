using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
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

		public static bool Connect()
        {
            bool result = true;
            try
            {
                if (application == null)
                {
                    application = new Application();
                    application.Connect();
                    application.ActiveCompany = application.Companies[0]; // JUST USE THE FIRST COMPANY - THIS WILL HAVE TO CHANGE IF THERE ARE MULTIPLE COMPANIES
                }
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

		public static POPOrder GetPurchaseOrderByDocumentNo(string documentno)
		{
			POPOrder poporder = null;
			if (application != null)
			{
				Sage.Accounting.POP.POPOrders poporders = new Sage.Accounting.POP.POPOrders();
				Sage.ObjectStore.Query query = new Sage.ObjectStore.Query();
				Sage.ObjectStore.Filter filter = new Sage.ObjectStore.Filter(Sage.Accounting.POP.POPOrder.FIELD_DOCUMENTNO, documentno);
				query.Filters.Add(filter);

				poporders.Find(query);
				if(poporders.Count == 1) // SHOULD BE JUST ONE - DOCUMENTNO IS UNIQUE
				{
					poporder = poporders.First;
				}
			}

			return poporder;
		}

		#endregion

		#region INVOICES

		public static List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry> GetPurchaseInvoices()
		{
			// instrumentno == externalid
			List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry> list = new List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry>();
			if (application != null)
			{
				// SALES INVOICES - NOT NEEDED
				//Sage.Accounting.SalesLedger.PostedSalesAccountEntries entries = Sage.Accounting.SalesLedger.PostedSalesAccountEntriesFactory.Factory.CreateNew();
				
				// PURCHASE INVOICES - ONLY PURCHASE INVOICES REQUIRED
				Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntries invoices = Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntriesFactory.Factory.CreateNew();
				Sage.ObjectStore.Query query = new Sage.ObjectStore.Query();
				Sage.ObjectStore.Filter filter = new Sage.ObjectStore.Filter(Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry.FIELD_ENTRYTYPE, Sage.Accounting.TradingAccountEntryTypeEnum.TradingAccountEntryTypeInvoice);
				query.Filters.Add(filter);

				invoices.Find(query);

				list = invoices.GetList().Cast<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry>().ToList();
			}

			return list;
		}

		public static void CreateInvoice(string supplierreference, string invoicenumber, string invoicedate, decimal amount, decimal taxamount)
		{
			Sage.Accounting.PurchaseLedger.PurchaseInvoiceInstrument invoice = Sage.Accounting.PurchaseLedger.PurchaseInvoiceInstrumentFactory.Factory.CreateNew();

			invoice.SuppressExceedsCreditLimitException = true;
			invoice.Supplier = Sage.Accounting.PurchaseLedger.SupplierFactory.Factory.Fetch(supplierreference);
			invoice.InstrumentNo = invoicenumber;
			invoice.InstrumentDate = DateTime.ParseExact(invoicedate, "MM/dd/yyyy hh:mm:ss", CultureInfo.InvariantCulture);

			invoice.NetValue = amount;
			invoice.TaxValue = taxamount;

			invoice.Authorised = Sage.Accounting.AuthorisationTypeEnum.AuthorisationTypeNotRequired;


			invoice.Validate();
			invoice.Update(); // NO WAY TO TELL IF THIS SUCCEEDS!
		}

		private static void CreatePayment(string supplierreference, string paymentdate)
		{
			if (Sage.Accounting.CashBook.CashBookModuleFactory.Factory.Fetch().Enabled)
			{
				Sage.Accounting.PurchaseLedger.PurchaseBankReceiptInstrument payment = Sage.Accounting.PurchaseLedger.PurchaseBankReceiptInstrumentFactory.Factory.CreateNew();
				payment.SuppressExceedsCreditLimitException = true;

				// SET THE BANK
				Sage.Accounting.CashBook.Banks banks = Sage.Accounting.CashBook.BanksFactory.Factory.CreateNew();
				banks.Query.Filters.Add(new Sage.ObjectStore.Filter(Sage.Accounting.CashBook.Bank.FIELD_NAME, "Petty Cash (Office)"));
				banks.Find();

				// Set the bank account on the instrument to the petty cash bank account
				payment.Bank = banks.First;

				// SET THE SUPPLIER
				payment.Supplier = Sage.Accounting.PurchaseLedger.SupplierFactory.Factory.Fetch(supplierreference);

				// Set the receipt date
				payment.InstrumentDate = DateTime.ParseExact(paymentdate, "MM/dd/yyyy hh:mm:ss", CultureInfo.InvariantCulture);

				// Set the transaction references - Note: a maximum of 10 characters
				//payment.InstrumentNo = "12345";
				//payment.SecondReferenceNo = "REC1234";

				payment.Validate();

				payment.Update();
			}
			else
			{

			}
		}

			#endregion

			#region CREDIT NOTES

		public static List<Sage.Accounting.PurchaseLedger.PostedPurchaseAccountEntry> GetCreditNotes()
		{
			// instrumentno == externalid
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
