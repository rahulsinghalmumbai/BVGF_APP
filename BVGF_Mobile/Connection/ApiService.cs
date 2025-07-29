using BVGF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BVGF.Connection
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(AppSettings.BaseApiUrl)
            };
        }
        public async Task<string> LoginAsync(string usermobile)
        {
            try
            {
                var url = $"{Endpoints.Login}?MobileNo={usermobile}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return response.IsSuccessStatusCode ? responseBody : null;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Login API Exception: " + ex.Message);
                throw;
            }
        }

        public async Task<List<MstMember>> GetMembersAsync(
        string company, string category, string name, string city, string mobile)
        {
            // Build query string with parameters
            var queryParams = new Dictionary<string, string>
        {
            { "Company", company },
            { "Category", category },
            { "Name", name },
            { "City", city },
            { "Mobile", mobile }
        };

            var queryString = string.Join("&",
                queryParams
                    .Where(kv => !string.IsNullOrWhiteSpace(kv.Value)) // Skip empty
                    .Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));

            //var url = $"http://172.18.144.1:5151/api/MstMember/search?{queryString}";



            var url = $"{Endpoints.SearchMember}?{queryString}";
            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return new List<MstMember>();

                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<MemberResponseData>>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Data?.Members ?? new List<MstMember>();


            }
            catch (Exception ex)
            {
                Console.WriteLine("API error: " + ex.Message);
                return new List<MstMember>();
            }
        }

        public async Task<List<mstCategary>> GetCategoriesAsync()
        {
            try
            {
                var url = $"{Endpoints.CategaryDrp}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return new List<mstCategary>();

                var responseBody = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<ApiResponse<List<mstCategary>>>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Data ?? new List<mstCategary>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Category API error: " + ex.Message);
                return new List<mstCategary>();
            }
        }


    }

}

