namespace Clound_1.Models
{
    public class CloudModel
    {
        public string ImageSrc { get; set; }  // путь к картинке
        public int TopPercent { get; set; }   // высота в % (0-100)
        public string Direction { get; set; } // "left" или "right"
        public int DurationSeconds { get; set; } // длительность анимации
        public int DelaySeconds { get; set; }     // задержка перед стартом
        public int WidthPixels { get; set; }      // ширина картинки
    }
}
