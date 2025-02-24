﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace PartsClient.Data
{
    public static class PartsManager
    {
        // TODO: Add fields for BaseAddress, Url, and authorizationKey
        static readonly string BaseAddress = "https://mslearnpartsserver1922627653.azurewebsites.net";
        static readonly string Url = $"{BaseAddress}/api/";

        private static string authorizationKey;

        static HttpClient client;

        private static async Task<HttpClient> GetClient()
        {
            if (client != null)
                return client;

            client = new HttpClient();

            if (string.IsNullOrEmpty(authorizationKey))
            {
                authorizationKey = await client.GetStringAsync($"{Url}login");
                authorizationKey = JsonConvert.DeserializeObject<string>(authorizationKey);
            }

            client.DefaultRequestHeaders.Add("Authorization", authorizationKey);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            return client;
        }

        public static async Task<IEnumerable<Part>> GetAll()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                return new List<Part>();
            }

            HttpClient client = await GetClient();
            string result = await client.GetStringAsync($"{Url}parts");

            return JsonConvert.DeserializeObject<List<Part>>(result);
        }

        public static async Task<Part> Add(string partName, string supplier, string partType)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                return new Part();
            }

            Part part = new()
            {
                PartName = partName,
                Suppliers = new List<string>(new[] { supplier }),
                PartID = string.Empty,
                PartType = partType,
                PartAvailableDate = DateTime.Now.Date
            };

            HttpClient client = await GetClient();
            var msg = new HttpRequestMessage(HttpMethod.Post, $"{Url}parts");

            msg.Content = JsonContent.Create(part);

            var response = await client.SendAsync(msg);
            response.EnsureSuccessStatusCode();

            var returnedJson = await response.Content.ReadAsStringAsync();
            var insertedPart = JsonConvert.DeserializeObject<Part>(returnedJson);
            return insertedPart;
        }

        public static async Task Update(Part part)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                return;
            }

            HttpClient client = await GetClient();
            var msg = new HttpRequestMessage(HttpMethod.Put, $"{Url}parts/{part.PartID}");

            msg.Content = JsonContent.Create(part);

            var response = await client.SendAsync(msg);
            response.EnsureSuccessStatusCode();
        }

        public static async Task Delete(string partID)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                return;
            }

            HttpClient client = await GetClient();
            var msg = new HttpRequestMessage(HttpMethod.Delete, $"{Url}parts/{partID}");
            
            var response = await client.SendAsync(msg);
            response.EnsureSuccessStatusCode();
        }
    }
}
