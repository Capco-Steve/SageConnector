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

namespace SyncLib
{
	public static class Compare
	{
		public static bool Same(SageNominalCode code, GlAccount glaccount)
		{
			if(code.Reference != glaccount.accountNumber || code.Name != glaccount.name)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		public static bool Same(SageDepartment sagedepartment, Department department)
		{
			if (sagedepartment.Name != department.name)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		public static bool Same(SageStockItem sagestockitem, Item item)
		{
			bool active = sagestockitem.StockItemStatus == Sage.Accounting.Stock.StockItemStatusEnum.EnumStockItemStatusTypeActive ? true : false;
			if (sagestockitem.Name != item.name || item.active != active)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		public static bool Same(SageCostCentre sagecostcentre, Location location)
		{
			if (sagecostcentre.Name != location.name)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		public static bool Same(Bank bank, PaymentMethod paymentmethod)
		{
			if (bank.Name != paymentmethod.bankAccount.name || bank.BankAccount.BankAccountNumber != paymentmethod.bankAccount.accountNumber)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		public static bool Same(TaxCode taxcode, Classification classification)
		{
			string name = string.Format("{0}-{1} ({2})", taxcode.Code, taxcode.Name, taxcode.TaxRate);
			if (name != classification.name)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
	}
}

