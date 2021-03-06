﻿using System;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AuctionHunterFront.Services.Implementation
{
	public class EmailSender : IEmailSender
	{
		public async Task SendEmailAsync(string email, string subject, string message)
		{
			var apiKey = Environment.GetEnvironmentVariable("SENDGRID_APIKEY");
			var client = new SendGridClient(apiKey);
			var msg = new SendGridMessage
			{
				From = new EmailAddress("no-reply@auctionhunter.azurewebsites.net", "AuctionHunter"),
				Subject = subject,
				HtmlContent = message
			};
			msg.AddTo(new EmailAddress(email));
			var response = await client.SendEmailAsync(msg);
		}

		public async Task SendEmailConfirmationAsync(string email, string callbackUrl)
		{
			var message = $"<h4>Email confirmation link: </h4><p><a href=\"{callbackUrl}\">Link</a></p>";
			await SendEmailAsync(email, "Email confirmation", message);
		}

		public async Task SendResetPasswordAsync(string email, string callbackUrl)
		{
			var message = $"<h4>Password reset link: </h4><p><a href=\"{callbackUrl}\">Link</a></p>";
			await SendEmailAsync(email, "Password reset", message);
		}
	}
}
