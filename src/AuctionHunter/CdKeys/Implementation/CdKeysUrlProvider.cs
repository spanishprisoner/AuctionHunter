﻿using AuctionHunter.Infrastructure;

namespace AuctionHunter.CdKeys.Implementation
{
	public class CdKeysUrlProvider : IUrlProvider
	{
		public string BaseUrl { get; set; }

		public string GetUrlForPage(int pageNumber)
		{
			return BaseUrl.Replace("page=", $"page={pageNumber}");
		}
	}
}
