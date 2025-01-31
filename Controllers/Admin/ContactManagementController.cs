using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactManagementController(MyDbContext _context) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetContactForms()
        {
            var contactForms = await _context.ContactForms.ToListAsync();
            return Ok(new { message = "SUCCESS: Contact forms retrieved", data = contactForms });
        }

        [HttpPut("{id}/mark-replied")]
        public async Task<IActionResult> MarkAsReplied(int id)
        {
            var contactForm = await _context.ContactForms.FindAsync(id);
            if (contactForm == null)
            {
                return NotFound(new { error = "ERROR: Contact form not found" });
            }

            contactForm.HasReplied = true;
            _context.Entry(contactForm).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ContactForms.Any(e => e.Id == id))
                {
                    return NotFound(new { error = "ERROR: Contact form not found" });
                }
                throw;
            }

            return Ok(new { message = "SUCCESS: Contact form marked as replied" });
        }
    }
}