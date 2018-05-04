using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyncLib
{
	interface ISync
	{
		event EventHandler<SyncEventArgs> OnProgress;
		event EventHandler<SyncEventArgs> OnError;
		event EventHandler<SyncEventArgs> OnCancelled;
		event EventHandler<SyncEventArgs> OnComplete;

		void SyncAll(CancellationToken token, bool fullsync, bool enablehttplogging, DateTime? lastsynctime);
		string GetHistoricalInvoiceCount(DateTime from, bool enablehttplogging);
		void LoadHistoricalInvoices(DateTime from, bool enablehttplogging, DateTime? lastsynctime);
	}
}
