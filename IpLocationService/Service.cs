using IpLocationService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace IpLocationService
{
    public class Service
    {
        /// <summary>
        /// This ip location service uses the http://ip-api.com/ endpoint.
        /// Please refer to their website for usage documentation and restrictions.
        /// </summary>
        public Service()
            :this(null)
        {
        }
        public Service(string apiKey)
        {
            if(!string.IsNullOrWhiteSpace(apiKey))
                ApiKey = apiKey;

            FieldQuery = new string[] { _fieldQueryAll };
        }


        // Private properties

        private const string _fieldQueryAll = "66846719";
        private string ApiKey { get; set; }
        private string[] FieldQuery { get; set; }
        private static HttpClient _httpClient;
        private static HttpClient HttpClient
        {
            get
            {
                if(_httpClient == null)
                {
                    _httpClient = new HttpClient();
                }
                return _httpClient;
            }
        }


        // Private methods

        private void CheckResponseErrors(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == (HttpStatusCode)429)
                {
                    var ex = new Exception("Http 429 - " + response.ReasonPhrase);
                    throw ex;
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized
                    || response.StatusCode == HttpStatusCode.Forbidden)
                {
                    var ex = new Exception(response.ReasonPhrase);
                    throw ex;
                }
            }
        }
        private IPAddress GetParsedIpAddress(string ip)
        {
            IPAddress parsedIp;
            if (!IPAddress.TryParse(ip, out parsedIp))
            {
                throw new ArgumentException("Parameter '" + nameof(ip) + "' is not in a valid IP address format.");
            }
            return parsedIp;
        }
        private string GetUrl(IPAddress parsedIp, Enums.FieldEnum[] fields)
        {
            var uri = new UriBuilder(string.Format("http://ip-api.com/json/{0}", parsedIp.ToString()));
            var query = HttpUtility.ParseQueryString("");
            if(!string.IsNullOrWhiteSpace(ApiKey))
            {
                // Get Pro Url
                uri = new UriBuilder(string.Format("https://pro.ip-api.com/json/{0}", parsedIp.ToString()));
                query.Add("key", ApiKey);
            }

            if(fields != null && fields.Length > 0)
            {
                FieldQuery = fields.Select(x => x.ToString()).ToArray();
            }
            query.Add("fields", string.Join(",", FieldQuery));

            uri.Query = query.ToString();
            return uri.ToString();
        }
        private string GetBatchUrl(Enums.FieldEnum[] fields)
        {
            var uri = new UriBuilder("http://ip-api.com/batch");
            var query = HttpUtility.ParseQueryString("");
            if (!string.IsNullOrWhiteSpace(ApiKey))
            {
                // Get Pro Url
                uri = new UriBuilder("https://pro.ip-api.com/batch");
                query.Add("key", ApiKey);
            }

            if (fields != null && fields.Length > 0)
            {
                FieldQuery = fields.Select(x => x.ToString()).ToArray();
            }
            query.Add("fields", string.Join(",", FieldQuery));

            uri.Query = query.ToString();
            return uri.ToString();
        }
        private string HandleResponse(HttpResponseMessage response)
        {
            CheckResponseErrors(response);

            string responseStr = response.Content.ReadAsStringAsync().Result;
            return responseStr;
        }
        private async Task<string> HandleResponseAsync(HttpResponseMessage response)
        {
            CheckResponseErrors(response);

            string responseStr = await response.Content.ReadAsStringAsync();
            return responseStr;
        }
        private T ParseResponseString<T>(T obj, string responseStr)
        {
            if (!string.IsNullOrWhiteSpace(responseStr))
            {
                obj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(responseStr,
                    new Newtonsoft.Json.JsonSerializerSettings
                    {
                        Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                        {
                            // Handle deserialization error
                            string msg = args.ErrorContext.Error.Message;
                            throw new Exception(msg);
                        }
                    });
            }

            return obj;
        }


        // Public methods

        /// <summary>
        /// Get geographical location information for IP address.
        /// </summary>
        /// <param name="ip"></param>
        /// <returns>IpLocationResponseModel</returns>
        /// <remarks>ip-api.com free includes 45 HTTP requests per minute. Anything over returns "HTTP 429 too many requests".</remarks>
        public IpLocationResponseModel Get(string ip, Enums.FieldEnum[] fields = null)
        {
            IPAddress parsedIp = GetParsedIpAddress(ip);

            IpLocationResponseModel model = null;
            string url = GetUrl(parsedIp, fields);

            try
            {
                var response = HttpClient.GetAsync(url).Result;

                var responseStr = HandleResponse(response);
                model = ParseResponseString(model, responseStr);
            }
            catch(Exception ex)
            {
                throw ex;
            }

            return model;
        }

        /// <summary>
        /// Get geographical location information for IP address.
        /// </summary>
        /// <param name="ip"></param>
        /// <returns>IpLocationResponseModel</returns>
        /// <remarks>ip-api.com free includes 45 HTTP requests per minute. Anything over returns "HTTP 429 too many requests".</remarks>
        public async Task<IpLocationResponseModel> GetAsync(string ip, Enums.FieldEnum[] fields = null)
        {
            IPAddress parsedIp = GetParsedIpAddress(ip);

            IpLocationResponseModel model = null;
            string url = GetUrl(parsedIp, fields);

            try
            {
                var response = await HttpClient.GetAsync(url);

                var responseStr = await HandleResponseAsync(response);
                model = ParseResponseString(model, responseStr);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return model;
        }

        /// <summary>
        /// Get geographical location information for multiple IP addresses.
        /// </summary>
        /// <param name="ips"></param>
        /// <returns>Array of IpLocationResponseModel</returns>
        /// <remarks>ip-api.com free includes 15 HTTP batch requests per minute. Anything over returns "HTTP 429 too many requests".</remarks>
        public IpLocationResponseModel[] GetBatch(string[] ips, Enums.FieldEnum[] fields = null)
        {
            List<IPAddress> parsedIps = new List<IPAddress>();
            foreach (string ip in ips)
            {
                parsedIps.Add(GetParsedIpAddress(ip));
            }

            IpLocationResponseModel[] models = null;
            string url = GetBatchUrl(fields);

            ips = parsedIps.Select(x => x.ToString()).ToArray();

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(url);
            request.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(ips), Encoding.UTF8, "application/json");

            try
            {
                var response = HttpClient.SendAsync(request).Result;

                var responseStr = HandleResponse(response);
                models = ParseResponseString(models, responseStr);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return models;
        }

        /// <summary>
        /// Get geographical location information for multiple IP addresses.
        /// </summary>
        /// <param name="ips"></param>
        /// <returns>Array of IpLocationResponseModel</returns>
        /// <remarks>ip-api.com free includes 15 HTTP batch requests per minute. Anything over returns "HTTP 429 too many requests".</remarks>
        public async Task<IpLocationResponseModel[]> GetBatchAsync(string[] ips, Enums.FieldEnum[] fields = null)
        {
            List<IPAddress> parsedIps = new List<IPAddress>();
            foreach (string ip in ips)
            {
                parsedIps.Add(GetParsedIpAddress(ip));
            }

            IpLocationResponseModel[] models = null;
            string url = GetBatchUrl(fields);

            ips = parsedIps.Select(x => x.ToString()).ToArray();

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(url);
            request.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(ips), Encoding.UTF8, "application/json");

            try
            {
                var response = await HttpClient.SendAsync(request);

                var responseStr = HandleResponse(response);
                models = ParseResponseString(models, responseStr);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return models;
        }
    }
}
