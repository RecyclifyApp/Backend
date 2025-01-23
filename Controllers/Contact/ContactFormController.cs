using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactFormController : ControllerBase
    {
        private readonly MyDbContext _context;

        public ContactFormController(MyDbContext context)
        {
            _context = context;
        }

        // Define the ContactFormRequest class inside the controller
        public class ContactFormRequest
        {
            public string senderName { get; set; } = string.Empty;
            public string senderEmail { get; set; } = string.Empty;
            public string message { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitContactForm([FromBody] ContactFormRequest contactFormRequest)
        {
            if (contactFormRequest == null || !ModelState.IsValid)
            {
                return BadRequest("Invalid contact form data.");
            }

            // Validate and sanitize data as required
            if (string.IsNullOrWhiteSpace(contactFormRequest.senderName) || string.IsNullOrWhiteSpace(contactFormRequest.senderEmail) || string.IsNullOrWhiteSpace(contactFormRequest.message))
            {
                return BadRequest("All fields are required.");
            }

            // Map the ContactFormRequest to the ContactForm entity
            var contactForm = new ContactForm
            {
                Id = Utilities.GenerateRandomInt(10000, 99999), // Generate a random ID
                SenderName = contactFormRequest.senderName,
                SenderEmail = contactFormRequest.senderEmail,
                Message = contactFormRequest.message,
                HasReplied = false // Default value
            };

            // Save the contact form to the database
            _context.ContactForms.Add(contactForm);
            await _context.SaveChangesAsync();

            return Ok("Contact form submitted successfully!");
        }
    }
}