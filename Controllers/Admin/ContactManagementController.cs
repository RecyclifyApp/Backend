using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class ContactManagementController(MyDbContext _context) : ControllerBase {
        [HttpGet]
        public async Task<IActionResult> GetContactForms() {
            var contactForms = await _context.ContactForms.ToListAsync();
            return Ok(contactForms);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContactForm(int id) {
            var contactForm = await _context.ContactForms.FindAsync(id);
            if (contactForm == null) {
                return NotFound();
            }

            _context.ContactForms.Remove(contactForm);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateContactForm(int id, [FromBody] ContactForm updatedContactForm) {
            if (id != updatedContactForm.Id) {
                return BadRequest();
            }

            _context.Entry(updatedContactForm).State = EntityState.Modified;

            try {
                await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                if (!_context.ContactForms.Any(e => e.Id == id)) {
                    return NotFound();
                }
                else {
                    throw;
                }
            }

            return NoContent();
        }
    }
}