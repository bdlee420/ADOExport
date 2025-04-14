using System.Net.Http.Headers;
using System.Text;
using ADOExport.Models;
using ADOExport.Services;
using Newtonsoft.Json;

namespace ADOExport.Common
{
    internal static class WebClientHelper
    {
        internal static void SetClientHeaders(HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Accept.Add(
                   new MediaTypeWithQualityHeaderValue("application/json"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(
                    Encoding.ASCII.GetBytes(
                        string.Format("{0}:{1}", "", token))));
        }

        internal static async Task<Tres> PostAsync<Treq, Tres>(string url, Treq request, string token)
        {
            if (SettingsService.CurrentSettings is null)
                throw new NullReferenceException("SettingsService.CurrentSettings");
            
            using HttpClient client = new();
            SetClientHeaders(client, token);

            var iterationRequestString = JsonConvert.SerializeObject(request);
            var content = new StringContent(iterationRequestString, Encoding.UTF8, "application/json");
            url = $"https://dev.azure.com/{SettingsService.CurrentSettings.RootUrl}/{url}";
            using HttpResponseMessage response = client.PostAsync(url, content).Result;
            string responseBody = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var res = JsonConvert.DeserializeObject<Tres>(responseBody) ?? throw new Exception("Shouldn't be null");
                return res;
            }
            else if (string.IsNullOrEmpty(responseBody))
            {
                Console.WriteLine($"Status Code: {response.StatusCode}");
                throw new Exception("Failed");
            }
            {
                var error = JsonConvert.DeserializeObject<ErrorReponse>(responseBody) ?? throw new Exception("Shouldn't be null");
                Console.WriteLine($"Status Code: {response.StatusCode}; Message: {error.Message}");
                throw new Exception(error.Message);
            }
        }

        internal static async Task<Tres> GetAsync<Tres>(string url, string token)
        {
            if (SettingsService.CurrentSettings is null)
                throw new NullReferenceException("SettingsService.CurrentSettings");

            using HttpClient client = new();
            SetClientHeaders(client, token);
            url = $"https://dev.azure.com/{SettingsService.CurrentSettings.RootUrl}/{url}";
            using HttpResponseMessage response = client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            var res = JsonConvert.DeserializeObject<Tres>(responseBody) ?? throw new Exception("Shouldn't be null");
            return res;
        }
    }
}
