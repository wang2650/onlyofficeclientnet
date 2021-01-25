using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnlyOfficeDocumentClientNetCore.Model;
using OnlyOfficeDocumentClientNetCore.Op;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace OnlyOfficeDocumentClientNetCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController :  Controller
    {

    

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
           
    
            Op.ConfigOp configOp = new Op.ConfigOp();
       
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }




        [HttpGet]
        [Route("GetDisplayPageConfig")]
        public JsonResult GetDisplayPageConfig()
        {


            OpResult result = new OpResult();
            string fileId = "53259a15dbb44deda722dd96dc774d87";
            string userId = "1";
            string userName = "songyan";
            bool canEdit = true;
            bool canDownLoad = true;
            string sign = "sign";


            var cfg = ConfigOp.GetDisplayPageConfig(fileId, userId, userName, canEdit, canDownLoad, sign);
            result.Result = cfg;




            return Json(result);


        }



    }
}
