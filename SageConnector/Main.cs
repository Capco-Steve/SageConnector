using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MTLib;
using MTLib.Objects;
using SageLib;
using SyncLib;
using Utils;
using Newtonsoft.Json;
using Microsoft.Win32;

namespace SageConnector
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
			btnSearch.Visible = false;
			Sync.OnProgress += Sync_OnProgress;
			Sync.OnComplete += Sync_OnComplete;
			//FindSage200Assemblies();
		}

		private void Sync_OnComplete(object sender, SyncEventArgs e)
		{
			Write(e.Message);
		}

		private void Sync_OnProgress(object sender, SyncEventArgs e)
		{
			Write(e.Message);
		}

		private async void btnRun_Click(object sender, EventArgs e)
        {
			EnableControls(false);
			CancellationToken token = new CancellationToken();
			await Task.Run(() =>
			{
				Sync.SyncAll();
			}, token);
			EnableControls(true);
		}

		private async void btnLoadHistoricalInvoices_Click(object sender, EventArgs e)
		{
			// GET THE DATE TO START THE SYNC FROM
			HistoricalInvoices hidlg = new HistoricalInvoices();
			if(hidlg.ShowDialog() == DialogResult.OK)
			{
				string data = Sync.GetHistoricalInvoiceCount(hidlg.SelectedDate);
				if(MessageBox.Show(string.Format("This will upload the following invoices to Mineral Tree.\r\n{0}\r\nAre you sure you want to continue?", data), "Upload", MessageBoxButtons.YesNo) != DialogResult.Yes)
				{
					return;
				}
				// RUNS THE HISTORICAL INVOICE SYNC
				EnableControls(false);
				CancellationToken token = new CancellationToken();
				await Task.Run(() =>
				{
					Sync.LoadHistoricalInvoices(hidlg.SelectedDate);
				}, token);
				EnableControls(true);
			}
			else
			{
				return;
			}
		}

		private void btnSearch_Click(object sender, EventArgs e)
		{
			//txtResults.Text = "";
			//Write("Start Search");
			//string sessiontoken = MTApi.GetSessionToken();
			//List<Company> companies = MTApi.GetCompaniesForCurrentUser(sessiontoken);
			//Company found = companies.Find(o => o.name == "CAPCO Company");

			//List<Bill> bills = MTApi.GetBillsWithNoExternalID(found.id, sessiontoken);
			//List<Payment> payments = MTApi.GetPayments(found.id, sessiontoken);

			int y = 10;
			

			//List<VendorRoot> vendors = MTApi.GetVendorByCompanyID(companies[0].id, sessiontoken);
			//VendorRoot vr = MTApi.GetVendorByExternalID(found.id, "ATL001", sessiontoken);
			//Write("End Search");
		}

		delegate void WriteDelegate(string text);
		private void Write(string text)
		{
			if (txtResults.InvokeRequired)
			{
				WriteDelegate d = new WriteDelegate(Write);
				this.Invoke(d, new object[] { text });
			}
			else
			{
				txtResults.AppendText(text + "\r\n");
				ActivityLogger.WriteLog(text);
			}
		}

		/// <summary>
		/// Locates and invokes assemblies from the client folder at runtime.
		/// </summary>
		private void FindSage200Assemblies()
		{
			// get registry info for Sage 200 server path
			string path = "";
			RegistryKey root = Registry.CurrentUser;
			RegistryKey key = root.OpenSubKey("Software\\Sage\\MMS");

			if (key != null)
			{
				object value = key.GetValue("ClientInstallLocation");
				if (value != null)
					path = value as string;
			}

			// refer to all installed assemblies based on location of default one
			if (path.Length > 0)
			{
				string assembly = System.IO.Path.Combine(path, "Sage.Common.dll");

				if (System.IO.File.Exists(assembly))
				{
					System.Reflection.Assembly defaultAssembly = System.Reflection.Assembly.LoadFrom(assembly);
					Type type = defaultAssembly.GetType("Sage.Common.Utilities.AssemblyResolver");
					System.Reflection.MethodInfo method = type.GetMethod("GetResolver");
					method.Invoke(null, null);
				}
			}
		}

		private void EnableControls(bool enable)
		{
			btnLoadHistoricalInvoices.Enabled = enable;
			btnRun.Enabled = enable;
			btnSearch.Enabled = enable;
		}
	}
}

