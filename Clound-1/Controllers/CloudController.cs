using Clound_1.Models;
using Microsoft.AspNetCore.Mvc;

namespace Clound_1.Controllers
{
    public class CloudController : Controller
    {
        public IActionResult Index()
        {
            var clouds = new List<CloudModel>();
            var random = new Random();

            // Список доступных картинок (можно добавить свои)
            string[] images = new[]
            {
            "/images/cloud1.png",
            "/images/cloud2.png",
            "/images/cloud3.png",
            "/images/cloud4.png",
            "/images/cloud5.png"
        };

            // Создаём, например, 10 облаков
            for (int i = 0; i < 20; i++)
            {
                var cloud = new CloudModel
                {
                    ImageSrc = images[random.Next(images.Length)],
                    TopPercent = random.Next(10, 50), // от 5% до 80% высоты
                    Direction = random.Next(2) == 0 ? "left" : "right",
                    DurationSeconds = random.Next(20, 61), // от 20 до 60 секунд
                    DelaySeconds = random.Next(0, 4),      // от 0 до 15 секунд задержки
                    WidthPixels = random.Next(150, 351)     // от 150 до 350 пикселей
                };
                clouds.Add(cloud);
            }

            return View(clouds);
        }
    }
}
