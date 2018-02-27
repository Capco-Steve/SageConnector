using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sage.ObjectStore;
using SageState = Sage.Accounting.OrderProcessing.DocumentStatusEnum;

namespace SyncLib
{
	/// <summary>
	/// MAPS SAGE ENUMS TO MINERAL TREE STRINGS
	/// </summary>
	public class EnumMapper
	{
		public static string SageDocumentStatusEnumToMTState(SageState sagestate)
		{
			string state = "";
			switch(sagestate)
			{
				case SageState.EnumDocumentStatusCancelled: state = "Cancelled"; break;
				case SageState.EnumDocumentStatusComplete: state = "Closed"; break;
				case SageState.EnumDocumentStatusDispute: state = "Closed"; break; //??
				case SageState.EnumDocumentStatusDraft: state = "Closed"; break;
				case SageState.EnumDocumentStatusLive: state = "Released"; break;
				case SageState.EnumDocumentStatusOnHold: state = "Closed"; break;
				case SageState.EnumDocumentStatusPrinted: state = "Closed"; break;

				default: state = "Unknown"; break;
			}

			return state;
		}
	}
}
