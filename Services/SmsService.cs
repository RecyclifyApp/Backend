using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Exceptions;

namespace Backend.Services {
    public static class SmsService {
        private static bool ContextChecked = false;
        private static readonly string? _accountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
        private static readonly string? _authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
        private static readonly string? _fromNumber = Environment.GetEnvironmentVariable("TWILIO_REGISTERED_PHONE_NUMBER");

        public static bool CheckPermission() {
            return Environment.GetEnvironmentVariable("SMS_ENABLED") == "True";
        }

        public static void CheckContext() {
            if (CheckPermission()) {
                if (string.IsNullOrEmpty(_accountSid) || string.IsNullOrEmpty(_authToken) || string.IsNullOrEmpty(_fromNumber)) {
                    throw new Exception("ERROR: TWILIO_ACCOUNT_SID, TWILIO_AUTH_TOKEN, or TWILIO_REGISTERED_PHONE_NUMBER environment variables not set.");
                }
            } else {
                throw new Exception("ERROR: SmsService is not enabled.");
            }

            ContextChecked = true;
        }

        public static async Task<string> SendSmsAsync(string toNumber, string message) {
            if (!ContextChecked) {
                return "ERROR: System context was not checked before sending email. Skipping SMS.";
            }

            if (!CheckPermission()) {
                return "ERROR: SMS is not enabled. Skipping SMS.";
            }

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