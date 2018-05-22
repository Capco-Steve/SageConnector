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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Diagnostics;

namespace SageConnector
{
    public partial class Main : Form
    {
		private CancellationTokenSource cts;
		private bool ContinuousMode = false;
		private bool SyncRunning = false;

		public Main()
        {
            InitializeComponent();
			Sync.OnProgress += Sync_OnProgress;
			Sync.OnComplete += Sync_OnComplete;
			Sync.OnCancelled += Sync_OnCancelled;
			Sync.OnError += Sync_OnError;
			SyncTimer.Interval = (SageConnectorSettings.MinsBetweenSync * 60) * 1000;
			btnStop.Enabled = false;
			chkHttpLogging.Enabled = true;
			//FindSage200Assemblies();
			//Task.Run(() =>
			//{
			//	CheckVersion();
			//});
		}

		private void Sync_OnError(object sender, SyncEventArgs e)
		{
			Write(e.Message);
		}

		private void Sync_OnCancelled(object sender, SyncEventArgs e)
		{
			Write(e.Message);
			SyncRunning = false;
		}

		private void Sync_OnComplete(object sender, SyncEventArgs e)
		{
			Write(e.Message);
			if(ContinuousMode == true)
			{
				Write(string.Format("Continuous mode: next sync at: {0}", DateTime.Now.AddMilliseconds(SyncTimer.Interval).ToString("HH:mm:ss")));
				StartTimer();
			}
			SyncRunning = false;
			SaveLastSyncTime();
		}

		private void Sync_OnProgress(object sender, SyncEventArgs e)
		{
			Write(e.Message);
		}

		private void btnContinuousSync_Click(object sender, EventArgs e)
		{
			ContinuousMode = true;
			EnableControls(false);
			OnSyncTimer_Tick(null, null);
		}

		private async void OnSyncTimer_Tick(object sender, EventArgs e)
		{
			StopTimer();
			DateTime? lastsynctime = GetLastSyncTime();
			SyncRunning = true;
			cts = new CancellationTokenSource();
			await Task.Run(() =>
			{
				Sync.SyncAll(cts.Token, true, chkHttpLogging.Checked, lastsynctime);
			});
		}

		private void btnStop_Click(object sender, EventArgs e)
		{
			StopTimer();
			cts.Cancel();
			if (SyncRunning == true)
			{
				Write("Cancelling Sync, please wait for pending operations to complete...");
			}
			else
			{
				Write("Sync Cancelled");
				SyncRunning = false;
			}
			EnableControls(true);
		}

		private async void btnFullSync_Click(object sender, EventArgs e)
		{
			EnableControls(false);
			DateTime? lastsynctime = GetLastSyncTime();
			cts = new CancellationTokenSource();
			await Task.Run(() =>
			{
				Sync.SyncAll(cts.Token, true, chkHttpLogging.Checked, lastsynctime);
			});
			SaveLastSyncTime();
			EnableControls(true);
		}

		private async void btnQuickSync_Click(object sender, EventArgs e)
		{
			EnableControls(false);
			DateTime? lastsynctime = GetLastSyncTime();
			cts = new CancellationTokenSource();
			await Task.Run(() =>
			{
				Sync.SyncAll(cts.Token, false, chkHttpLogging.Checked, lastsynctime);
			});
			SaveLastSyncTime();
			EnableControls(true);
		}

		private async void btnLoadHistoricalInvoices_Click(object sender, EventArgs e)
		{
			// GET THE DATE TO START THE SYNC FROM
			HistoricalInvoices hidlg = new HistoricalInvoices();
			if(hidlg.ShowDialog() == DialogResult.OK)
			{
				string data = Sync.GetHistoricalInvoiceCount(hidlg.SelectedDate, chkHttpLogging.Checked);
				if(MessageBox.Show(string.Format("This will upload {0} invoice(s) to APtimise.\r\n\r\nAre you sure you want to continue?", data), "Upload", MessageBoxButtons.YesNo) != DialogResult.Yes)
				{
					return;
				}
				// RUNS THE HISTORICAL INVOICE SYNC
				EnableControls(false);
				btnStop.Enabled = false;
				CancellationToken token = new CancellationToken();
				await Task.Run(() =>
				{
					Sync.LoadHistoricalInvoices(hidlg.SelectedDate, chkHttpLogging.Checked, null);
				}, token);
				EnableControls(true);
			}
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

		delegate void StartTimerDelegate();
		private void StartTimer()
		{
			if (this.InvokeRequired)
			{
				StartTimerDelegate d = new StartTimerDelegate(StartTimer);
				this.Invoke(d, null);
			}
			else
			{
				SyncTimer.Start();
			}
		}

		delegate void StopTimerDelegate();
		private void StopTimer()
		{
			if (this.InvokeRequired)
			{
				StopTimerDelegate d = new StopTimerDelegate(StopTimer);
				this.Invoke(d, null);
			}
			else
			{
				SyncTimer.Stop();
			}
		}

		delegate void TooOldDelegate();
		private void TooOld()
		{
			if (this.InvokeRequired)
			{
				TooOldDelegate d = new TooOldDelegate(TooOld);
				this.Invoke(d, null);
			}
			else
			{
				if (MessageBox.Show("This version is out of date. Upgrade?", "Upgrade", MessageBoxButtons.YesNo) == DialogResult.Yes)
				{
					// START THE UPDATER
					Process.Start("C:\\Windows\\System32\\notepad.exe");
					// EXIT
					Application.Exit();
				}
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
			btnFullSync.Enabled = enable;
			btnQuickSync.Enabled = enable;
			btnContinuousSync.Enabled = enable;
			btnStop.Enabled = !enable;
			chkHttpLogging.Enabled = enable;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			chkHttpLogging.Checked = Properties.Settings.Default.EnableHTTPLogging;
		}

		private void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			Properties.Settings.Default.EnableHTTPLogging = chkHttpLogging.Checked;
			Properties.Settings.Default.Save();
		}

		// LAST SYNC TIME
		private DateTime? GetLastSyncTime()
		{
			string path = string.Format("{0}\\LastSyncTime.dat", Application.StartupPath);
			if(File.Exists(path))
			{
				DateTime dt = File.GetLastAccessTime(path);
				if(dt.IsDaylightSavingTime())
				{
					dt = dt.AddHours(-1); // SAGE DOES NOT ACCOUNT FOR DAYLIGHT SAVING TIME
				}
				return dt;
			}
			else
			{
				return null; // FORCE A FULL SYNC
			}
		}

		private void SaveLastSyncTime()
		{
			string path = string.Format("{0}\\LastSyncTime.dat", Application.StartupPath);
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			FileStream fs = File.Create(path);
			fs.Close();

		}
		//

		private bool CheckVersion()
		{
			// GET THE CURRENT VERSION OF THE ASSEMBLIES
			System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
			Version version = assembly.GetName().Version;

			WebClient client = new WebClient();
			string str = client.DownloadString("http://aptimise/api/version");

			TooOld();
			//version.
			// THE FOLLOWING ASSEMBLIES NEED TO BE UPDATED - WE ARE NOT BOTHERING TO UPDATE SAGE API ASSEMBLIES
			// SageConnector.exe
			// MTLib.dll
			// SageLib.dll
			// SyncLib.dll
			// Utils.dll

			// WE USE THE VERSION NUMBER OF SageConnector.exe TO DETERMINE THE VERSION - DON'T BOTHER WITH INDIVIDUAL 
			// ASSEMBLY VERSIONS

			// UPDATE SEQUENCE -- TODO: APP.CONFIG SETTINGS -- SQLLITE????
			// 1. CHECK VERSION AGAINST REST ENDPOINT
			// 2. IF OLDER THAN CURRENT
			// 3. PROMPT USER TO UPDATE VIA MESSAGE BOX
			// 4. IF YES
			// 5. START UPDATER.EXE, CLOSE SAGECONNECTOR
			// 6. UPDATER.EXE DOWNLOADS CURRENT VERSION AND INSTALLS NEW DLLS
			// 7. UPDATER STARTS SAGECONNECTOR.EXE AND CLOSES ITSELF
			// 8. FINISH
			
			return true;
		}
	}
}