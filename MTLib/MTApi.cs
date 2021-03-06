﻿using System;
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
		public static event EventHandler<MTEventArgs> OnError;
		public static bool EnableHTTPLogging = false;
		private static JsonSerializerSettings Settings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore };
		private static JsonSerializer JS = new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore };

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
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

            return token;
        }

		#endregion

		#region EVENTS

		private static void Error(string message)
		{
			if (OnError != null)
			{
				OnError(null, new MTEventArgs() { Message = message });
			}
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
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
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
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return companies;
		}

		#endregion

		#region VENDOR

		public static Vendor GetVendorByExternalID(string companyid, string sessiontoken, string externalid)
		{
			Vendor vendor = null;
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

					if(results.Count() == 1)
					{
						vendor = results[0].ToObject<Vendor>();
					}
				}
			}
			catch (Exception ex)
			{
				vendor = null;
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return vendor;
		}

		public static List<Vendor> GetVendorsByCompanyID(string companyid, string sessiontoken)
		{
			List<Vendor> vendors = new List<Vendor>();
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
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();

					foreach (JToken result in results)
					{
						vendors.Add(result.ToObject<Vendor>());
					}
				}
			}
			catch(Exception ex)
			{
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return vendors;
		}

		public static VendorRoot GetVendorByID(string companyid, string id, string sessiontoken)
		{
			VendorRoot vendorroot = null;
			try
			{
				string url = string.Format("{0}{1}/{2}/objects", MTSettings.BaseUrl, MTSettings.SearchUrl, companyid);

				SearchQuery query = new SearchQuery();
				query.view = "VENDOR";
				query.query = string.Format("id=={0}", id);
				query.page = 0;
				query.count = 1;
				query.sortField = "modified";
				query.sortAsc = true;

				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(query));

				if (json.Length > 0)
				{
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();

					if (results.Count() == 1)
					{
						vendorroot = new VendorRoot();
						vendorroot.vendor = results[0].ToObject<Vendor>();
					}
				}
			}
			catch (Exception ex)
			{
				vendorroot = null;
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return vendorroot;
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
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
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
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return newvendor;
		}

		#endregion

		#region DEPARTMENT

		public static List<Department> GetDepartmentsByCompanyID(string companyid, string sessiontoken)
		{
			List<Department> departments = new List<Department>();
			try
			{
				string url = string.Format("{0}{1}/{2}/objects", MTSettings.BaseUrl, MTSettings.SearchUrl, companyid);

				SearchQuery query = new SearchQuery();
				query.view = "DEPARTMENT";
				query.query = string.Format("dimension_name=={0}", "*");
				query.page = 0;
				query.count = 10000;
				query.sortField = "modified";
				query.sortAsc = true;

				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(query));

				if (json.Length > 0)
				{
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();

					foreach (JToken result in results)
					{
						departments.Add(result.ToObject<Department>());
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return departments;
		}

		public static Department GetDepartmentByExternalID(string companyid, string sessiontoken, string externalid)
		{
			Department department = null;
			try
			{
				string url = string.Format("{0}{1}/{2}/objects", MTSettings.BaseUrl, MTSettings.SearchUrl, companyid);

				SearchQuery query = new SearchQuery();
				query.view = "DEPARTMENT";
				query.query = string.Format("dimension_externalId=={0}", externalid);
				query.page = 0;
				query.count = 1;
				query.sortField = "modified";
				query.sortAsc = true;

				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(query));

				if (json.Length > 0)
				{
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();

					if (results.Count() == 1)
					{
						department = results[0].ToObject<Department>();
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return department;
		}

		public static Department UpdateDepartment(DepartmentRoot departmentroot, string sessiontoken)
		{
			Department updateddepartment = null;
			try
			{
				string url = string.Format("{0}{1}", MTSettings.BaseUrl, MTSettings.DepartmentUrl);
				string json = HTTPRequest(url, "PUT", sessiontoken, JsonConvert.SerializeObject(departmentroot));

				if (json.Length > 0)
				{
					updateddepartment = JsonConvert.DeserializeObject<Department>(json);
				}
			}
			catch (Exception ex)
			{
				updateddepartment = null;
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return updateddepartment;
		}

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
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return newdepartment;
		}

		#endregion

		#region ITEM

		public static Item GetItemByExternalID(string companyid, string sessiontoken, string externalid)
		{
			Item item = null;
			try
			{
				string url = string.Format("{0}{1}/{2}/objects", MTSettings.BaseUrl, MTSettings.SearchUrl, companyid);

				SearchQuery query = new SearchQuery();
				query.view = "ITEM";
				query.query = string.Format("dimension_externalId=={0}", externalid);
				query.page = 0;
				query.count = 1;
				query.sortField = "modified";
				query.sortAsc = true;

				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(query));

				if (json.Length > 0)
				{
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();

					if(results.Count() == 1)
					{
						item = results[0].ToObject<Item>();
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return item;
		}

		public static Item GetItemByName(string companyid, string sessiontoken, string name)
		{
			Item item = null;
			try
			{
				string url = string.Format("{0}{1}/{2}/objects", MTSettings.BaseUrl, MTSettings.SearchUrl, companyid);

				SearchQuery query = new SearchQuery();
				query.view = "ITEM";
				query.query = string.Format("dimension_name=='{0}'", name);
				//query.query = string.Format("dimension_name=in=('{0}')", name);
				query.page = 0;
				query.count = 1;
				query.sortField = "modified";
				query.sortAsc = true;

				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(query));

				if (json.Length > 0)
				{
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();

					if (results.Count() == 1)
					{
						item = results[0].ToObject<Item>();
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return item;
		}

		public static List<Item> GetItemsByCompanyID(string companyid, string sessiontoken)
		{
			List<Item> items = new List<Item>();
			try
			{
				string url = string.Format("{0}{1}/{2}/objects", MTSettings.BaseUrl, MTSettings.SearchUrl, companyid);

				SearchQuery query = new SearchQuery();
				query.view = "ITEM";
				query.query = string.Format("dimension_name=={0}", "*");
				query.page = 0;
				query.count = 10000;
				query.sortField = "modified";
				query.sortAsc = true;

				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(query));

				if (json.Length > 0)
				{
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();

					foreach (JToken result in results)
					{
						items.Add(result.ToObject<Item>());
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return items;
		}

		public static Item UpdateItem(ItemRoot itemroot, string sessiontoken)
		{
			Item updateditem = null;
			try
			{
				string url = string.Format("{0}{1}", MTSettings.BaseUrl, MTSettings.ItemUrl);
				string json = HTTPRequest(url, "PUT", sessiontoken, JsonConvert.SerializeObject(itemroot));

				if (json.Length > 0)
				{
					updateditem = JsonConvert.DeserializeObject<Item>(json);
				}
			}
			catch (Exception ex)
			{
				updateditem = null;
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return updateditem;
		}

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
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return newitem;
		}

		#endregion

		#region NOMINAL CODE / GLACCOUNT

		public static List<GlAccount> GetGlAccountsByCompanyID(string companyid, string sessiontoken)
		{
			List<GlAccount> glaccounts = new List<GlAccount>();
			try
			{
				string url = string.Format("{0}{1}/{2}/objects", MTSettings.BaseUrl, MTSettings.SearchUrl, companyid);

				SearchQuery query = new SearchQuery();
				query.view = "GLACCOUNT";
				query.query = string.Format("gl_account_name=={0}", "*");
				query.page = 0;
				query.count = 10000;
				query.sortField = "modified";
				query.sortAsc = true;

				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(query));

				if (json.Length > 0)
				{
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();

					foreach (JToken result in results)
					{
						glaccounts.Add(result.ToObject<GlAccount>());
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return glaccounts;
		}

		public static GlAccount GetGlAccountByExternalID(string companyid, string sessiontoken, string externalid)
		{
			GlAccount glaccount = null;
			try
			{
				string url = string.Format("{0}{1}/{2}/objects", MTSettings.BaseUrl, MTSettings.SearchUrl, companyid);

				SearchQuery query = new SearchQuery();
				query.view = "GLACCOUNT";
				query.query = string.Format("gl_account_number_externalId=={0}", externalid);
				query.page = 0;
				query.count = 1;
				query.sortField = "modified";
				query.sortAsc = true;

				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(query));

				if (json.Length > 0)
				{
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();

					if (results.Count() == 1)
					{
						glaccount = results[0].ToObject<GlAccount>();
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return glaccount;
		}

		public static GlAccount UpdateGlAccount(GlAccountRoot glaccountroot, string sessiontoken)
		{
			GlAccount updatedglaccount = null;
			try
			{
				string url = string.Format("{0}{1}", MTSettings.BaseUrl, MTSettings.GlAccountUrl);
				string json = HTTPRequest(url, "PUT", sessiontoken, JsonConvert.SerializeObject(glaccountroot));

				if (json.Length > 0)
				{
					updatedglaccount = JsonConvert.DeserializeObject<GlAccount>(json);
				}
			}
			catch (Exception ex)
			{
				updatedglaccount = null;
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return updatedglaccount;
		}

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
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return newglaccount;
		}

		#endregion

		#region COST CENTRE / LOCATION

		public static List<Location> GetLocationsByCompanyID(string companyid, string sessiontoken)
		{
			List<Location> locations = new List<Location>();
			try
			{
				string url = string.Format("{0}{1}/{2}/objects", MTSettings.BaseUrl, MTSettings.SearchUrl, companyid);

				SearchQuery query = new SearchQuery();
				query.view = "LOCATION";
				query.query = string.Format("dimension_name=={0}", "*");
				query.page = 0;
				query.count = 10000;
				query.sortField = "modified";
				query.sortAsc = true;

				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(query));

				if (json.Length > 0)
				{
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();

					foreach (JToken result in results)
					{
						locations.Add(result.ToObject<Location>());
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return locations;
		}

		public static Location GetLocationByExternalID(string companyid, string sessiontoken, string externalid)
		{
			Location location = null;
			try
			{
				string url = string.Format("{0}{1}/{2}/objects", MTSettings.BaseUrl, MTSettings.SearchUrl, companyid);

				SearchQuery query = new SearchQuery();
				query.view = "LOCATION";
				query.query = string.Format("dimension_externalId=={0}", externalid);
				query.page = 0;
				query.count = 1;
				query.sortField = "modified";
				query.sortAsc = true;

				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(query));

				if (json.Length > 0)
				{
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();

					if (results.Count() == 1)
					{
						location = results[0].ToObject<Location>();
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return location;
		}

		public static Location UpdateLocation(LocationRoot locationroot, string sessiontoken)
		{
			Location updatedlocation = null;
			try
			{
				string url = string.Format("{0}{1}", MTSettings.BaseUrl, MTSettings.LocationUrl);
				string json = HTTPRequest(url, "PUT", sessiontoken, JsonConvert.SerializeObject(locationroot));

				if (json.Length > 0)
				{
					updatedlocation = JsonConvert.DeserializeObject<Location>(json);
				}
			}
			catch (Exception ex)
			{
				updatedlocation = null;
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return updatedlocation;
		}

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
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return newlocation;
		}

		#endregion

		#region PAYMENT TERMS

		// TODO: NEEDS TO BE CHECKED AGAINST API DOCS WHEN AVAILABLE
		public static Term GetTermByExternalID(string companyid, string sessiontoken, string externalid)
		{
			Term term = null;
			try
			{
				string url = string.Format("{0}{1}/{2}/objects", MTSettings.BaseUrl, MTSettings.SearchUrl, companyid);

				SearchQuery query = new SearchQuery();
				query.view = "TERMS";
				query.query = string.Format("dimension_externalId=={0}", externalid);
				query.page = 0;
				query.count = 1;
				query.sortField = "modified";
				query.sortAsc = true;

				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(query));

				if (json.Length > 0)
				{
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();

					if (results.Count() == 1)
					{
						term = results[0].ToObject<Term>();
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return term;
		}

		public static Term UpdateTerm(TermRoot termroot, string sessiontoken)
		{
			Term updatedterm = null;
			try
			{
				string url = string.Format("{0}{1}", MTSettings.BaseUrl, MTSettings.TermUrl);
				string json = HTTPRequest(url, "PUT", sessiontoken, JsonConvert.SerializeObject(termroot));

				if (json.Length > 0)
				{
					updatedterm = JsonConvert.DeserializeObject<Term>(json);
				}
			}
			catch (Exception ex)
			{
				updatedterm = null;
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return updatedterm;
		}

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
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
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
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return newpaymentmethod;
		}

		public static PaymentMethod UpdatePaymentMethod(PaymentMethodRoot paymentmethodroot, string sessiontoken)
		{
			PaymentMethod updatedpaymentmethod = null;
			try
			{
				string url = string.Format("{0}{1}", MTSettings.BaseUrl, MTSettings.PaymentMethodUrl);
				string json = HTTPRequest(url, "PUT", sessiontoken, JsonConvert.SerializeObject(paymentmethodroot));

				if (json.Length > 0)
				{
					updatedpaymentmethod = JsonConvert.DeserializeObject<PaymentMethod>(json);
				}
			}
			catch (Exception ex)
			{
				updatedpaymentmethod = null;
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return updatedpaymentmethod;
		}

		public static List<PaymentMethod> GetPaymentMethodsByCompanyName(string companyname, string sessiontoken)
		{
			// PAYMENT METHOD ISN'T SUPPORTED BY SEARCH SO WE HAVE TO QUERY THE COMPANY OBJECT AND GET THE PAYMENT METHODS FROM THAT 
			List<PaymentMethod> paymentmethods = new List<PaymentMethod>();
			List<Company> companies = MTApi.GetCompaniesForCurrentUser(sessiontoken);

			Company company = companies.Find(c => c.name == companyname);

			if(company != null && company.paymentMethods != null)
			{
				paymentmethods = company.paymentMethods;
			}

			return paymentmethods;
		}

		public static PaymentMethod GetPaymentMethodByExternalID(string companyname, string sessiontoken, string externalid)
		{
			// PAYMENT METHOD ISN'T SUPPORTED BY SEARCH SO WE HAVE TO QUERY THE COMPANY OBJECT AND GET THE PAYMENT METHODS FROM THAT 
			List<PaymentMethod> paymentmethods = MTApi.GetPaymentMethodsByCompanyName(companyname, sessiontoken);

			foreach(PaymentMethod p in paymentmethods)
			{
				if(p.externalId == externalid)
				{
					return p;
				}
			}


			return null;
		}

		#endregion

		#region PURCHASE ORDERS

		public static List<PurchaseOrder> GetPurchaseOrdersByCompanyID(string companyid, string sessiontoken)
		{
			List<PurchaseOrder> orders = new List<PurchaseOrder>();
			try
			{
				string url = string.Format("{0}{1}/{2}/objects", MTSettings.BaseUrl, MTSettings.SearchUrl, companyid);

				SearchQuery query = new SearchQuery();
				query.view = "PURCHASE_ORDER";
				query.query = string.Format("purchaseOrder_externalId=={0}", "*");
				query.page = 0;
				query.count = 10000;
				query.sortField = "modified";
				query.sortAsc = true;

				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(query));

				if (json.Length > 0)
				{
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();

					foreach (JToken result in results)
					{
						orders.Add(result.ToObject<PurchaseOrder>());
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLog(ex);
			}

			return orders;
		}

		public static PurchaseOrder UpdatePurchaseOrder(PurchaseOrderRoot purchaseorderroot, string sessiontoken)
		{
			PurchaseOrder updatedpurchaseorder = null;
			try
			{
				string url = string.Format("{0}{1}", MTSettings.BaseUrl, MTSettings.PurchaseOrderUrl);
				string json = HTTPRequest(url, "PUT", sessiontoken, JsonConvert.SerializeObject(purchaseorderroot));

				if (json.Length > 0)
				{
					updatedpurchaseorder = JsonConvert.DeserializeObject<PurchaseOrder>(json);
				}
			}
			catch (Exception ex)
			{
				updatedpurchaseorder = null;
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return updatedpurchaseorder;
		}

		public static PurchaseOrder CreatePurchaseOrder(string companyid, PurchaseOrderRoot purchaseorderroot, string sessiontoken)
		{
			PurchaseOrder newpurchaseorder = null;
			try
			{
				string url = string.Format("{0}{1}/{2}", MTSettings.BaseUrl, MTSettings.PurchaseOrderUrl, companyid);
				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(purchaseorderroot));

				if (json.Length > 0)
				{
					newpurchaseorder = JsonConvert.DeserializeObject<PurchaseOrder>(json);
				}
			}
			catch (Exception ex)
			{
				newpurchaseorder = null;
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return newpurchaseorder;
		}

		public static PurchaseOrder GetPurchaseOrderByExternalID(string companyid, string sessiontoken, string externalid)
		{
			PurchaseOrder order = null;
			try
			{
				string url = string.Format("{0}{1}/{2}/objects", MTSettings.BaseUrl, MTSettings.SearchUrl, companyid);

				SearchQuery query = new SearchQuery();
				query.view = "PURCHASE_ORDER";
				query.query = string.Format("purchaseOrder_externalId=={0}", externalid);
				query.page = 0;
				query.count = 1;
				query.sortField = "modified";
				query.sortAsc = true;

				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(query));

				if (json.Length > 0)
				{
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();
					if(results.Count() == 1)
						order = results[0].ToObject<PurchaseOrder>();
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return order;
		}

		public static List<PurchaseOrder> GetPendingBillingPurchaseOrders(string companyid, string sessiontoken)
		{
			List<PurchaseOrder> orders = new List<PurchaseOrder>();
			try
			{
				string url = string.Format("{0}{1}/{2}/objects", MTSettings.BaseUrl, MTSettings.SearchUrl, companyid);

				SearchQuery query = new SearchQuery();
				query.view = "PURCHASE_ORDER";
				query.query = string.Format("purchaseOrder_status=={0}", "5"); // PENDINGBILLING == 4
				query.page = 0;
				query.count = 10000;
				query.sortField = "modified";
				query.sortAsc = true;

				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(query));

				if (json.Length > 0)
				{
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();

					foreach (JToken result in results)
					{
						orders.Add(result.ToObject<PurchaseOrder>());
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return orders;
		}
		#endregion

		#region INVOICES/BILLS

		public static List<Bill> GetNewBillsWithStatusOpenOrPendingSettlement(string companyid, string sessiontoken)
		{
			List<Bill> bills = new List<Bill>();
			try
			{
				string url = string.Format("{0}{1}/{2}/objects", MTSettings.BaseUrl, MTSettings.SearchUrl, companyid);

				SearchQuery query = new SearchQuery();
				query.view = "BILL";
				// 1 = Open, 2 = Settled/Paid, 3 = Pending Settlement, 4 = Draft, 6 = Closed By Credit, 8 = Awaiting Approval 
				//query.query = string.Format("billOrCredit_status=={0};bill_externalId==''", "1");
				query.query = string.Format("billOrCredit_status=in=({0},{1},{2});bill_externalId==''", "1", "2", "3");
				query.page = 0;
				query.count = 10000;
				query.sortField = "modified";
				query.sortAsc = true;

				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(query));

				if (json.Length > 0)
				{
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();
					foreach (JToken token in results)
					{
						bills.Add(token.ToObject<Bill>(JS));
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return bills;
		}

		public static Bill GetBillByExternalID(string companyid, string sessiontoken, string externalid)
		{
			Bill bill = null;
			try
			{
				string url = string.Format("{0}{1}/{2}/objects", MTSettings.BaseUrl, MTSettings.SearchUrl, companyid);

				SearchQuery query = new SearchQuery();
				query.view = "BILL";
				query.query = string.Format("bill_externalId=={0}", externalid);
				query.page = 0;
				query.count = 1;
				query.sortField = "modified";
				query.sortAsc = true;

				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(query, Settings));

				if (json.Length > 0)
				{
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();

					if(results.Count() == 1)
					{
						bill = results[0].ToObject<Bill>(JS);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return bill;
		}

		public static Bill GetBillByInvoiceNumber(string companyid, string sessiontoken, string invoicenumber)
		{
			Bill bill = null;
			try
			{
				string url = string.Format("{0}{1}/{2}/objects", MTSettings.BaseUrl, MTSettings.SearchUrl, companyid);

				SearchQuery query = new SearchQuery();
				query.view = "BILL";
				query.query = string.Format("bill_invoiceNumber=={0}", invoicenumber);
				query.page = 0;
				query.count = 1;
				query.sortField = "modified";
				query.sortAsc = true;

				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(query, Settings));

				if (json.Length > 0)
				{
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();

					if (results.Count() == 1)
					{
						bill = results[0].ToObject<Bill>(JS);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return bill;
		}

		/*
		public static Bill GetBillByID(string companyid, string sessiontoken, string id)
		{
			Bill bill = null;
			try
			{
				string url = string.Format("{0}{1}/{2}", MTSettings.BaseUrl, MTSettings.BillUrl, companyid);
				string json = HTTPRequest(url, "GET", sessiontoken, null);

				if (json.Length > 0)
				{
					bill = JsonConvert.DeserializeObject<Bill>(json, Settings);
				}
			}
			catch (Exception ex)
			{
				bill = null;
				Logger.WriteLog(ex);
				Error(ex.ToString());
			}

			return bill;
		}
		*/

		public static Bill GetBillByID(string companyid, string sessiontoken, string id)
		{
			Bill bill = null;
			try
			{
				string url = string.Format("{0}{1}/{2}/objects", MTSettings.BaseUrl, MTSettings.SearchUrl, companyid);

				SearchQuery query = new SearchQuery();
				query.view = "BILL";
				query.query = string.Format("id=={0}", id);
				query.page = 0;
				query.count = 1;
				query.sortField = "modified";
				query.sortAsc = true;

				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(query, Settings));

				if (json.Length > 0)
				{
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();

					if (results.Count() == 1)
					{
						bill = results[0].ToObject<Bill>(JS);
					}
				}
			}
			catch (Exception ex)
			{
				bill = null;
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return bill;
		}

		public static List<Bill> GetSettledBills(string companyid, string sessiontoken)
		{
			List<Bill> bills = new List<Bill>();
			try
			{
				string url = string.Format("{0}{1}/{2}/objects", MTSettings.BaseUrl, MTSettings.SearchUrl, companyid);

				SearchQuery query = new SearchQuery();
				query.view = "BILL";
				query.query = string.Format("bill_approvalStatus=={0}", "1"); // 0 = Any
				//query.query = string.Format("billOrCredit_vendorName=={0}", "*"); // 0 = Any
				query.page = 0;
				query.count = 10000;
				query.sortField = "modified";
				query.sortAsc = true;

				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(query));

				if (json.Length > 0)
				{
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();
					foreach (JToken token in results)
					{
						bills.Add(token.ToObject<Bill>());
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return bills;
		}

		public static Bill UpdateBill(BillRoot billroot, string sessiontoken)
		{
			Bill updatedbill = null;
			try
			{
				string url = string.Format("{0}{1}", MTSettings.BaseUrl, MTSettings.BillUrl);
				string json = HTTPRequest(url, "PUT", sessiontoken, JsonConvert.SerializeObject(billroot, Settings));

				if (json.Length > 0)
				{
					updatedbill = JsonConvert.DeserializeObject<Bill>(json, Settings);
				}
			}
			catch (Exception ex)
			{
				updatedbill = null;
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return updatedbill;
		}

		public static Bill CreateBill(string companyid, BillRoot billroot, string sessiontoken)
		{
			Bill newbill = null;
			try
			{
				string url = string.Format("{0}{1}/{2}", MTSettings.BaseUrl, MTSettings.BillUrl, companyid);
				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(billroot, Settings));

				if (json.Length > 0)
				{
					newbill = JsonConvert.DeserializeObject<Bill>(json, Settings);
				}
			}
			catch (Exception ex)
			{
				newbill = null;
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return newbill;
		}

		#endregion

		#region PAYMENTS

		public static List<Payment> GetPayments(string companyid, string sessiontoken)
		{
			List<Payment> payments = new List<Payment>();
			try
			{
				string url = string.Format("{0}{1}/{2}/objects", MTSettings.BaseUrl, MTSettings.SearchUrl, companyid);

				SearchQuery query = new SearchQuery();
				query.view = "PAYMENT";
				// payment_status: Approved = 1000, Paid = 1010, Void = 2000
				// MT have different Paid status depending on the payment type!
				// Manual Check = 1070
				// Biller Portal = 1120
				// Manual Card = 1110
				// All other types = 1010
				//query.query = string.Format("payment_status=={0};payment_externalId==''", "1010"); // THIS NEEDS TO BE CHANGED TO PAID
				query.query = string.Format("payment_status=in=({0},{1},{2},{3});payment_externalId==''", "1010", "1070", "1120", "1110");
				query.page = 0;
				query.count = 10000;
				query.sortField = "modified";
				query.sortAsc = true;

				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(query));

				if (json.Length > 0)
				{
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();

					JsonSerializer js = new JsonSerializer();
					js.NullValueHandling = NullValueHandling.Ignore;

					foreach (JToken token in results)
					{
						payments.Add(token.ToObject<Payment>(js));
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return payments;
		}

		public static Payment UpdatePayment(PaymentRoot paymentroot, string sessiontoken)
		{
			Payment updatedpayment = null;
			try
			{
				string url = string.Format("{0}{1}", MTSettings.BaseUrl, MTSettings.PaymentUrl);
				string json = HTTPRequest(url, "PUT", sessiontoken, JsonConvert.SerializeObject(paymentroot, Settings));

				if (json.Length > 0)
				{
					updatedpayment = JsonConvert.DeserializeObject<Payment>(json, Settings);
				}
			}
			catch (Exception ex)
			{
				updatedpayment = null;
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return updatedpayment;
		}

		#endregion

		#region CREDIT

		public static Credit GetCreditByExternalID(string companyid, string sessiontoken, string externalid)
		{
			Credit credit = null;
			try
			{
				string url = string.Format("{0}{1}/{2}/objects", MTSettings.BaseUrl, MTSettings.SearchUrl, companyid);

				SearchQuery query = new SearchQuery();
				query.view = "CREDIT";
				query.query = string.Format("credit_externalId=={0}", externalid);
				query.page = 0;
				query.count = 1;
				query.sortField = "modified";
				query.sortAsc = true;

				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(query));

				if (json.Length > 0)
				{
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();
					if(results.Count == 1)
						credit = results[0].ToObject<Credit>();
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return credit;
		}

		public static Credit UpdateCredit(CreditRoot creditroot, string sessiontoken)
		{
			Credit updatedcredit = null;
			try
			{
				string url = string.Format("{0}{1}", MTSettings.BaseUrl, MTSettings.CreditUrl);
				string json = HTTPRequest(url, "PUT", sessiontoken, JsonConvert.SerializeObject(creditroot));

				if (json.Length > 0)
				{
					updatedcredit = JsonConvert.DeserializeObject<Credit>(json);
				}
			}
			catch (Exception ex)
			{
				updatedcredit = null;
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return updatedcredit;
		}

		public static Credit CreateCredit(string companyid, CreditRoot creditroot, string sessiontoken)
		{
			Credit newcredit = null;
			try
			{
				string url = string.Format("{0}{1}/{2}", MTSettings.BaseUrl, MTSettings.CreditUrl, companyid);
				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(creditroot));

				if (json.Length > 0)
				{
					newcredit = JsonConvert.DeserializeObject<Credit>(json);
				}
			}
			catch (Exception ex)
			{
				newcredit = null;
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return newcredit;
		}

		#endregion

		#region BILLCREDIT

		public static BillCredit CreateBillCredit(string companyid, BillCreditRoot billcreditroot, string sessiontoken)
		{
			BillCredit newbillcredit = null;
			try
			{
				string url = string.Format("{0}{1}/{2}", MTSettings.BaseUrl, MTSettings.BillCreditUrl, companyid);
				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(billcreditroot));

				if (json.Length > 0)
				{
					newbillcredit = JsonConvert.DeserializeObject<BillCredit>(json);
				}
			}
			catch (Exception ex)
			{
				newbillcredit = null;
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return newbillcredit;
		}

		#endregion

		#region VAT RATES / CLASSIFICATION

		public static List<Classification> GetClassificationByCompanyID(string companyid, string sessiontoken)
		{
			List<Classification> items = new List<Classification>();
			try
			{
				string url = string.Format("{0}{1}/{2}/objects", MTSettings.BaseUrl, MTSettings.SearchUrl, companyid);

				SearchQuery query = new SearchQuery();
				query.view = "CLASS";
				query.query = string.Format("dimension_name=={0}", "*");
				query.page = 0;
				query.count = 10000;
				query.sortField = "modified";
				query.sortAsc = true;

				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(query));

				if (json.Length > 0)
				{
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();

					foreach (JToken result in results)
					{
						items.Add(result.ToObject<Classification>(JS));
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return items;
		}

		public static Classification GetClassificationByExternalID(string companyid, string sessiontoken, string externalid)
		{
			Classification item = null;
			try
			{
				string url = string.Format("{0}{1}/{2}/objects", MTSettings.BaseUrl, MTSettings.SearchUrl, companyid);

				SearchQuery query = new SearchQuery();
				query.view = "CLASS";
				query.query = string.Format("dimension_externalId=={0}", externalid);
				query.page = 0;
				query.count = 1;
				query.sortField = "modified";
				query.sortAsc = true;

				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(query));

				if (json.Length > 0)
				{
					JObject obj = JObject.Parse(json);
					IList<JToken> results = obj["entities"].Children().ToList();

					if (results.Count() == 1)
					{
						item = results[0].ToObject<Classification>();
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return item;
		}

		public static Classification UpdateClassification(ClassificationRoot classificationroot, string sessiontoken)
		{
			Classification updatedclassification = null;
			try
			{
				string url = string.Format("{0}{1}", MTSettings.BaseUrl, MTSettings.ClassificationUrl);
				string json = HTTPRequest(url, "PUT", sessiontoken, JsonConvert.SerializeObject(classificationroot));

				if (json.Length > 0)
				{
					updatedclassification = JsonConvert.DeserializeObject<Classification>(json, Settings);
				}
			}
			catch (Exception ex)
			{
				updatedclassification = null;
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return updatedclassification;
		}

		public static Classification CreateClassification(string companyid, ClassificationRoot classificationroot, string sessiontoken)
		{
			Classification newclassification = null;
			try
			{
				string url = string.Format("{0}{1}/{2}", MTSettings.BaseUrl, MTSettings.ClassificationUrl, companyid);
				string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(classificationroot));

				if (json.Length > 0)
				{
					newclassification = JsonConvert.DeserializeObject<Classification>(json, Settings);
				}
			}
			catch (Exception ex)
			{
				newclassification = null;
				Logger.WriteLog(ex);
				Error(string.Format("Exception: {0}, check log for details", ex.Message));
			}

			return newclassification;
		}

		#endregion

		#region HTTP REQUESTS

		public static string HttpAuthRequest(string url, string method, string username, string password)
		{
			if (MTApi.EnableHTTPLogging == true)
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
				if(MTApi.EnableHTTPLogging == true)
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

				if (MTApi.EnableHTTPLogging == true)
				{
					Logger.WriteLog(string.Format("Token: {0}", token));
				}
			}
			catch (Exception ex)
			{
				if(MTApi.EnableHTTPLogging)
					Logger.WriteLog(ex);
				Error("HTTPAuth Request Exception: check log for details.");
			}

			if (MTApi.EnableHTTPLogging == true)
			{
				Logger.WriteLog("############# End Http Request   #############");
			}

			return token;
		}

		private static string HTTPRequest(string url, string method, string sessiontoken, string json)
		{
			if(MTApi.EnableHTTPLogging == true)
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

				if (MTApi.EnableHTTPLogging == true)
				{
					Logger.WriteLog(string.Format("Response: {0}", results));
					Logger.WriteLog(string.Format("Status Code: {0}", response.StatusCode));
				}
			}
			catch (Exception ex)
			{
				if (MTApi.EnableHTTPLogging)
					Logger.WriteLog(ex);
				Error("HTTPRequest Exception: check log for details.");
			}

			if (MTApi.EnableHTTPLogging == true)
			{
				Logger.WriteLog("############# End Http Request   #############");
			}

			return results;
		}

		#endregion
	}
}
