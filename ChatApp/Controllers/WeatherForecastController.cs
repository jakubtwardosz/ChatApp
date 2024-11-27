using ChatApp.Core.Domain;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : Controller
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ChatDbContext _chatDbContext;

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, ChatDbContext chatDbContext)
        {
            _logger = logger;
            _chatDbContext = chatDbContext;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public JsonResult Get()
        {
            var user = _chatDbContext.Users.FirstOrDefault();   

            return Json(user);
        }
    }
}
