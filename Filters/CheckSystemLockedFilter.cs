using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Backend.Filters {
    public class CheckSystemLockedFilter : IAsyncActionFilter {
        private readonly MyDbContext _context;

        public CheckSystemLockedFilter(MyDbContext context) {
            _context = context;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
            var systemLocked = await _context.EnvironmentConfigs.FirstOrDefaultAsync(e => e.Name == "SYSTEM_LOCKED") ?? throw new InvalidOperationException("ERROR: SYSTEM_LOCKED configuration is missing.");

            if (systemLocked.Value == "true") {
                context.Result = new ObjectResult(new { error = "ERROR: System is locked. Please try again later." }) {
                    StatusCode = 503
                };
                return;
            }

            await next();
        }
    }
}