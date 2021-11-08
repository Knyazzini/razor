using BadNews.Repositories.Weather;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BadNews.Components
{
    public class WeatherViewComponent : ViewComponent
    {
        private readonly IWeatherForecastRepository weatherForecastRepository;

        public WeatherViewComponent(IWeatherForecastRepository weatherForecastRepository)
        {
            this.weatherForecastRepository = weatherForecastRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View(await weatherForecastRepository.GetWeatherForecastAsync());
        }
    }
}
