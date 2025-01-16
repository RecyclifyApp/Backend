using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactFormController(MyDbContext _context) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> SubmitContactForm([FromBody] ContactForm contactForm)
        {
            if (contactForm == null || !ModelState.IsValid)
            {
                return BadRequest("Invalid contact form data.");
            }

            // Validate and sanitize data as required
            // Example validation
            if (string.IsNullOrWhiteSpace(contactForm.SenderName)
                || string.IsNullOrWhiteSpace(contactForm.SenderEmail)
                || string.IsNullOrWhiteSpace(contactForm.Message))
            {
                return BadRequest("All fields are required.");
            }
            Console.WriteLine(contactForm);
            // Save the contact form to the database
            _context.ContactForms.Add(contactForm);
            await _context.SaveChangesAsync();

            return Ok("Contact form submitted successfully!");
        }
    }
}