using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class studentController(MyDbContext context) : ControllerBase {
        private readonly MyDbContext _context = context;

        [HttpGet("get-student")]
        public IActionResult GetStudent([FromQuery] string studentID) {
            if (string.IsNullOrEmpty(studentID)) {
                return BadRequest(new { error = "UERROR: Student ID is required" });
            } else {
                var matchedStudent = _context.Students.FirstOrDefault(s => s.StudentID == studentID);
                if (matchedStudent == null) {
                    return NotFound(new { error = "ERROR: Student not found" });
                }

                var studentClassRecord = _context.ClassStudents.FirstOrDefault(cs => cs.StudentID == matchedStudent.StudentID);
                if (studentClassRecord == null) {
                    return NotFound(new { error = "ERROR: Student's class not found" });
                }

                var classStudents = _context.ClassStudents
                    .Where(cs => cs.ClassID == studentClassRecord.ClassID)
                    .Join(_context.Students,
                        cs => cs.StudentID,
                        s => s.StudentID,
                        (cs, s) => s)
                    .ToList();

                var classStudentsRanked = classStudents.OrderByDescending(s => s.TotalPoints).ToList();
                matchedStudent.LeagueRank = classStudentsRanked.FindIndex(s => s.StudentID == studentID) + 1;

                _context.SaveChanges();

                return Ok(new { message = "SUCCESS: Student details retrieved", data = matchedStudent });
            }
        }

        [HttpGet("get-student-leafs")]
        public IActionResult GetStudentLeafs([FromQuery] string studentID) {
            if (string.IsNullOrEmpty(studentID)) {
                return BadRequest(new { error = "UERROR: Student ID is required" });
            } else {
                var matchedStudent = _context.Students.FirstOrDefault(s => s.StudentID == studentID);
                if (matchedStudent == null) {
                    return NotFound(new { error = "ERROR: Student not found" });
                }

                return Ok(new { message = "SUCCESS: Student leafs retrieved", data = matchedStudent.CurrentPoints });
            }
        }

        [HttpGet("get-all-students")]
        public async Task<IActionResult> GetAllStudents([FromQuery] string studentID) {
            if (string.IsNullOrEmpty(studentID)) {
                return BadRequest(new { error = "UERROR: Student ID is required" });
            }

            var studentClass = await _context.ClassStudents.FirstOrDefaultAsync(cs => cs.StudentID == studentID);

            if (studentClass == null) {
                return NotFound(new { error = "ERROR: Student's class not found" });
            }

            var allStudents = await _context.ClassStudents
                .Where(cs => cs.ClassID == studentClass.ClassID)
                .Include(cs => cs.Student)
                .Where(cs => cs.Student != null && cs.Student.User != null)
                .OrderByDescending(cs => cs.Student!.TotalPoints)
                .Select(cs => new {
                    StudentID = cs.Student!.StudentID,
                    cs.ClassID,
                    ParentID = cs.Student.ParentID,
                    cs.Student.League,
                    cs.Student.LeagueRank,
                    cs.Student.CurrentPoints,
                    cs.Student.TotalPoints,
                    cs.Student.UserID,
                    cs.Student.TaskLastSet,
                    cs.Student.Streak,
                    cs.Student.LastClaimedStreak,
                    Name = cs.Student.User!.Name
                }).ToListAsync();

            return Ok(new { message = "SUCCESS: All students retrieved", data = allStudents });
        }


        [HttpGet("get-student-tasks")]
        public async Task<IActionResult> GetStudentTasks([FromQuery] string studentID) {
            if (string.IsNullOrEmpty(studentID)) {
                return BadRequest(new { error = "UERROR: Student ID is required" });
            } else {
                var matchedStudent = _context.Students.FirstOrDefault(s => s.StudentID == studentID);
                if (matchedStudent == null) {
                    return NotFound(new { error = "ERROR: Student not found" });
                } else {
                    var studentTaskProgresses = _context.TaskProgresses.Where(tp => tp.StudentID == matchedStudent.StudentID).ToList();
                    var todayTaskProgresses = studentTaskProgresses.Where(tp => DateTime.Parse(tp.DateAssigned) == DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))).ToList();
                    
                    if (todayTaskProgresses.Count == 0) {
                        var allTasks = _context.Tasks.ToList();
                        var randomTasks = allTasks.OrderBy(t => Utilities.GenerateUniqueID()).Take(3).ToList();
                        foreach (var task in randomTasks) {
                            var studentClassRecord = _context.ClassStudents.FirstOrDefault(cs => cs.StudentID == matchedStudent.StudentID);
                            if (studentClassRecord == null) {
                                return NotFound(new { error = "ERROR: Student's class not found" });
                            }

                            var studentClass = _context.Classes.FirstOrDefault(c => c.ClassID == studentClassRecord.ClassID);
                            if (studentClass == null) {
                                return NotFound(new { error = "ERROR: Student's class not found" });
                            }

                            var assignedTeacher = _context.Teachers.FirstOrDefault(t => t.TeacherID == studentClass.TeacherID);
                            if (assignedTeacher == null) {
                                return NotFound(new { error = "ERROR: Class's teacher not found" });
                            }

                            var taskProgress = new TaskProgress {
                                Task = task,
                                AssignedTeacher = assignedTeacher,
                                Student = matchedStudent,
                                TaskID = task.TaskID,
                                StudentID = matchedStudent.StudentID,
                                TaskVerified = false,
                                VerificationPending = false,
                                AssignedTeacherID = assignedTeacher.TeacherID,
                                DateAssigned = DateTime.Now.ToString("yyyy-MM-dd")
                            };

                            _context.TaskProgresses.Add(taskProgress);
                            _context.SaveChanges();
                        }
                        return Ok(new { message = "SUCCESS: Student tasks assigned", data = randomTasks });       
                    } else {
                        var studentTasks = new List<dynamic>();
                        foreach (var task in todayTaskProgresses) {
                            var foundTask = _context.Tasks.FirstOrDefault(t => t.TaskID == task.TaskID);
                            if (foundTask != null) {
                                var taskProgress = await _context.TaskProgresses.Where(tp => tp.TaskID == foundTask.TaskID && tp.StudentID == matchedStudent.StudentID).ToListAsync();
                                var selectedTaskProgress = taskProgress.OrderByDescending(tp => tp.DateAssigned).FirstOrDefault();
                                if (selectedTaskProgress == null) {
                                    return NotFound(new { error = "ERROR: Existing Task Progress not found", data = taskProgress });
                                }

                                studentTasks.Add(new {
                                    foundTask.TaskID,
                                    foundTask.TaskTitle,
                                    foundTask.TaskDescription,
                                    foundTask.TaskPoints,
                                    selectedTaskProgress.VerificationPending,
                                    selectedTaskProgress.TaskVerified,
                                });
                            }
                        }
                        return Ok(new { message = "SUCCESS: Student tasks retrieved", data = studentTasks });
                    }
                }
            }
        }

        [HttpGet("get-student-chart-statistics")]
        public IActionResult GetStudentChartStatistics([FromQuery] string studentID) {
            if (string.IsNullOrEmpty(studentID)) {
                return BadRequest(new { error = "UERROR: Student ID is required" });
            } else {
                var matchedStudent = _context.Students.FirstOrDefault(s => s.StudentID == studentID);
                if (matchedStudent == null) {
                    return NotFound(new { error = "ERROR: Student not found" });
                } else {
                    var studentPointRecords = _context.StudentPoints.Where(sp => sp.StudentID == studentID).ToList();
                    var studentPointsObj = new Dictionary<string, int> {
                        { "Monday", studentPointRecords.Where(sp => DateTime.Parse(sp.DateCompleted).DayOfWeek.ToString() == "Monday").Sum(sp => sp.PointsAwarded) },
                        { "Tuesday", studentPointRecords.Where(sp => DateTime.Parse(sp.DateCompleted).DayOfWeek.ToString() == "Tuesday").Sum(sp => sp.PointsAwarded) },
                        { "Wednesday", studentPointRecords.Where(sp => DateTime.Parse(sp.DateCompleted).DayOfWeek.ToString() == "Wednesday").Sum(sp => sp.PointsAwarded) },
                        { "Thursday", studentPointRecords.Where(sp => DateTime.Parse(sp.DateCompleted).DayOfWeek.ToString() == "Thursday").Sum(sp => sp.PointsAwarded) },
                        { "Friday", studentPointRecords.Where(sp => DateTime.Parse(sp.DateCompleted).DayOfWeek.ToString() == "Friday").Sum(sp => sp.PointsAwarded) },
                        { "Saturday", studentPointRecords.Where(sp => DateTime.Parse(sp.DateCompleted).DayOfWeek.ToString() == "Saturday").Sum(sp => sp.PointsAwarded) },
                        { "Sunday", studentPointRecords.Where(sp => DateTime.Parse(sp.DateCompleted).DayOfWeek.ToString() == "Sunday").Sum(sp => sp.PointsAwarded) }
                    };

                    return Ok(new { message = "SUCCESS: Student chart statistics retrieved", data = studentPointsObj });
                }
            }
        }

        [HttpGet("get-all-rewards")]
        public async Task<IActionResult> GetAllRewards() {
            var allRewards = await _context.RewardItems.ToListAsync();
            return Ok(new { message = "SUCCESS: All rewards retrieved", data = allRewards });
        }

        [HttpPost("submit-task")]
        public async Task<IActionResult> SubmitTask([FromForm] IFormFile file, [FromForm] string taskID, [FromForm] string studentID) {
            if (file == null || file.Length == 0) {
                return BadRequest(new { error = "UERROR: No file uploaded" });
            } 

            if (string.IsNullOrEmpty(taskID) || string.IsNullOrEmpty(studentID)) {
                return BadRequest(new { error = "UERROR: Task ID and Student ID are required" });
            } else {
                try {
                    var task = _context.Tasks.FirstOrDefault(t => t.TaskID == taskID);
                    if (task == null) {
                        return NotFound(new { error = "ERROR: Associated task not found" });
                    }

                    var student = _context.Students.FirstOrDefault(s => s.StudentID == studentID);
                    if (student == null) {
                        return NotFound(new { error = "ERROR: Student not found" });
                    }

                    var studentClassRecord = _context.ClassStudents.FirstOrDefault(cs => cs.StudentID == student.StudentID);
                    if (studentClassRecord == null) {
                        return NotFound(new { error = "ERROR: Student's class not found" });
                    }
                    var studentClass = _context.Classes.FirstOrDefault(c => c.ClassID == studentClassRecord.ClassID);
                    if (studentClass == null) {
                        return NotFound(new { error = "ERROR: Student's class not found" });
                    }

                    var assignedTeacher = _context.Teachers.FirstOrDefault(t => t.TeacherID == studentClass.TeacherID);
                    if (assignedTeacher == null) {
                        return NotFound(new { error = "ERROR: Class's teacher not found" });
                    }

                    var taskProgress = await _context.TaskProgresses.Where(tp => tp.TaskID == taskID && tp.StudentID == studentID).ToListAsync();
                    var selectedTaskProgress = taskProgress.OrderByDescending(tp => tp.DateAssigned).FirstOrDefault();
                    if (selectedTaskProgress == null) {
                        return NotFound(new { error = "ERROR: Task progress not found", data = taskProgress });
                    }

                    try {
                        await AssetsManager.UploadFileAsync(file);
                        selectedTaskProgress.ImageUrls = await AssetsManager.GetFileUrlAsync(file.FileName);
                        selectedTaskProgress.VerificationPending = true;

                        try {
                            await _context.SaveChangesAsync();
                            return Ok(new { message = "SUCCESS: Task submitted successfully" });
                        } catch (Exception ex) {
                            return StatusCode(500, new { error = "ERROR: Failed to save changes: " + ex.Message });
                        }
                    } catch (Exception ex) {
                        var innerException = ex.InnerException?.Message;
                        return StatusCode(500, new { error = "ERROR: Failed to upload image: " + innerException });
                    }
                } catch (Exception ex) {
                    return StatusCode(500, new { error = "ERROR: ", ex.Message });
                }
            }
        }

        [HttpPost("redeem-reward")]
        public async Task<IActionResult> RedeemReward([FromForm] string studentID, [FromForm] string rewardID) {
            if (string.IsNullOrEmpty(studentID) || string.IsNullOrEmpty(rewardID)) {
                return BadRequest(new { error = "UERROR: Student ID and Reward ID are required" });
            } else {
                var student = _context.Students.FirstOrDefault(s => s.StudentID == studentID);
                if (student == null) {
                    return NotFound(new { error = "ERROR: Student not found" });
                }

                var reward = _context.RewardItems.FirstOrDefault(r => r.RewardID == rewardID);
                if (reward == null) {
                    return NotFound(new { error = "ERROR: Reward not found" });
                }

                if (student.CurrentPoints < reward.RequiredPoints) {
                    return BadRequest(new { error = "UERROR: Insufficient leafs to redeem reward" });
                }

                try {
                    var redemption = new Redemption {
                        RedemptionID = Utilities.GenerateUniqueID(),
                        RedeemedOn = DateTime.Now,
                        ClaimedOn = null,
                        RedemptionStatus = "Pending",
                        RewardID = reward.RewardID,
                        StudentID = student.StudentID
                    };

                    _context.Redemptions.Add(redemption);
                    student.CurrentPoints -= reward.RequiredPoints;
                    _context.SaveChanges();

                    var studentName = _context.Users.FirstOrDefault(u => u.Id == student.StudentID)?.Name ?? "Student";
                    var studentEmail = _context.Users.FirstOrDefault(u => u.Id == student.StudentID)?.Email ?? "student@mymail.nyp.edu.sg";

                    var frontendUrl = Environment.GetEnvironmentVariable("VITE_FRONTEND_URL");
                    if (string.IsNullOrEmpty(frontendUrl)) {
                        return StatusCode(500, new { error = "ERROR: Environment not ready to generate QR Code for redemption" });
                    }
                    string apiPath = $"/student/claimReward?studentID={student.StudentID}&redemptionID={redemption.RedemptionID}";
                    string fullUrl = $"{frontendUrl}{apiPath}";

                    string qrCodeUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=150x150&data={Uri.EscapeDataString(fullUrl)}";

                    var emailVars = new Dictionary<string, string> {
                        { "username", studentName },
                        { "qrcode", qrCodeUrl }
                    };

                    await Emailer.SendEmailAsync(studentEmail, "Your reward is here!", "RewardRedemption", emailVars);
                    return Ok(new { message = "SUCCESS: Reward redeemed successfully", data = student.CurrentPoints });
                } catch (Exception ex) {
                    return StatusCode(500, new { error = ex.Message });
                }
            }
        }

        [HttpGet("claim-reward")]
        public async Task<IActionResult> ClaimReward([FromQuery] string studentID, [FromQuery] string redemptionID) {
            if (string.IsNullOrEmpty(studentID) || string.IsNullOrEmpty(redemptionID)) {
                return BadRequest(new { error = "UERROR: StudentID and RedemptionID are required" });
            } else {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentID == studentID);
                if (student == null) {
                    return NotFound(new { error = "ERROR: Student not found" });
                }

                var redemption = await _context.Redemptions.FirstOrDefaultAsync(r => r.RedemptionID == redemptionID);
                if (redemption == null) {
                    return NotFound(new { error = "ERROR: Redemption not found" });
                }

                var reward = await _context.RewardItems.FirstOrDefaultAsync(r => r.RewardID == redemption.RewardID);
                if (reward == null) {
                    return NotFound(new { error = "ERROR: Reward not found" });
                }

                if (redemption.RedemptionStatus == "Claimed") {
                    return BadRequest(new { error = "UERROR: Reward has already been claimed" });
                }

                try {
                    redemption.ClaimedOn = DateTime.Now;
                    redemption.RedemptionStatus = "Claimed";
                    _context.SaveChanges();

                    return Ok(new { message = "SUCCESS: Reward claimed successfully" });
                } catch (Exception ex) {
                    return StatusCode(500, new { error = "ERROR: ", ex.Message });
                }
            }
        }

        [HttpPost("award-gift")]
        public IActionResult AwardGift([FromBody] string studentID) {
            if (string.IsNullOrEmpty(studentID)) {
                return BadRequest(new { error = "UERROR: Student ID is required" });
            } else {
                var student = _context.Students.FirstOrDefault(s => s.StudentID == studentID);
                if (student == null) {
                    return NotFound(new { error = "ERROR: Student not found" });
                }

                var randomPoints = Utilities.GenerateRandomInt(10, 100);

                try {
                    student.CurrentPoints += randomPoints;
                    student.TotalPoints += randomPoints;
                    student.LastClaimedStreak = DateTime.Now.ToString("yyyy-MM-dd");
                    _context.SaveChanges();
                    return Ok(new { message = "SUCCESS: Gift awarded successfully", data = new { pointsAwarded = randomPoints, currentPoints = student.CurrentPoints } });
                } catch (Exception ex) {
                    return StatusCode(500, new { error = ex.Message });
                }
            }
        }
            
        [HttpPost("recognise-image")]
        public async Task<IActionResult> RecogniseImage([FromForm] IFormFile file) {
            if (file == null || file.Length == 0) {
                return BadRequest(new { error = "UERROR: No file uploaded" });
            } else {
                try {
                    var recognitionResult = await CompVision.Recognise(file);
                    return Ok(recognitionResult);
                } catch (Exception ex) {
                    return StatusCode(500, new { error = ex });
                }
            }
        }
    }
}