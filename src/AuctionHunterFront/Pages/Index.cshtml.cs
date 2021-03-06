﻿using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using AuctionHunterFront.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Identity;
using AuctionHunterFront.Extensions;
using System;

namespace AuctionHunterFront.Pages
{
	[Authorize]
	public class IndexModel : PageModel
	{
		private readonly AuctionHunterDbContext _auctionHunterDbContext;
		private readonly UserManager<ApplicationUser> _userManager;

		public IList<AuctionHunterItem> AuctionHunterItems { get; set; }
		public bool HasAuctionHunterItems => AuctionHunterItems?.Count > 0;
		public string ItemIds => string.Join(",", AuctionHunterItems.Select(e => e.Id));

		public int ItemsPerPage { get; set; } = 12;

		public int ItemCount { get; set; }

		[BindProperty(SupportsGet = true)]
		public int? PageNumber { get; set; }

		[BindProperty(SupportsGet = true)]
		public bool ShowAll { get; set; }

		public IndexModel(AuctionHunterDbContext auctionHunterDbContext, UserManager<ApplicationUser> userManager)
		{
			_auctionHunterDbContext = auctionHunterDbContext;
			_userManager = userManager;
		}

		public async Task OnGetAsync()
		{
			var currentUser = await _userManager.GetUserAsync(HttpContext.User);
			PageNumber = PageNumber ?? 1;

			if (ShowAll)
			{
				ItemCount = await _auctionHunterDbContext.AuctionHunterItems
					.CountAsync();

				AuctionHunterItems = await _auctionHunterDbContext.AuctionHunterItems
					.Skip(ItemsPerPage * ((int)PageNumber - 1))
					.Take(ItemsPerPage)
					.ToListAsync();
			}
			else
			{
				ItemCount = await _auctionHunterDbContext.ApplicationUserAuctionHunterItems
					.Where(e => e.ApplicationUserId == currentUser.Id)
					.CountAsync();

				AuctionHunterItems = await _auctionHunterDbContext.ApplicationUserAuctionHunterItems
					.Where(e => e.ApplicationUserId == currentUser.Id)
					.Select(e => e.AuctionHunterItem)
					.Skip(ItemsPerPage * ((int)PageNumber - 1))
					.Take(ItemsPerPage)
					.ToListAsync();
			}
		}

		[ValidateAntiForgeryToken]
		public async Task<IActionResult> OnPostMarkAsReadAsync(int id, bool showAll)
		{
			await MarkAsReadAsync(id);

			return RedirectToPage(new { showAll });
		}

		[ValidateAntiForgeryToken]
		public async Task<IActionResult> OnPostMarkAllAsReadAsync(string itemIds, bool showAll)
		{
			foreach (var id in itemIds.Split(',', System.StringSplitOptions.RemoveEmptyEntries))
			{
				await MarkAsReadAsync(int.Parse(id));
			}

			var pageNumber = 1;
			return RedirectToPage(new { pageNumber, showAll });
		}

		public JToken JsonParse(string json)
		{
			return JObject.Parse(json);
		}

		public string GetReviewLink(string contentJson)
		{
			var name = JToken.Parse(contentJson).SelectToken("$.name").ToString();
			name = SmartStrip(name);
			var urlEncodedName = WebUtility.UrlEncode(name);
			return $"https://www.google.pl/search?tbm=vid&q={urlEncodedName}+review";
		}

		private async Task MarkAsReadAsync(int id)
		{
			var currentUser = await _userManager.GetUserAsync(HttpContext.User);
			var item = await _auctionHunterDbContext.ApplicationUserAuctionHunterItems
				.Where(e => e.ApplicationUserId == currentUser.Id && e.AuctionHunterItem.Id == id)
				.FirstOrDefaultAsync();

			if (item != null)
			{
				_auctionHunterDbContext.ApplicationUserAuctionHunterItems.Remove(item);
				await _auctionHunterDbContext.SafeSaveChangesAsync();
			}
		}

		private string SmartStrip(string name)
		{
			var stipList = new List<string>
			{
				" PC",
				" (PC)",
				" + DLC",
				" Steam Key",
				" Steam Gift",
				" NORTH",
				" SOUTH",
				" EASTERN",
				" EAST",
				" WESTERN",
				" WEST",
				" GLOBAL",
				" ASIA",
				" AMERICA",
				" EUROPE",
				" ROW",
			};

			foreach (var strip in stipList)
			{
				name = name.Replace(strip, string.Empty);
			}

			return name;
		}
	}
}
