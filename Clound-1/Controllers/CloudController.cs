
using Clound_1.Models;
using Microsoft.AspNetCore.Mvc;
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


        public CloudController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(string weather = null)
        {
            // Получаем погоду для виджета
            var weatherData = await GetWeatherAsync(55.75, 37.62);
            ViewBag.Weather = weatherData;

            // Если передан параметр weather (из кнопки), используем его, иначе берём из данных
            ViewBag.WeatherDescription = weather ?? weatherData.Description;

            var clouds = new List<CloudModel>();
            var random = new Random();

            string[] images = new[]
            {
                "/images/cloud1.png",
                "/images/cloud2.png",
                "/images/cloud3.png",
                "/images/cloud4.png",
                "/images/cloud5.png"
            };

            for (int i = 0; i < 20; i++)
            {
                var cloud = new CloudModel
                {
                    ImageSrc = images[random.Next(images.Length)],
                    TopPercent = random.Next(10, 50), // от 10% до 50% высоты
                    Direction = random.Next(2) == 0 ? "left" : "right",
                    DurationSeconds = random.Next(20, 61), // от 20 до 60 секунд
                    DelaySeconds = random.Next(0, 4),      // от 0 до 3 секунд задержки
                    WidthPixels = random.Next(150, 351)    // от 150 до 350 пикселей
                };
                clouds.Add(cloud);
            }

            return View(clouds);
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
    }
}