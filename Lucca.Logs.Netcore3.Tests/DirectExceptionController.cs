using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lucca.Logs.Netcore3.Tests
{
    [Route("api/[controller]")]
    public class DirectExceptionController : Controller
    {
        private readonly ILoggerFactory _loggerFactory;

        public DirectExceptionController(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            try
            {
                throw new NotImplementedException("get");
            }
            catch (Exception e)
            {
                _loggerFactory.CreateLogger<DirectExceptionController>().LogError(e, "DirectExceptionController");
            }
            return Ok(Enumerable.Empty<string>());
        }

        [HttpGet("direct")]
        public ActionResult<IEnumerable<string>> GetDirect()
        {
            throw new NotImplementedException("get");
        }
    }
}