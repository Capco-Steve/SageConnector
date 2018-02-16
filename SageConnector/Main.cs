using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MTLib;
using MTLib.Objects;
using SageLib;
using SyncLib;
using Utils;
using Newtonsoft.Json; 

namespace SageConnector
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
			Sync.OnProgress += Sync_OnProgress;
			Sync.OnComplete += Sync_OnComplete;
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
			await Task.Run(() =>
			{
				Sync.SyncAll();
			});

		}

		private void btnSearch_Click(object sender, EventArgs e)
		{
			PurchaseOrder po = new PurchaseOrder();
			po.externalId = "test";
			po.vendor = new ObjID() { id = "vendorid" };
			po.classification = new ObjID() { id = "classificationid" };

			string json = JsonConvert.SerializeObject(po);

			int y = 10;
			//txtResults.Text = "";
			//Write("Start Search");
			//string sessiontoken = MTApi.GetSessionToken();
			//List<Company> companies = MTApi.GetCompaniesForCurrentUser(sessiontoken);
			//Company found = companies.Find(o => o.name == "CAPCO Company");
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
	}
}
