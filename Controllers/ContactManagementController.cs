using System.Linq;
using System.Threading.Tasks;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactManagementController : ControllerBase
    {
        private readonly MyDbContext _context;

        public ContactManagementController(MyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetContactForms()
        {
            var contactForms = await _context.ContactForms.ToListAsync();
            return Ok(contactForms);
        }

        // You can add more actions here as needed, for example:
    }
}