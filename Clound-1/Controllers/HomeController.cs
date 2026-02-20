using System.Text.Json;
using Clound_1.Models; // если ещё нет
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Diagnostics;

namespace Clound_1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }


        public async Task<IActionResult> Index()
        {
            var weather = await GetWeatherAsync(55.75, 37.62); // Москва
            ViewBag.Weather = weather; // передаём в частичное представление
            return View(weather); // передаём модель в представление
        }

        private async Task<WeatherModel> GetWeatherAsync(double lat, double lon)
        {
            var url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current=temperature_2m,wind_speed_10m&timezone=auto";
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                if (root.GetArrayLength() == 0) throw new Exception("Пустой массив");
                root = root[0];
            }

            if (!root.TryGetProperty("current", out var currentElement))
                throw new Exception("Нет поля 'current'");

            if (currentElement.ValueKind == JsonValueKind.Array)
            {
                if (currentElement.GetArrayLength() == 0) throw new Exception("Пустой массив current");
                currentElement = currentElement[0];
            }

            var temp = currentElement.GetProperty("temperature_2m").GetDouble();
            var wind = currentElement.GetProperty("wind_speed_10m").GetDouble();

            string description = temp switch
            {
                < 0 => "❄️ Морозно",
                < 10 => "☁️ Прохладно",
                < 20 => "⛅ Тепло",
                _ => "☀️ Жарко"
            };

            return new WeatherModel
            {
                Temperature = temp,
                WindSpeed = wind,
                Description = description,
                City = "Москва"
            };
        }
        [HttpGet]
        public async Task<IActionResult> GetWeatherJson()
        {
            try
            {
                var weather = await GetWeatherAsync(55.75, 37.62);
                return Json(new
                {
                    city = weather.City,
                    temperature = weather.Temperature,
                    windSpeed = weather.WindSpeed,
                    description = weather.Description
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
