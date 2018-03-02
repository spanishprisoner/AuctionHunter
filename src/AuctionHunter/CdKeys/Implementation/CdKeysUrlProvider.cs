﻿namespace AuctionHunter.CdKeys.Implementation
{
	public class CdKeysUrlProvider : ICdKeysUrlProvider
	{
		public string BaseUrl { get; set; }

		public string GetUrlForPage(int pageNumber)
		{
			return BaseUrl.Insert(BaseUrl.Length, pageNumber.ToString());
		}
	}
}
