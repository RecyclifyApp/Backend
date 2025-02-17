using Backend.Models;
using Backend.Filters;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(CheckSystemLockedFilter))]
    public class studentController(MyDbContext context) : ControllerBase {
        private readonly MyDbContext _context = context;

        [Authorize]
        [HttpGet("get-student")]
        public IActionResult GetStudent([FromQuery] string studentID) {
            if (string.IsNullOrEmpty(studentID)) {
                return BadRequest(new { error = "UERROR: Required parameters missing" });
            } else {
                var matchedStudent = _context.Students.FirstOrDefault(s => s.StudentID == studentID);
                if (matchedStudent == null) {
                    return NotFound(new { error = "ERROR: Student not found" });
                }

                var studentClassRecord = _context.ClassStudents.FirstOrDefault(cs => cs.StudentID == matchedStudent.StudentID);
                if (studentClassRecord != null) {
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
                }

                return Ok(new { message = "SUCCESS: Student details retrieved", data = matchedStudent });
            }
        }

        [Authorize]
        [HttpGet("get-student-classID")]
        public IActionResult GetStudentClassID([FromQuery] string studentID) {
            if (string.IsNullOrEmpty(studentID)) {
                return BadRequest(new { error = "UERROR: Required parameters missing" });
            } else {
                var matchedStudent = _context.Students.FirstOrDefault(s => s.StudentID == studentID);
                if (matchedStudent == null) {
                    return NotFound(new { error = "ERROR: Student not found" });
                }

                var studentClassRecord = _context.ClassStudents.FirstOrDefault(cs => cs.StudentID == matchedStudent.StudentID);
                if (studentClassRecord == null) {
                    return NotFound(new { error = "ERROR: Class not found. Please Join a Class" });
                }

                return Ok(new { message = "SUCCESS: Student classID retrieved", data = studentClassRecord.ClassID });
            }
        }   

        [Authorize]
        [HttpGet("get-student-leafs")]
        public IActionResult GetStudentLeafs([FromQuery] string studentID) {
            if (string.IsNullOrEmpty(studentID)) {
                return BadRequest(new { error = "UERROR: Required parameters missing" });
            } else {
                var matchedStudent = _context.Students.FirstOrDefault(s => s.StudentID == studentID);
                if (matchedStudent == null) {
                    return NotFound(new { error = "ERROR: Student not found" });
                }

                return Ok(new { message = "SUCCESS: Student leafs retrieved", data = matchedStudent.CurrentPoints });
            }
        }

        [Authorize]
        [HttpGet("get-all-students")]
        public async Task<IActionResult> GetAllStudents([FromQuery] string studentID) {
            if (string.IsNullOrEmpty(studentID)) {
                return BadRequest(new { error = "UERROR: Required parameters missing" });
            }

            var studentClass = await _context.ClassStudents.FirstOrDefaultAsync(cs => cs.StudentID == studentID);

            if (studentClass == null) {
                return NotFound(new { error = "ERROR: Class not found. Please Join a Class" });
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

        [Authorize]
        [HttpGet("get-student-tasks")]
        public async Task<IActionResult> GetStudentTasks([FromQuery] string studentID) {
            if (string.IsNullOrEmpty(studentID)) {
                return BadRequest(new { error = "UERROR: Required parameters missing" });
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
                                return NotFound(new { error = "ERROR: Class not found. Please Join a Class" });
                            }

                            var studentClass = _context.Classes.FirstOrDefault(c => c.ClassID == studentClassRecord.ClassID);
                            if (studentClass == null) {
                                return NotFound(new { error = "ERROR: Class not found. Please Join a Class" });
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
                                TaskRejected = false,
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

        [Authorize]
        [HttpGet("get-student-chart-statistics")]
        public IActionResult GetStudentChartStatistics([FromQuery] string studentID) {
            if (string.IsNullOrEmpty(studentID)) {
                return BadRequest(new { error = "UERROR: Required parameters missing" });
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

        [Authorize]
        [HttpGet("get-class-students")]
        public async Task<IActionResult> GetStudents([FromQuery] string studentID) {
            if (string.IsNullOrEmpty(studentID)) {
                return BadRequest(new { error = "UERROR: Required parameters missing" });
            }

            var studentClass = await _context.ClassStudents.FirstOrDefaultAsync(cs => cs.StudentID == studentID);
            if (studentClass == null) {
                return NotFound(new { error = "ERROR: Class not found. Please Join a Class" });
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
                    Name = cs.Student.User!.Name,
                    Email = cs.Student.User!.Email
                }).ToListAsync();

            return Ok(new { message = "SUCCESS: All students retrieved", data = allStudents });
        }

        [Authorize]
        [HttpGet("get-all-rewards")]
        public async Task<IActionResult> GetAllRewards() {
            var allRewards = await _context.RewardItems.Where(r => r.IsAvailable == true).ToListAsync();
            return Ok(new { message = "SUCCESS: All rewards retrieved", data = allRewards });
        }

        [Authorize]
        [HttpPost("submit-task")]
        public async Task<IActionResult> SubmitTask([FromForm] IFormFile file, [FromForm] string taskID, [FromForm] string studentID) {
            if (file == null || file.Length == 0) {
                return BadRequest(new { error = "UERROR: No file uploaded" });
            } 

            if (string.IsNullOrEmpty(taskID) || string.IsNullOrEmpty(studentID)) {
                return BadRequest(new { error = "UERROR: Required parameters missing" });
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
                        return NotFound(new { error = "ERROR: Class not found. Please Join a Class" });
                    }
                    var studentClass = _context.Classes.FirstOrDefault(c => c.ClassID == studentClassRecord.ClassID);
                    if (studentClass == null) {
                        return NotFound(new { error = "ERROR: Class not found. Please Join a Class" });
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

                    if (selectedTaskProgress.DateAssigned != DateTime.Now.ToString("yyyy-MM-dd")) {
                        return BadRequest(new { error = "UERROR: Task expired. Please refresh the page" });
                    }

                    try {
                        await AssetsManager.UploadFileAsync(file);
                        selectedTaskProgress.ImageUrls = (await AssetsManager.GetFileUrlAsync(file.FileName)).Substring("SUCCESS: ".Length).Trim();
                        selectedTaskProgress.TaskRejected = false;
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

        [Authorize]
        [HttpPost("redeem-reward")]
        public async Task<IActionResult> RedeemReward([FromForm] string studentID, [FromForm] string rewardID) {
            if (string.IsNullOrEmpty(studentID) || string.IsNullOrEmpty(rewardID)) {
                return BadRequest(new { error = "UERROR: Required parameters missing" });
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
                    await _context.SaveChangesAsync();

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

                    var Emailer = new Emailer(_context);
                    await Emailer.SendEmailAsync(studentEmail, "Your reward is here!", "RewardRedemption", emailVars);

                    var studentInboxMessage = new Inbox {
                        UserID = student.StudentID,
                        Message = $"You have redeemed {reward.RewardTitle}. Please check your email for the attached QR Code or claim it in My Rewards.",
                        Date = DateTime.Now.ToString("yyyy-MM-dd")
                    };

                    _context.Inboxes.Add(studentInboxMessage);
                    await _context.SaveChangesAsync();

                    return Ok(new { message = "SUCCESS: Reward redeemed successfully", data = student.CurrentPoints });
                } catch (Exception ex) {
                    return StatusCode(500, new { error = ex.Message });
                }
            }
        }

        [Authorize]
        [HttpGet("get-student-rewards")]
        public async Task<IActionResult> GetStudentRewards([FromQuery] string studentID) {
            if (string.IsNullOrEmpty(studentID)) {
                return BadRequest(new { error = "UERROR: Required parameters missing" });
            } else {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentID == studentID);
                if (student == null) {
                    return NotFound(new { error = "ERROR: Student not found" });
                }

                var studentRedemptions = await _context.Redemptions.Where(r => r.StudentID == studentID).ToListAsync();
                var studentRewards = new List<dynamic>();
                foreach (var redemption in studentRedemptions) {
                    var reward = await _context.RewardItems.FirstOrDefaultAsync(r => r.RewardID == redemption.RewardID);
                    if (reward != null) {
                        studentRewards.Add(new {
                            RedemptionStatus = redemption.RedemptionStatus,
                            RewardID = reward.RewardID,
                            RewardTitle = reward.RewardTitle,
                            RewardDescription = reward.RewardDescription,
                            RequiredPoints = reward.RequiredPoints,
                            ImageUrl = reward.ImageUrl,
                            ClaimedOn = redemption.ClaimedOn
                        });
                    }
                }

                return Ok(new { message = "SUCCESS: Student rewards retrieved", data = studentRewards });
            }
        }

        [HttpGet("claim-reward")]
        public async Task<IActionResult> ClaimReward([FromQuery] string studentID, [FromQuery] string redemptionID) {
            if (string.IsNullOrEmpty(studentID) || string.IsNullOrEmpty(redemptionID)) {
                return BadRequest(new { error = "UERROR: Required parameters missing" });
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
                    await _context.SaveChangesAsync();

                    var studentInboxMessage = new Inbox {
                        UserID = student.StudentID,
                        Message = $"You have claimed {reward.RewardTitle}. Enjoy!",
                        Date = DateTime.Now.ToString("yyyy-MM-dd")
                    };

                    _context.Inboxes.Add(studentInboxMessage);
                    await _context.SaveChangesAsync();

                    return Ok(new { message = "SUCCESS: Reward claimed successfully" });
                } catch (Exception ex) {
                    return StatusCode(500, new { error = "ERROR: ", ex.Message });
                }
            }
        }

        [Authorize]
        [HttpPost("award-gift")]
        public async Task<IActionResult> AwardGift([FromBody] string studentID) {
            if (string.IsNullOrEmpty(studentID)) {
                return BadRequest(new { error = "UERROR: Required parameters missing" });
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
                    await _context.SaveChangesAsync();

                    var studentInboxMessage = new Inbox {
                        UserID = student.StudentID,
                        Message = $"You have received bonus {randomPoints} leafs. Keep up your streak!",
                        Date = DateTime.Now.ToString("yyyy-MM-dd")
                    };

                    _context.Inboxes.Add(studentInboxMessage);
                    await _context.SaveChangesAsync();

                    return Ok(new { message = "SUCCESS: Gift awarded successfully", data = new { pointsAwarded = randomPoints, currentPoints = student.CurrentPoints } });
                } catch (Exception ex) {
                    return StatusCode(500, new { error = ex.Message });
                }
            }
        }

        [Authorize]
        [HttpPost("join-class")]
        public async Task<IActionResult> JoinClass([FromForm] string studentID, [FromForm] int joinCode) {
            if (string.IsNullOrEmpty(studentID) || joinCode <= 0) {
                return BadRequest(new { error = "UERROR: Required parameters missing" });
            } else {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentID == studentID);
                if (student == null) {
                    return NotFound(new { error = "ERROR: Student not found" });
                }

                var studentClass = await _context.Classes.FirstOrDefaultAsync(c => c.JoinCode == joinCode);
                if (studentClass == null) {
                    return BadRequest(new { error = "ERROR: Invalid Class Join Code" });
                }

                var existingStudent = await _context.ClassStudents.FirstOrDefaultAsync(cs => cs.StudentID == studentID);
                if (existingStudent != null) {
                    return BadRequest(new { error = "UERROR: You're already enrolled into another class" });
                }

                var existingClassStudent = await _context.ClassStudents.FirstOrDefaultAsync(cs => cs.StudentID == studentID && cs.ClassID == studentClass.ClassID);
                if (existingClassStudent != null) {
                    return BadRequest(new { error = "UERROR: You're already enrolled into this class" });
                }

                try {
                    var newClassStudent = new ClassStudents {
                        ClassID = studentClass.ClassID,
                        StudentID = studentID
                    };

                    _context.ClassStudents.Add(newClassStudent);
                    _context.SaveChanges();

                    return Ok(new { message = "SUCCESS: Student joined class successfully" });
                } catch (Exception ex) {
                    return StatusCode(500, new { error = "ERROR: ", ex.Message });
                }
            }
        }

        [Authorize]
        [HttpGet("check-student-enrolment")]
        public async Task<IActionResult> CheckStudentEnrolment([FromQuery] string studentID) {
            if (string.IsNullOrEmpty(studentID)) {
                return BadRequest(new { error = "UERROR: Required parameters missing" });
            } else {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentID == studentID);
                if (student == null) {
                    return NotFound(new { error = "ERROR: Student not found" });
                }

                var studentClass = await _context.ClassStudents.FirstOrDefaultAsync(cs => cs.StudentID == studentID);
                if (studentClass == null) {
                    return Ok(new { message = "SUCCESS: Student is not enrolled into any class" });
                } else {
                    return BadRequest(new { error = "UERROR: You're already enrolled into a class" });
                }
            }
        }

        [Authorize]
        [HttpGet("get-class-quests")]
        public async Task<IActionResult> GetClassQuests([FromQuery] string classID) {
            if (string.IsNullOrEmpty(classID)) {
                return BadRequest(new { error = "UERROR: Required parameters missing" });
            } else {
               var matchedClass = _context.Classes.FirstOrDefault(c => c.ClassID == classID);
                if (matchedClass == null) {
                    return NotFound(new { error = "ERROR: Class not found" });
                } else {
                    var classQuestProgresses = _context.QuestProgresses.Where(qp => qp.ClassID == matchedClass.ClassID).ToList();
                    var invalidQuestProgresses = classQuestProgresses.Where(qp => DateTime.Parse(qp.DateAssigned) < DateTime.Now.AddDays(-7)).ToList();
                    var validQuestProgresses = classQuestProgresses.Where(qp => DateTime.Parse(qp.DateAssigned) >= DateTime.Now.AddDays(-7)).ToList();

                    foreach (var questProgress in invalidQuestProgresses) {
                        _context.QuestProgresses.Remove(questProgress);
                        await _context.SaveChangesAsync();
                    }

                    var questList = new List<dynamic>();
                    
                    if (validQuestProgresses.Count == 0 || validQuestProgresses.Count != 3) {
                        var numberOfQuestsToRegenerate = 3 - validQuestProgresses.Count;
                        var reccomendResponse = await ReccommendationsManager.RecommendQuestsAsync(_context, classID, numberOfQuestsToRegenerate);

                        foreach (var quest in validQuestProgresses) {
                            var foundQuest = _context.Quests.FirstOrDefault(q => q.QuestID == quest.QuestID);
                            if (foundQuest != null) {
                                questList.Add(foundQuest);
                            }
                        }

                        if (reccomendResponse != null) {
                            foreach (var quest in reccomendResponse.result) {
                                var assignedTeacher = _context.Teachers.FirstOrDefault(t => t.TeacherID == matchedClass.TeacherID);
                                if (assignedTeacher == null) {
                                    return NotFound(new { error = "ERROR: Class's teacher not found" });
                                }

                                var questProgress = new QuestProgress {
                                    QuestID = quest.QuestID,
                                    ClassID = matchedClass.ClassID,
                                    DateAssigned = DateTime.Now.ToString("yyyy-MM-dd"),
                                    AmountCompleted = 0,
                                    Completed = false,
                                    Quest = quest,
                                    AssignedTeacherID = assignedTeacher.TeacherID,
                                    AssignedTeacher = assignedTeacher
                                };

                                _context.QuestProgresses.Add(questProgress);

                                questList.Add(new {
                                    quest.QuestID,
                                    quest.QuestTitle,
                                    quest.QuestDescription,
                                    quest.QuestPoints,
                                    quest.QuestType,
                                    quest.TotalAmountToComplete,
                                    AmountCompleted = 0
                                });

                                _context.SaveChanges();
                            }
                        }
                    } else {
                        foreach (var quest in validQuestProgresses) {
                            var foundQuest = _context.Quests.FirstOrDefault(q => q.QuestID == quest.QuestID);
                            if (foundQuest != null) {
                                var questProgress = await _context.QuestProgresses.Where(qp => qp.QuestID == foundQuest.QuestID && qp.ClassID == matchedClass.ClassID).ToListAsync();

                                questList.Add(new {
                                    foundQuest.QuestID,
                                    foundQuest.QuestTitle,
                                    foundQuest.QuestDescription,
                                    foundQuest.QuestPoints,
                                    foundQuest.QuestType,
                                    foundQuest.TotalAmountToComplete,
                                    AmountCompleted = questProgress.FirstOrDefault()?.AmountCompleted ?? 0
                                });
                            }
                        }
                    }
                    return Ok(new { message = "SUCCESS: Class Quests retrieved", data = questList });
                }
            }   
        }

        [Authorize]
        [HttpGet("get-student-inbox-messages")]
        public async Task<IActionResult> GetStudentInboxMessages([FromQuery] string studentID) {
            if (string.IsNullOrEmpty(studentID)) {
                return BadRequest(new { error = "UERROR: Required parameters missing" });
            } else {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentID == studentID);
                if (student == null) {
                    return NotFound(new { error = "ERROR: Student not found" });
                }

                var studentMessages = await _context.Inboxes.Where(i => i.UserID == studentID).ToListAsync();
                return Ok(new { message = "SUCCESS: Student inbox messages retrieved", data = studentMessages });
            }
        }
            
        [Authorize]
        [HttpPost("recognise-image")]
        public async Task<IActionResult> RecogniseImage([FromForm] IFormFile file) {
            if (file == null || file.Length == 0) {
                return BadRequest(new { error = "UERROR: No file uploaded" });
            } else {
                try {
                    var CompVision = new CompVision(_context);
                    var recognitionResult = await CompVision.Recognise(file);
                    return Ok(recognitionResult);
                } catch (Exception ex) {
                    return StatusCode(500, new { error = ex.Message });
                }
            }
        }
    }
}