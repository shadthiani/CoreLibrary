using CoreLibrary.Exceptions;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Http;

namespace CoreLibrary.Helpers
{
    public class ApiHelper
    {
        private HttpClient apiClient;
        private readonly IHttpClientFactory httpClientFactory;
        /// <summary>
        /// Constructor for Api Helper
        /// </summary>
        /// <param name="baseAddress"></param>
        public ApiHelper(string baseAddress)
        {
            InitializeClient(baseAddress);
        }

        /// <summary>
        /// Constructor that uses IHttpClientFactory to create the HttpClient.
        /// Provide a client name when you have registered a named client; otherwise pass null or empty to get a default client.
        /// </summary>
        /// <param name="httpClientFactory"></param>
        /// <param name="clientName"></param>
        public ApiHelper(IHttpClientFactory httpClientFactory, string clientName = "")
        {
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            apiClient = this.httpClientFactory.CreateClient(clientName ?? string.Empty);
            apiClient.DefaultRequestHeaders.Accept.Clear();
            apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        /// <summary>
        /// Initialize Api Client with baseAddress
        /// </summary>
        /// <param name="baseAddress"></param>
        private void InitializeClient(string baseAddress)
        {
            apiClient = new HttpClient();
            apiClient.BaseAddress = new Uri(baseAddress);
            apiClient.DefaultRequestHeaders.Accept.Clear();
            apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        /// <summary>
        /// Add JWT Bearer Token to HttpClient Authorization Header
        /// </summary>
        /// <param name="jwtToken"></param>
        public void AddJwtAuthorization(string jwtToken)
        {
            if (!string.IsNullOrEmpty(jwtToken))
            {
                apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }
        }
        /// <summary>
        /// Removes the previously set JWT Bearer Token from the HttpClient Header
        /// </summary>
        public void RemoveJwtAuthorization()
        {
            apiClient.DefaultRequestHeaders.Authorization = null;
        }

        private readonly int maxRetryAttempts = 3;

        private TimeSpan pauseBetweenFailures = TimeSpan.FromSeconds(5);
        public async Task<T> GetAsync<T>(string endPointUrl)
        {
            var response = new HttpResponseMessage();
            await RetryOnExceptionAsync(maxRetryAttempts, pauseBetweenFailures, async () =>
            {
                response = await apiClient.GetAsync(endPointUrl).ConfigureAwait(false);
            }).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<T>(content);
            }
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedException();
            }
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NotFoundException();
            }
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedException();
            }
            else
            {
                throw new Exception("Exception!");
            }
        }

        public async Task<T> PostAsync<T>(string endPointUrl, object data)
        {
            var json = JsonConvert.SerializeObject(data);
            var payload = new StringContent(json, Encoding.UTF8, "application/json");
            var response = new HttpResponseMessage();
            await RetryOnExceptionAsync(maxRetryAttempts, pauseBetweenFailures, async () =>
            {
                response = await apiClient.PostAsync(endPointUrl, payload).ConfigureAwait(false);
            }).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<T>(content);
            }
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedException();
            }
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NotFoundException();
            }
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedException();
            }
            else
            {
                throw new Exception("Exception!");
            }
        }

        public async Task<T> DeleteAsync<T>(string endPointUrl)
        {
            var response = new HttpResponseMessage();
            await RetryOnExceptionAsync(maxRetryAttempts, pauseBetweenFailures, async () =>
            {
                response = await apiClient.DeleteAsync(endPointUrl).ConfigureAwait(false);
            }).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<T>(content);
            }
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedException();
            }
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NotFoundException();
            }
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedException();
            }
            else
            {
                throw new Exception("Exception!");
            }
        }

        public async Task<T> PutAsync<T>(string endPointUrl, object data)
        {
            var json = JsonConvert.SerializeObject(data);
            var payload = new StringContent(json, Encoding.UTF8, "application/json");
            var response = new HttpResponseMessage();
            await RetryOnExceptionAsync(maxRetryAttempts, pauseBetweenFailures, async () =>
            {
                response = await apiClient.PutAsync(endPointUrl, payload).ConfigureAwait(false);
            }).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<T>(content);
            }
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedException();
            }
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NotFoundException();
            }
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedException();
            }
            else
            {
                throw new Exception("Exception!");
            }
        }

        private async Task RetryOnExceptionAsync(int times, TimeSpan delay, Func<Task> operation)
        {
            var attempts = 0;
            do
            {
                try
                {
                    attempts++;
                    await operation().ConfigureAwait(false);
                    break; // Success! Lets exit the loop!
                }
                catch (Exception)
                {
                    if (attempts >= times)
                        throw;
                    await Task.Delay(delay).ConfigureAwait(false);
                }
            } while (attempts <= times);
        }
    }
}
