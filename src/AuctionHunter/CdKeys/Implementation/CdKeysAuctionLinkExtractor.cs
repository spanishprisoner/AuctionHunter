﻿using AuctionHunter.Infrastructure;
using Newtonsoft.Json.Linq;

namespace AuctionHunter.CdKeys.Implementation
{
	public class CdKeysAuctionLinkExtractor : IAuctionLinkExtractor
	{
		public string Extract(string item)
		{
			var token = JObject.Parse(item);
			var slug = token.SelectToken("$.slug").ToString();
			return $"https://www.g2a.com/pl-pl/{slug}";
		}
	}
}
