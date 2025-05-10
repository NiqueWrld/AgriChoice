using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AgriChoice.Services
{
    public class DistanceService
    {
        private static readonly HttpClient client = new HttpClient();
        private const string ApiKey = "5b3ce3597851110001cf62489da67d0c6faa4561aba5e35f6783b019"; // Replace with your actual ORS API key

        // Geocode address to (longitude, latitude)
        public async Task<(double lon, double lat)> GeocodeAddress(string address)
        {
            string url = $"https://api.openrouteservice.org/geocode/search?api_key={ApiKey}&text={Uri.EscapeDataString(address)}";

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            var json = JObject.Parse(content);

            // Handle cases where no result is found
            if (json["features"]?.HasValues != true)
            {
                throw new Exception($"Could not geocode address: {address}");
            }

            var coordinates = json["features"][0]["geometry"]["coordinates"];
            double lon = coordinates[0].Value<double>();
            double lat = coordinates[1].Value<double>();

            return (lon, lat);
        }

        // Get distance between coordinates in kilometers
        public async Task<double> GetDistanceInKm(double startLon, double startLat, double endLon, double endLat)
        {
            string url = $"https://api.openrouteservice.org/v2/directions/driving-car?api_key={ApiKey}&start={startLon},{startLat}&end={endLon},{endLat}";

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            var json = JObject.Parse(content);

            double distanceMeters = json["features"][0]["properties"]["segments"][0]["distance"].Value<double>();

            return distanceMeters / 1000.0; // Convert to kilometers
        }

        // Get distance between two addresses
        public async Task<double> GetDistanceBetweenAddresses(string originAddress, string destinationAddress)
        {
            var origin = await GeocodeAddress(originAddress);
            var destination = await GeocodeAddress(destinationAddress);

            return await GetDistanceInKm(origin.lon, origin.lat, destination.lon, destination.lat);
        }

        // Optional: Get a shipping fee based on a custom rate per km
        public async Task<double> CalculateShippingFee(string originAddress, string destinationAddress, double ratePerKm)
        {
            double distance = await GetDistanceBetweenAddresses(originAddress, destinationAddress);
            return distance * ratePerKm;
        }
    }
}
