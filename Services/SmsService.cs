using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services {
    public class SmsService {
        private readonly MyDbContext _context;
        public SmsService(MyDbContext context) {
            _context = context;
        }

        private static readonly string? _accountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
        private static readonly string? _authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
        private static readonly string? _fromNumber = Environment.GetEnvironmentVariable("TWILIO_REGISTERED_PHONE_NUMBER");

        private async Task CheckPermissionAsync() {
            var smsEnabled = await _context.EnvironmentConfigs.FirstOrDefaultAsync(e => e.Name == "SMS_ENABLED");
            if (smsEnabled == null) {
                throw new InvalidOperationException("ERROR: SMS_ENABLED configuration is missing.");
            }
            if (smsEnabled.Value == "false") {
                throw new Exception("ERROR: SMS Dispatcher Service is Disabled.");
            }
        }

        public async Task<string> SendSmsAsync(string toNumber, string message) {
            await CheckPermissionAsync();

            try {
                TwilioClient.Init(_accountSid, _authToken);

                var messageResponse = await MessageResource.CreateAsync(
                    body: message,
                    from: new Twilio.Types.PhoneNumber(_fromNumber),
                    to: new Twilio.Types.PhoneNumber(toNumber)
                );

                return $"SUCCESS: Message dispatched with SID: {messageResponse.Sid}";
            } catch (ApiException ex) {
                return $"ERROR: {ex.Message}";
            }
        }
    }
}