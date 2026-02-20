
using Clound_1.Controllers;
using Clound_1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cloud_1.Controllers
{
    public class CloudController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private static readonly Dictionary<string, (double Lat, double Lon)> Cities = new()
        {
            { "Москва", (55.75, 37.62) },
            { "Казань", (55.79, 49.12) },
            { "Новосибирск", (55.04, 82.93) },
            { "Екатеринбург", (56.84, 60.65) },
            { "Нижний Новгород", (56.33, 44.00) },
            { "Самара", (53.20, 50.15) },
            { "Омск", (54.99, 73.37) },
            { "Челябинск", (55.16, 61.40) },
            { "Ростов-на-Дону", (47.24, 39.71) },
            { "Уфа", (54.74, 55.97) },
            { "Красноярск", (56.02, 92.87) },
            { "Пермь", (58.01, 56.25) },
            { "Воронеж", (51.66, 39.20) },
            { "Волгоград", (48.71, 44.52) }
        };
        public CloudController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        
        public async Task<IActionResult> Index(string city = null)
        {
            Console.WriteLine("=== CloudController.Index вызван ===");

            // 1. Если нужно, получаем погоду для сайдбара (не обязательно для облаков)
            if (!string.IsNullOrEmpty(city) && Cities.ContainsKey(city))
            {
                var coord = Cities[city];
                var weather = await GetWeatherAsync(coord.Lat, coord.Lon, city);
                ViewBag.Weather = weather;
            }

            // 2. Генерируем список облаков (обязательно)
            var clouds = new List<CloudModel>();
            var random = new Random();
            string[] images = new[] { "/images/cloud1.png", "/images/cloud2.png", "/images/cloud3.png", "/images/cloud4.png", "/images/cloud5.png" };

            for (int i = 0; i < 10; i++) // хотя бы несколько облаков
            {
                clouds.Add(new CloudModel
                {
                    ImageSrc = images[random.Next(images.Length)],
                    TopPercent = random.Next(10, 50),
                    Direction = random.Next(2) == 0 ? "left" : "right",
                    DurationSeconds = random.Next(20, 61),
                    DelaySeconds = random.Next(0, 4),
                    WidthPixels = random.Next(150, 351)
                });
            }
            Console.WriteLine($"Сгенерировано облаков: {clouds.Count}");
            if (clouds == null) Console.WriteLine("ОШИБКА: clouds = null");
            else Console.WriteLine($"Передаём в View {clouds.Count} облаков");
            // 3. Передаём список в представление – ЭТО ВАЖНО!
            return View(clouds);
        }


    

        private async Task<WeatherModel> GetWeatherAsync(double lat, double lon, string cityName)
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
                City = cityName
            };
        }
    }
}