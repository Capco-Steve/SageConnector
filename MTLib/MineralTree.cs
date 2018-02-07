using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using MTLib.Objects;

namespace MTLib
{
    public static class MineralTree
    {
		#region AUTHENTICATION
		public static string GetSessionToken(string username, string password)
        {
            string token = "";

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(MTSettings.BaseUrl + MTSettings.AuthenticationUrl);
                request.Method = "POST";
                request.Headers.Add(string.Format("Authorization:MT {0}:{1}", username, password));
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
                
            }
            catch (Exception ex)
            {
				// TODO
            }

            return token;
        }

		#endregion

		#region USER

		public static User GetCurrentUser(string sessiontoken)
		{
			User user = null;
			string url = string.Format("{0}{1}", MTSettings.BaseUrl, MTSettings.UserUrl);
			string json = HTTPRequest(url, "GET", sessiontoken, null);

			if(json.Length > 0)
			{
				user = JsonConvert.DeserializeObject<User>(json);
			}

			return user;
		}

		public static List<Company> GetCompaniesForCurrentUser(string sessiontoken)
		{
			List<Company> companies = null;
			string url = string.Format("{0}{1}", MTSettings.BaseUrl, MTSettings.UserCompaniesUrl);
			string json = HTTPRequest(url, "GET", sessiontoken, null);

			if (json.Length > 0)
			{
				companies = JsonConvert.DeserializeObject<List<Company>>(json);
			}

			return companies;
		}

		#endregion

		#region VENDOR

		public static Vendor GetVendorByID(int vendorid, string sessiontoken)
		{
			Vendor vendor = null;
			string url = string.Format("{0}{1}/{2}", MTSettings.BaseUrl, MTSettings.VendorUrl, vendorid);
			string json = HTTPRequest(url, "GET", sessiontoken, null);

			if (json.Length > 0)
			{
				vendor = JsonConvert.DeserializeObject<Vendor>(json);
			}

			return vendor;
		}

		public static Vendor UpdateVendor(int vendorid, Vendor vendor, string sessiontoken)
		{
			// PUT
			Vendor updatedvendor = null;
			string url = string.Format("{0}{1}/{2}", MTSettings.BaseUrl, MTSettings.VendorUrl, vendorid);
			string json = HTTPRequest(url, "PUT", sessiontoken, JsonConvert.SerializeObject(vendor));

			if (json.Length > 0)
			{
				updatedvendor = JsonConvert.DeserializeObject<Vendor>(json);
			}

			return updatedvendor;
		}

		public static Vendor CreateVendor(int companyid, Vendor vendor, string sessiontoken)
		{
			Vendor newvendor = null;
			string url = string.Format("{0}{1}/{2}", MTSettings.BaseUrl, MTSettings.VendorUrl, companyid);
			string json = HTTPRequest(url, "POST", sessiontoken, JsonConvert.SerializeObject(vendor));

			if (json.Length > 0)
			{
				newvendor = JsonConvert.DeserializeObject<Vendor>(json);
			}

			return newvendor;
		}

		#endregion

		#region GENERIC HTTP REQUESTS

		private static string HTTPRequest(string url, string verb, string sessiontoken, string json)
		{
			string results = "";

			try
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
				request.Method = verb;
				request.Proxy = null;
				request.Headers.Add(string.Format("If-Match: {0}", sessiontoken));

				if(json != null)
				{
					request.ContentType = "application/json";
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

			}
			catch (Exception ex)
			{
				// TODO
			}

			return results;
		}

		#endregion
	}
}
