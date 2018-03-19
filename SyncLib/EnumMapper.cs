using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sage.ObjectStore;
using SageInvoiceState = Sage.Accounting.AllocationStatusEnum;

namespace SyncLib
{
	/// <summary>
	/// MAPS SAGE ENUMS TO MINERAL TREE STRINGS
	/// </summary>
	public class EnumMapper
	{
		public static string SageDocumentStatusEnumToMTState(SageInvoiceState sageinvoicestate)
		{
			string state = "";
			switch(sageinvoicestate)
			{
				case SageInvoiceState.DocumentStatusBlank: state = "Open"; break;
				case SageInvoiceState.DocumentStatusPart: state = "Open"; break;
				case SageInvoiceState.DocumentStatusFull: state = "Settled"; break; //??

				default: state = "Unknown"; break;
			}

			return state;
		}
	}
}
