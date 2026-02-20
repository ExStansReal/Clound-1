using System.Text.Json;
using Clound_1.Models; // если ещё нет
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Globalization;


namespace Clound_1.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HomeController> _logger;
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

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }


        public async Task<IActionResult> Index(string city = "Москва")
        {
            // Проверяем, есть ли такой город в словаре (Cities должен быть определён)
            if (!Cities.ContainsKey(city))
                city = "Москва";

            var coord = Cities[city];
            var weather = await GetWeatherAsync(coord.Lat, coord.Lon, city);
            ViewBag.Weather = weather;
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetAllWeatherJson()
        {
            try
            {
                var weatherList = new List<WeatherModel>();
                int count = 0;
                foreach (var city in Cities)
                {
                    var weather = await GetWeatherAsync(city.Value.Lat, city.Value.Lon, city.Key);
                    if (weather != null)
                        weatherList.Add(weather);

                    count++;
                    if (count < Cities.Count)
                        await Task.Delay(300);
                }
                return Json(weatherList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в GetAllWeatherJson");
                return Json(new { error = ex.Message });
            }
        }

        private async Task<WeatherModel?> GetWeatherAsync(double lat, double lon, string cityName)
        {
            var url = $"https://api.open-meteo.com/v1/forecast?latitude={lat.ToString(CultureInfo.InvariantCulture)}&longitude={lon.ToString(CultureInfo.InvariantCulture)}&current=temperature_2m,wind_speed_10m&timezone=auto";

            var client = _httpClientFactory.CreateClient("WeatherClient"); // используем именованного клиента

            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения погоды для города {City}. URL: {Url}", cityName, url);
                return null; // возвращаем null, если не удалось получить погоду
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetWeatherJson(string city = "Москва")
        {
            try
            {
                if (!Cities.TryGetValue(city, out var coord))
                    return Json(new { error = "Город не найден" });

                var weather = await GetWeatherAsync(coord.Lat, coord.Lon, city);
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
