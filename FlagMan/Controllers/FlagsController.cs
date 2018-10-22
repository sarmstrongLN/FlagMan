using System.Collections.Generic;
using FlagMan.DTOs;
using FlagMan.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FlagMan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlagsController : ControllerBase
    {
        private IConfiguration _config;

        public FlagsController(IConfiguration config)
        {
            _config = config;
        }
        // GET api/flags
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }


        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody] FlagRequestDTO value)
        {
            var flags = value.Flags;
            var repo = value.RepoUrl;
            var baseBranch = value.BaseBranch;
            Flag svc = new Flag(_config, flags, repo, baseBranch);
            svc.Process();
            return StatusCode(418);
        }
    }
}
