﻿using AuctionHunter.Extensions;
using HtmlAgilityPack;
using System.Linq;

namespace AuctionHunter.CdKeys.Implementation
{
	public class CdKeysAuctionLinkExtractor : ICdKeysAuctionLinkExtractor
	{
		public string Extract(string item)
		{
			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(item);

			var htmlNodeCollection = htmlDocument.DocumentNode.SafeSelectNodes("//a/@href");

			return htmlNodeCollection?.FirstOrDefault()?.Attributes["href"]?.Value;
		}
	}
}
