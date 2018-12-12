using GetStartedDotnet.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace GetStartedDotnet.Services
{
    public class CloudantService : ICloudantService
    {
        private static readonly string _dbName = "mydb";
        private readonly Creds _cloudantCreds;
        private readonly UrlEncoder _urlEncoder;
        private readonly IHttpClientFactory _factory;

        public CloudantService(Creds creds, UrlEncoder urlEncoder, IHttpClientFactory factory)
        {
            _cloudantCreds = creds;
            _urlEncoder = urlEncoder;
            _factory = factory;
        }

        public async Task<dynamic> CreateAsync(Visitor item)
        {
            string jsonInString = JsonConvert.SerializeObject(item);

            var _client = _factory.CreateClient("cloudant");

            var response = await _client.PostAsync(_client.BaseAddress+_dbName, new StringContent(jsonInString, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return responseJson;
            }
            else if (Equals(response.ReasonPhrase,"Object Not Found")) //need to create database
            {
                var contents = new StringContent("", Encoding.UTF8, "application/json");
                response = await _client.PutAsync(_client.BaseAddress + _dbName, contents); //creating database using PUT request
                if (response.IsSuccessStatusCode) //if successful, try POST request again
                {
                    response = await _client.PostAsync(_client.BaseAddress+_dbName, new StringContent(jsonInString, Encoding.UTF8, "application/json"));
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        return responseJson;
                    }
                }

            }

            string msg = "Failure to POST. Status Code: " + response.StatusCode + ". Reason: " + response.ReasonPhrase;
            Console.WriteLine(msg);
            return JsonConvert.SerializeObject(new { msg = "Failure to POST. Status Code: " + response.StatusCode + ". Reason: " + response.ReasonPhrase });
        }

        public async Task<dynamic> GetAllAsync()
        {
            var _client = _factory.CreateClient("cloudant");
            var response = await _client.GetAsync(_client.BaseAddress + _dbName+"/_all_docs?include_docs=true");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else if (Equals(response.ReasonPhrase,"Object Not Found")) //need to create database
            {
                var contents = new StringContent("", Encoding.UTF8, "application/json");
                response = await _client.PutAsync(_client.BaseAddress + _dbName, contents); //creating database using PUT request
                if (response.IsSuccessStatusCode) //if successful, try GET request again
                {
                    response = await _client.GetAsync(_client.BaseAddress + _dbName + "/_all_docs?include_docs=true");
                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                }

            }

            string msg = "Failure to GET. Status Code: " + response.StatusCode + ". Reason: " + response.ReasonPhrase;
            Console.WriteLine(msg);
            return JsonConvert.SerializeObject(new { msg = msg });
        }
    }
}
