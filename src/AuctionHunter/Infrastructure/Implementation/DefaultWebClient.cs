﻿using AuctionHunter.Results;
using System;
using System.Threading.Tasks;
using AuctionHunter.Controls;
using AuctionHunter.Extensions;
using System.Threading;
using System.Collections.Specialized;
using System.Text;

namespace AuctionHunter.Infrastructure.Implementation
{
	public class DefaultWebClient : IWebClient
	{
		private readonly CookieAwareWebClient _webClient;
		private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);

		public DefaultWebClient()
		{
			_webClient = new CookieAwareWebClient();
		}

		public async Task<WebClientResult> Get(string url, int retryCount = 5)
		{
			Task<string> makeRequest()
			{
				_webClient.Headers["User-Agent"] =
					"Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:58.0) Gecko/20100101 Firefox/58.0";
				var result = _webClient.DownloadString(new Uri(url));

				return Task.FromResult(result);
			}

			using (await _semaphoreSlim.DisposableWaitAsync(TimeSpan.FromMinutes(10)))
			{
				return await RetryableRequest(makeRequest, retryCount);
			}
		}

		public async Task<WebClientResult> Post(string url, NameValueCollection values, int retryCount = 5)
		{
			Task<string> makeRequest()
			{
				_webClient.Headers["User-Agent"] =
					"Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:58.0) Gecko/20100101 Firefox/58.0";
				_webClient.Headers["Content-Type"] = "application/x-www-form-urlencoded";
				var result = _webClient.UploadValues(url, "POST", values);

				return Task.FromResult(Encoding.UTF8.GetString(result));
			}

			using (await _semaphoreSlim.DisposableWaitAsync(TimeSpan.FromMinutes(10)))
			{
				return await RetryableRequest(makeRequest, retryCount);
			}
		}

		private async Task<WebClientResult> RetryableRequest(Func<Task<string>> makeRequest, int retryCount)
		{
			var webClientResult = new WebClientResult();
			for (var attempt = 0; attempt < retryCount; attempt++)
			{
				try
				{
					if (attempt > 0)
					{
						webClientResult.DebugInfo.AppendLine("retrying...");
					}
					webClientResult.Content = await makeRequest();
					webClientResult.DebugInfo.AppendLine("Complete");
					webClientResult.Success = true;
					return webClientResult;
				}
				catch (Exception e)
				{
					webClientResult.DebugInfo.AppendLine($"{e.Message}");
				}

				await Task.Delay(1000);
			}

			webClientResult.DebugInfo.AppendLine("Failed");
			return webClientResult;
		}
	}
}
