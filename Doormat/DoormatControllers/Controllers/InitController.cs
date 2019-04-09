using DoormatCore;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoormatControllers.Controllers
{
    [Route("api/[controller]")]
    public class InitController : Controller
    {
        // GET api/init
        [HttpGet]
        public IActionResult Get()
        {
            /*var result = new[] {
                new { FirstName = "John", LastName = "Doe" },
                new { FirstName = "Mike", LastName = "Smith" }
            };

            return Ok(result); */
            Program.DiceBot = new Doormat();
            return Ok(Program.DiceBot);
        }
    }
}
