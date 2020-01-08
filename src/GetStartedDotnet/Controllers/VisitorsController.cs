using Microsoft.AspNetCore.Mvc;
using GetStartedDotnet.Models;
using GetStartedDotnet.Services;
using System.Threading.Tasks;
using System.Text.Encodings.Web;
using System;

namespace GetStartedDotnet.Controllers
{
    [Route("api/[controller]")]
    public class VisitorsController : Controller
    {
        private readonly HtmlEncoder _htmlEncoder;
        private readonly ICloudantService _cloudantService;

        public VisitorsController(HtmlEncoder htmlEncoder, ICloudantService cloudantService = null)
        {
            _cloudantService = cloudantService;
            _htmlEncoder = htmlEncoder;
        }

        // GET: api/values
        [HttpGet]
        public async Task<dynamic> Get()
        {
            if (_cloudantService == null)
            {
                return new string[] { "No database connection" };
            }
            else
            {
                return await _cloudantService.GetAllAsync();
            }
        }

        // POST api/values
        [HttpPost]
        public async Task<dynamic> Post([FromBody]Visitor visitor)
        {
            if (_cloudantService != null)
            {
                var response = await _cloudantService.CreateAsync(visitor);
                Console.WriteLine("POST RESULT " + response);
            }

            return new string[] {visitor.Name};
        }
    }
}
