using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Services;
using Backend.Filters;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(CheckSystemLockedFilter))]
    public class ContactManagementController(MyDbContext _context) : ControllerBase {
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetContactForms() {
            var contactForms = await _context.ContactForms.ToListAsync();
            return Ok(new { message = "SUCCESS: Contact forms retrieved", data = contactForms });
        }

        [HttpPut("{id}/mark-replied")]
        [Authorize]
        public async Task<IActionResult> MarkAsReplied(int id) {
            var contactForm = await _context.ContactForms.FindAsync(id);
            if (contactForm == null) {
                return NotFound(new { error = "ERROR: Contact form not found" });
            }

            contactForm.HasReplied = true;
            _context.Entry(contactForm).State = EntityState.Modified;

            try {
                await _context.SaveChangesAsync();
                return Ok(new { message = "SUCCESS: Contact form marked as replied" });
            } catch (DbUpdateConcurrencyException) {
                if (!_context.ContactForms.Any(e => e.Id == id)) {
                    return NotFound(new { error = "ERROR: Contact form not found" });
                }

                throw;
            }
        }

        [HttpPost("{id}/send-email")]
        [Authorize]
        public async Task<IActionResult> SendEmail(int id, [FromBody] EmailRequest emailRequest) {
            var contactForm = await _context.ContactForms.FindAsync(id);
            if (contactForm == null) {
                return NotFound(new { error = "ERROR: Contact form not found" });
            }

            string to = contactForm.SenderEmail;
            string subject = emailRequest.Subject;
            string template = "contact_replied";
            var variables = new Dictionary<string, string> {
                { "Name", contactForm.SenderName },
                { "Message", emailRequest.Body }
            };

            var Emailer = new Emailer(_context);
            var emailResult = await Emailer.SendEmailAsync(to, subject, template, variables);

            if (emailResult.StartsWith("ERROR")) {
                return BadRequest(new { error = emailResult });
            }

            return Ok(new { message = "SUCCESS: Email sent successfully" });
        }

        public class EmailRequest {
            public required string Subject { get; set; }
            public required string Body { get; set; }
        }
    }
}