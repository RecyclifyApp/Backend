# 📄 Services Documentation
Read usage documentation for the backend services below.

## AssetsManager
1.) UploadFileAsync(file): Accepts 1 `file` parameter of type `IFormFile`. Calling this method will directly upload the provided file to cloud storage.

`var result = await AssetsManager.UploadFileAsync(file);`

2.) DeleteFileAsync(filename): Accepts 1 `filename` parameter of type `string`. Calling this method will remove the file of that filename in cloud storage. If file does not exist, error code `500` is returned.

`var result = await AssetsManager.DeleteFileAsync(filename);`

3.) GetFileUrlAsync(filename): Accepts 1 `filename` parameter of type `string`. Calling this method will return the public URL of the file. If file does not exist, error code `500` is returned.

`var result = await AssetsManager.GetFileUrlAsync(filename);`

## Bootcheck
_This service runs on system boot. No methods available for this service._

**IMPORTANT:** Add all `env` variables to `Bootcheck`.

## CompVision
1.) Recognise(file): Accepts 1 `file` parameter of type `IFormFile`. Calling this method will perform Image Recognition on the file. Invalid image formats will return error code `500`

`var recognitionResult = await CompVision.Recognise(file);`

**IMPORTANT: **`COMPVISION_ENABLED` needs to be set to `True` in the `.env` file for `CompVision` to work.

## Emailer
1.) SendEmailAsync(to, subject, template): Accepts 3 parameters. Calling this method will send an email to a recipient using recyclifySystem's Gmail account.

`to`: This is the email of the recipient

`subject`: This is the subject of the email

`template`: This is the HTML template you are using for this email. If your template filename is `WelcomeEmail.html`, your template parameter just needs to be `WelcomeEmail`.

`var emailResult = await Emailer.SendEmailAsync(recipientEmail, title, template);`

**IMPORTANT:** `EMAILER_ENABLED` needs to be set to `True` in the `.env` file for `Emailer` to work.

## Logger
1.) Log(message): Accepts 1 `message` parameter of type `string`. Calling this method will log a message to `logs.txt` for debugging purposes.

`Logger.Log(message);`

## SmsService
_This service is down at the moment. Docs will be updated after the service is fixed._

## Utilities
This service serves as a class of microservices, and common microservices or tools will be found here. More microservices and tools will be added on to `Utilities` in the future.

1.) GenerateUniqueID(customLength): Accepts an **OPTIONAL** `length` parameter of type `int`. Calling this method will return a standard UUID of default length, unless a `customLength` is given.

`Utilities.GenerateUniqueID(customLength)`

2.) HashString(input): Accepts a `input` parameter of type `string`. Calling this method will hash the given input using the SHA256 algorithm.

`Utilities.HashString(input)`

3.) EncodeToBase64(input): Accepts a `input` parameter of type `string`. Calling this method will encode the given input to Base64 format.

`Utilities.EncodeToBase64(input)`

4.) DecodeFromBase64(input): Accepts a `input` parameter of type `string`. Calling this method will decode the given input from Base64 format.

`Utilities.DecodeFromBase64(input)`

##

Last updated: 5 Jan 2025, 6.38PM

**---End of documentation---**
