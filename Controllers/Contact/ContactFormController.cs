using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Backend.Services;
using Backend.Filters;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(CheckSystemLockedFilter))]
    public class ContactFormController : ControllerBase {
        private readonly MyDbContext _context;

        public ContactFormController(MyDbContext context) {
            _context = context;
        }

        public class ContactFormRequest {
            public string senderName { get; set; } = string.Empty;
            public string senderEmail { get; set; } = string.Empty;
            public string message { get; set; } = string.Empty;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SubmitContactForm([FromBody] ContactFormRequest contactFormRequest) {
            if (contactFormRequest == null || !ModelState.IsValid) {
                return BadRequest(new { error = "UERROR: Invalid contact form data" });
            }

            if (string.IsNullOrWhiteSpace(contactFormRequest.senderName) || string.IsNullOrWhiteSpace(contactFormRequest.senderEmail) || string.IsNullOrWhiteSpace(contactFormRequest.message)) {
                return BadRequest(new { error = "UERROR: All fields are required" });
            }

            var contactForm = new ContactForm {
                Id = Utilities.GenerateRandomInt(10000, 99999),
                SenderName = contactFormRequest.senderName,
                SenderEmail = contactFormRequest.senderEmail,
                Message = contactFormRequest.message,
                HasReplied = false
            };

            _context.ContactForms.Add(contactForm);
            await _context.SaveChangesAsync();

            return Ok(new { message = "SUCCESS: Contact form submitted successfully" });
        }
    }
}