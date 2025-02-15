using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using System.Linq;
using System.Threading.Tasks;
using Backend.Services;
using Backend.Filters;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(CheckSystemLockedFilter))]
    public class RecyclingController(MyDbContext _context) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetClassPoints()
        {
            var classPoints = await _context.Classes
                .Select(c => new
                {
                    ClassID = c.ClassName,
                    TotalPoints = c.ClassPoints
                })
                .ToListAsync();

            return Ok(classPoints);
        }

    }
}