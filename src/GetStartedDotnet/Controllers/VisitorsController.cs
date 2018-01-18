using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using GetStartedDotnet.Models;
using GetStartedDotnet.Services;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Encodings.Web;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GetStartedDotnet.Controllers
{
    [Route("api/[controller]")]
    public class VisitorsController : Controller
    {
        private readonly HtmlEncoder _htmlEncoder;
        //private readonly VisitorsDbContext _dbContext;
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
                //return _dbContext.Visitors.Select(m => _htmlEncoder.Encode(m.Name)).ToList();
                return await _cloudantService.GetAllAsync();
            }
        }

        // POST api/values
        [HttpPost]
        public async Task<dynamic> Post([FromBody]Visitor visitor)
        {
            //if (_dbContext != null)
            //{
            //    _dbContext.Visitors.Add(visitor);
            //    _dbContext.SaveChanges();
            //}

            if(_cloudantService != null)
            {
                await _cloudantService.CreateAsync(visitor);
            }

            return new string[] { _htmlEncoder.Encode("Hello " + visitor.Name) };
        }
    }
}
