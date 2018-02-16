using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using MTLib.Objects;
using Utils;

namespace MTLib
{
    public static class MTApi
    {
		#region AUTHENTICATION

		public static string GetSessionToken()
        {
			string token = "";

            try
            {
				string url = string.Format("{0}{1}", MTSettings.BaseUrl, MTSettings.AuthenticationUrl);
				token = HttpAuthRequest(url, "POST", MTSettings.MTUsername, MTSettings.MTPassword);
            }
            catch (Exception ex)
            {
				token = null;
				Logger.WriteLog(ex);
            }

            return token;
        }

		#endregion

		#region USER

		public static User GetCurrentUser(string sessiontoken)
		{
			User user = null;
			try
			{
				string url = string.Format("{0}{1}", MTSettings.BaseUrl, MTSettings.UserUrl);
				string json = HTTPRequest(url, "GET", sessiontoken, null);

				if (json.Length > 0)
				{
					user = JsonConvert.DeserializeObject<User>(json);
				}
			}
			catch(Exception ex)
			{
				user = null;
				Logger.WriteLog(ex);
			}

			return user;
		}

		public static List<Company> GetCompaniesForCurrentUser(string sessiontoken)
		{
			List<Company> companies = null;
			try
			{
				string url = string.Format("{0}{1}", MTSettings.BaseUrl, MTSettings.UserCompaniesUrl);
				string json = HTTPRequest(url, "GET", sessiontoken, null);

				if (json.Length > 0)
				{
					UserRoot userroot = JsonConvert.DeserializeObject<UserRoot>(json);
					companies = userroot.companies;
				}
			}
			catch(Exception ex)
			{
				companies = null;
				Logger.WriteLog(ex);
			}

			return companies;
		}

		#endregion

		#region VENDOR

		public static VendorRoot GetVendorByExternalID(string companyid, string externalid, string sessiontoken)
		{
			VendorRoot vendorroot = null;
			try
			{
				string url = string.Format("{0}{1}/{2}/objects", MTSettings.BaseUrl, MTSettings.SearchUrl, companyid);

				SearchQuery query = new SearchQuery();
				query.view = "VENDOR";
				query.query = string.Format("vendor_externalId=={0}", externalid);
				query.page = 0;
				query.count = 1;
				query.sortField = "modified";
				query.sortAsc = true;

				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(query));

				if (json.Length > 0)
				{
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();

					foreach (JToken result in results)
					{
						vendorroot = new VendorRoot();
						vendorroot.vendor = result.ToObject<Vendor>();
					}
				}
			}
			catch (Exception ex)
			{
				vendorroot = null;
				Logger.WriteLog(ex);
			}

			return vendorroot;
		}

		public static List<VendorRoot> GetVendorByCompanyID(string companyid, string sessiontoken)
		{
			List<VendorRoot> vendors = null;
			try
			{
				string url = string.Format("{0}{1}/{2}/objects", MTSettings.BaseUrl, MTSettings.SearchUrl, companyid);

				SearchQuery query = new SearchQuery();
				query.view = "VENDOR";
				query.query = string.Format("vendor_name=={0}", "*");
				query.page = 0;
				query.count = 1000;
				query.sortField = "modified";
				query.sortAsc = true;

				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(query));

				if (json.Length > 0)
				{
					vendors = new List<VendorRoot>();
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();

					foreach (JToken result in results)
					{
						VendorRoot vendorroot = new VendorRoot();
						vendorroot.vendor = result.ToObject<Vendor>();
						vendors.Add(vendorroot);
					}
				}
			}
			catch(Exception ex)
			{
				vendors = null;
				Logger.WriteLog(ex);
			}

			return vendors;
		}

		public static Vendor GetVendorByID(int vendorid, string sessiontoken)
		{
			Vendor vendor = null;
			try
			{
				string url = string.Format("{0}{1}/{2}", MTSettings.BaseUrl, MTSettings.VendorUrl, vendorid);
				string json = HTTPRequest(url, "GET", sessiontoken, null);

				if (json.Length > 0)
				{
					vendor = JsonConvert.DeserializeObject<Vendor>(json);
				}
			}
			catch(Exception ex)
			{
				vendor = null;
				Logger.WriteLog(ex);
			}

			return vendor;
		}

		public static Vendor UpdateVendor(string vendorid, VendorRoot vendorroot, string sessiontoken)
		{
			Vendor updatedvendor = null;
			try
			{
				string url = string.Format("{0}{1}", MTSettings.BaseUrl, MTSettings.VendorUrl);
				string json = HTTPRequest(url, "PUT", sessiontoken, JsonConvert.SerializeObject(vendorroot));

				if (json.Length > 0)
				{
					updatedvendor = JsonConvert.DeserializeObject<Vendor>(json);
				}
			}
			catch(Exception ex)
			{
				updatedvendor = null;
				Logger.WriteLog(ex);
			}

			return updatedvendor;
		}

		public static Vendor CreateVendor(string companyid, VendorRoot vendorroot, string sessiontoken)
		{
			Vendor newvendor = null;
			try
			{
				string url = string.Format("{0}{1}/{2}", MTSettings.BaseUrl, MTSettings.VendorUrl, companyid);
				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(vendorroot));

				if (json.Length > 0)
				{
					newvendor = JsonConvert.DeserializeObject<Vendor>(json);
				}
			}
			catch(Exception ex)
			{
				newvendor = null;
				Logger.WriteLog(ex);
			}

			return newvendor;
		}

		#endregion

		#region DEPARTMENT

		public static Department CreateDepartment(string companyid, DepartmentRoot departmentroot, string sessiontoken)
		{
			Department newdepartment = null;
			try
			{
				string url = string.Format("{0}{1}/{2}", MTSettings.BaseUrl, MTSettings.DepartmentUrl, companyid);
				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(departmentroot));

				if (json.Length > 0)
				{
					newdepartment = JsonConvert.DeserializeObject<Department>(json);
				}
			}
			catch(Exception ex)
			{
				newdepartment = null;
				Logger.WriteLog(ex);
			}

			return newdepartment;
		}

		#endregion

		#region ITEM

		public static Item CreateItem(string companyid, ItemRoot itemroot, string sessiontoken)
		{
			Item newitem = null;
			try
			{
				string url = string.Format("{0}{1}/{2}", MTSettings.BaseUrl, MTSettings.ItemUrl, companyid);
				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(itemroot));

				if (json.Length > 0)
				{
					newitem = JsonConvert.DeserializeObject<Item>(json);
				}
			}
			catch(Exception ex)
			{
				newitem = null;
				Logger.WriteLog(ex);
			}

			return newitem;
		}

		#endregion

		#region GLACCOUNT

		public static GlAccount CreateGlAccount(string companyid, GlAccountRoot glaccountroot, string sessiontoken)
		{
			GlAccount newglaccount = null;
			try
			{
				string url = string.Format("{0}{1}/{2}", MTSettings.BaseUrl, MTSettings.GlAccountUrl, companyid);
				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(glaccountroot));

				if (json.Length > 0)
				{
					newglaccount = JsonConvert.DeserializeObject<GlAccount>(json);
				}
			}
			catch(Exception ex)
			{
				newglaccount = null;
				Logger.WriteLog(ex);
			}

			return newglaccount;
		}

		#endregion

		#region COST CENTRE / LOCATION

		public static Location CreateLocation(string companyid, LocationRoot locationroot, string sessiontoken)
		{
			Location newlocation = null;
			try
			{
				string url = string.Format("{0}{1}/{2}", MTSettings.BaseUrl, MTSettings.LocationUrl, companyid);
				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(locationroot));

				if (json.Length > 0)
				{
					newlocation = JsonConvert.DeserializeObject<Location>(json);
				}
			}
			catch(Exception ex)
			{
				newlocation = null;
				Logger.WriteLog(ex);
			}

			return newlocation;
		}

		#endregion

		#region PAYMENT TERMS

		public static Term CreateTerm(string companyid, TermRoot termroot, string sessiontoken)
		{
			Term newterm = null;
			try
			{
				string url = string.Format("{0}{1}/{2}", MTSettings.BaseUrl, MTSettings.TermUrl, companyid);
				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(termroot));

				if (json.Length > 0)
				{
					newterm = JsonConvert.DeserializeObject<Term>(json);
				}
			}
			catch (Exception ex)
			{
				newterm = null;
				Logger.WriteLog(ex);
			}

			return newterm;
		}

		#endregion

		#region PAYMENT METHODS

		public static PaymentMethod CreatePaymentMethod(string companyid, PaymentMethodRoot paymentmethodroot, string sessiontoken)
		{
			PaymentMethod newpaymentmethod = null;
			try
			{
				string url = string.Format("{0}{1}/{2}", MTSettings.BaseUrl, MTSettings.PaymentMethodUrl, companyid);
				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(paymentmethodroot));

				if (json.Length > 0)
				{
					newpaymentmethod = JsonConvert.DeserializeObject<PaymentMethod>(json);
				}
			}
			catch (Exception ex)
			{
				newpaymentmethod = null;
				Logger.WriteLog(ex);
			}

			return newpaymentmethod;
		}

		#endregion

		#region HTTP REQUESTS

		public static string HttpAuthRequest(string url, string method, string username, string password)
		{
			if (MTSettings.LogHTTPRequests == true)
			{
				Logger.WriteLog("############# Start Http Request #############");
				Logger.WriteLog(string.Format("Url: {0}", url));
				Logger.WriteLog(string.Format("Method: {0}", method));
			}

			string token = "";

			try
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
				request.Method = method;
				string header = string.Format("Authorization:MT {0}:{1}", username, password);
				request.Headers.Add(header);
				if(MTSettings.LogHTTPRequests == true)
				{
					Logger.WriteLog(string.Format("Header: {0}", header));
				}
				request.Proxy = null;

				HttpWebResponse response = (HttpWebResponse)request.GetResponse();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					token = response.Headers["ETag"];
				}
				else
				{
					token = "";
				}

				if (MTSettings.LogHTTPRequests == true)
				{
					Logger.WriteLog(string.Format("Token: {0}", token));
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLog(ex);
			}

			if (MTSettings.LogHTTPRequests == true)
			{
				Logger.WriteLog("############# End Http Request   #############");
			}

			return token;
		}

		private static string HTTPRequest(string url, string method, string sessiontoken, string json)
		{
			if(MTSettings.LogHTTPRequests == true)
			{
				Logger.WriteLog("############# Start Http Request #############");
				Logger.WriteLog(string.Format("Url: {0}", url));
				Logger.WriteLog(string.Format("Method: {0}", method));
				Logger.WriteLog(string.Format("Request Body: {0}", json));
			}

			string results = "";

			try
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
				request.Method = method;
				request.Proxy = null;
				request.Headers.Add(string.Format("If-Match: {0}", sessiontoken));

				if (json != null)
				{
					request.ContentType = "application/json;";

					using (var streamwriter = new StreamWriter(request.GetRequestStream()))
					{
						streamwriter.Write(json);
					}
				}

				HttpWebResponse response = (HttpWebResponse)request.GetResponse();

				if (response.StatusCode == HttpStatusCode.OK)
				{
					using (StreamReader streamreader = new StreamReader(response.GetResponseStream()))
					{
						results = streamreader.ReadToEnd();
					}
				}
				else
				{
					results = "";
				}

				if (MTSettings.LogHTTPRequests == true)
				{
					Logger.WriteLog(string.Format("Response: {0}", results));
					Logger.WriteLog(string.Format("Status Code: {0}", response.StatusCode));
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLog(ex);
			}

			if (MTSettings.LogHTTPRequests == true)
			{
				Logger.WriteLog("############# End Http Request   #############");
			}

			return results;
		}

		#endregion
	}
}
