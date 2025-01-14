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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContactForm(int id)
        {
            var contactForm = await _context.ContactForms.FindAsync(id);
            if (contactForm == null)
            {
                return NotFound();
            }

            _context.ContactForms.Remove(contactForm);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}