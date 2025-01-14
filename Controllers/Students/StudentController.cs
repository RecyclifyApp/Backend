using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController (MyDbContext context) : ControllerBase {
        private readonly MyDbContext _context = context;

        [HttpPost]
        public async Task<IActionResult> EnrolStudentIntoClass(string ClassID) {
            // to be changed
            Student dummyStudentObj = new Student {
                StudentID = Utilities.GenerateUniqueID(),
                ClassID = null,
                ParentID = Utilities.GenerateUniqueID(),
                CurrentPoints = 300,
                TotalPoints = 500
            };

            _context.Students.Add(dummyStudentObj);
            await _context.SaveChangesAsync();

            return Ok(dummyStudentObj);
        }
    }
}