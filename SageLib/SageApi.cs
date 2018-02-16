using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		public static List<POPOrder> GetPurchaseOrders()
		{
			List<POPOrder> list = null;
			if (application != null)
			{
				Sage.Accounting.POP.POPOrders poporders = new Sage.Accounting.POP.POPOrders();
				list = poporders.GetList().Cast<POPOrder>().ToList();
			}

			return list;
		}

		#endregion
	}
}
