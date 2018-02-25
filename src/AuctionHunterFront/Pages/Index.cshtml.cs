﻿using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using AuctionHunterFront.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace AuctionHunterFront.Pages
{
	[Authorize]
	public class IndexModel : PageModel
	{
		private readonly AuctionHunterDbContext _auctionHunterDbContext;

		public IList<AuctionHunterItem> AuctionHunterItems { get; set; }
		public bool HasAuctionHunterItems => AuctionHunterItems?.Count > 0;

		public int ItemsPerPage { get; set; } = 12;

		public int ItemCount { get; set; }

		[BindProperty(SupportsGet = true)]
		public int? PageNumber { get; set; }

		[BindProperty(SupportsGet = true)]
		public bool ShowAll { get; set; }

		public IndexModel(AuctionHunterDbContext auctionHunterDbContext)
		{
			_auctionHunterDbContext = auctionHunterDbContext;
		}

		public async Task OnGetAsync()
		{
			PageNumber = PageNumber ?? 1;
			ItemCount = await _auctionHunterDbContext.AuctionHunterItems
				.Where(e => ShowAll || e.MarkedAsRead == false)
				.CountAsync();
			AuctionHunterItems = await _auctionHunterDbContext.AuctionHunterItems
				.Where(e => ShowAll || e.MarkedAsRead == false)
				.Skip(ItemsPerPage * ((int)PageNumber - 1))
				.Take(ItemsPerPage)
				.ToListAsync();
		}

		public async Task<IActionResult> OnPostMarkAsReadAsync(int id, bool showAll)
		{
			var update = new AuctionHunterItem { Id = id };
			_auctionHunterDbContext.AuctionHunterItems.Attach(update);

			update.MarkedAsRead = true;
			await _auctionHunterDbContext.SaveChangesAsync();

			return RedirectToPage(new { showAll });
		}

		public async Task<IActionResult> OnPostMarkAllAsReadAsync(bool showAll)
		{
			await _auctionHunterDbContext.Database
				.ExecuteSqlCommandAsync("UPDATE AuctionHunterItems SET MarkedAsRead = true");

			return RedirectToPage(new { showAll });
		}

		public JToken JsonParse(string json)
		{
			return JObject.Parse(json);
		}
	}
}