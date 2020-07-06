using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Common;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace ServiceManager.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UpdateController : Controller
    {
        private readonly ILogger<UpdateController> _logger;
        private readonly string _packagesBasePath;

        public UpdateController(ILogger<UpdateController> logger)
        {
            _logger = logger;
            _packagesBasePath = "packages";
        }

        [HttpPost("Inform")]
        //[ValidateAntiForgeryToken]
        public ActionResult Inform(InformItem informItem)
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            _logger.LogInformation(JsonSerializer.Serialize(informItem));

            try
            {
            }
            catch
            {
            }
            return Ok();
        }

        [HttpGet("GetManifest")]
        public IActionResult GetManifest([FromQuery(Name = "os")] string os)
        {
            if (os != null)
            {
                var file = Path.Combine(Directory.GetCurrentDirectory(), "packages", os, "version.json");

                return PhysicalFile(file, "application/json");
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("GetPackage")]
        public IActionResult GetPackage(PackageItem packageItem)
        {
            if (packageItem != null && packageItem.ZipFileName != null)
            {
                var file = Path.Combine(Directory.GetCurrentDirectory(), "packages", packageItem.ZipFileName);

                return PhysicalFile(file, "application/zip");
            }
            else
            {
                return NotFound();
            }
        }
    }
}