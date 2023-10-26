using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Asm03_17_VuDucHuy.Models;
using Microsoft.AspNetCore.SignalR;
using Asm03_17_VuDucHuy.Utils;
using System.Configuration;
using Microsoft.Extensions.Hosting;
using System.Xml.Serialization;

namespace Asm03_17_VuDucHuy.Controllers
{
	public class PostDto
	{
		public PaginatedList<PostsDto> posts { get; set; }

		public int totalPage { get; set; }
	}

	public class PostsDto
	{
		public int PostId {get; set;}
		public DateTime? CreatedDate {get; set;}
		public DateTime? UpdateDate {get; set;}
		public string Title {get; set;}
		public string Content {get; set;}
		public bool? PublishStatus {get; set;}
		public string AuthorName  {get; set;}
		public string CategoryName { get; set; }	
	}


	public class PostsController : Controller
	{
		private readonly Assignment03PRN221Context _context;
		private readonly IHubContext<PostSignalR> _hubContext;


		public PostsController(Assignment03PRN221Context context, IHubContext<PostSignalR> hubContext)
		{
			_context = context;
			_hubContext = hubContext;
		}


		//GetPosts
		// Assuming you are in a controller class
		[HttpGet]
		public IActionResult GetPosts()
		{
			// Use asynchronous database access
			var posts = _context.Posts.Include(p => p.Author).Include(p => p.Category).ToList();

			var result = posts.Select(p => new
			{
				p.PostId,
				p.AuthorId,
				p.CreatedDate,
				p.UpdateDate,
				p.Title,
				p.Content,
				p.PublishStatus,
				p.CategoryId,
				AuthorName = p.Author.FullName,
				CategoryName = p.Category.CategoryName
			});

			return Ok(result);
		}


		[HttpPost]
		public async Task<IActionResult> SearchPosts(string searchString, int? pageIndex, int pageTotal, string searchBy, DateTime startDate, DateTime endDate)
		{

			PaginatedList<PostsDto> Posts;
			int pageSize = 2;
			if (string.IsNullOrEmpty(searchString))
			{
				var pIq = _context.Posts
					.Include(p => p.Author)
					.Include(p => p.Category)
					.Where(p => p.CreatedDate >= startDate && p.CreatedDate <= endDate);
				IQueryable<PostsDto> result = pIq.Select(p => new PostsDto
				{
					PostId = p.PostId,
					CreatedDate = p.CreatedDate,
					UpdateDate = p.UpdateDate,
					Title = p.Title,
					Content = p.Content,
					PublishStatus = p.PublishStatus,
					AuthorName = p.Author.FullName,
					CategoryName = p.Category.CategoryName
				});
				var check = result.Count();
				Posts = await PaginatedList<PostsDto>.CreateAsync(result, pageIndex ?? 1, pageSize);
				PostDto postDtos = new PostDto();
				postDtos.posts = Posts;
				postDtos.totalPage = Posts.TotalPage;
				return Ok(postDtos);
			}
			IQueryable<Post> postBy;
			IQueryable<PostsDto> postsDtoBy;

			switch (searchBy)
			{
				case "ID":
					postBy = _context.Posts.Include(p => p.Author)
					.Include(p => p.Category).Where(p => p.PostId.ToString().Equals(searchString) && p.CreatedDate >= startDate && p.CreatedDate <= endDate);
					break;
				case "Title":
					postBy = _context.Posts.Include(p => p.Author).Include(p => p.Category).Where(p=>p.Title.Contains(searchString) && p.CreatedDate >= startDate && p.CreatedDate <= endDate);
					break;
				case "Description" :
					postBy = _context.Posts.Include(p => p.Author).Include(p => p.Category).Where(p => p.Content.Contains(searchString) && p.CreatedDate >= startDate && p.CreatedDate <= endDate);
					break;
				default:
					postBy = _context.Posts;
					break;
			}

			postsDtoBy = postBy.Select(p => new PostsDto
			{
				PostId = p.PostId,
				CreatedDate = p.CreatedDate,
				UpdateDate = p.UpdateDate,
				Title = p.Title,
				Content = p.Content,
				PublishStatus = p.PublishStatus,
				AuthorName = p.Author.FullName,
				CategoryName = p.Category.CategoryName
			});

			//var p = from posts in _context.Posts
			//		where posts.Title.Contains(searchString)
			//		select posts;
			Posts = await PaginatedList<PostsDto>.CreateAsync(postsDtoBy, pageIndex ?? 1, pageSize);
			PostDto postDto = new PostDto();
			postDto.posts = Posts;
			postDto.totalPage = Posts.TotalPage;
			return Ok(postDto);
		}


		private List<String> searchBys = new List<String>() { "ID", "Title", "Description" };

		// GET: Posts
		public async Task<IActionResult> Index()
		{
			ViewData["searchBys"] = searchBys.Select(item => new SelectListItem
			{
				Text = item,
				Value = item
			});
			var assignment03PRN221Context = _context.Posts.Include(p => p.Author).Include(p => p.Category);
			return View(await assignment03PRN221Context.ToListAsync());
		}

		// GET: Posts/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null || _context.Posts == null)
			{
				return NotFound();
			}

			var post = await _context.Posts
				.Include(p => p.Author)
				.Include(p => p.Category)
				.FirstOrDefaultAsync(m => m.PostId == id);
			if (post == null)
			{
				return NotFound();
			}

			return View(post);
		}

		// GET: Posts/Create
		public IActionResult Create()
		{
			ViewData["AuthorId"] = new SelectList(_context.AppUsers, "UserId", "UserId");
			ViewData["CategoryId"] = new SelectList(_context.PostCategories, "CategoryId", "CategoryId");
			return View();
		}

		// POST: Posts/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("PostId,AuthorId,CreatedDate,UpdateDate,Title,Content,PublishStatus,CategoryId")] Post post)
		{
			if (ModelState.IsValid)
			{
				_context.Add(post);
				await _context.SaveChangesAsync();
				await _hubContext.Clients.All.SendAsync("LoadPosts", post);
				return RedirectToAction(nameof(Index));
			}
			ViewData["AuthorId"] = new SelectList(_context.AppUsers, "UserId", "UserId", post.AuthorId);
			ViewData["CategoryId"] = new SelectList(_context.PostCategories, "CategoryId", "CategoryId", post.CategoryId);
			return View(post);
		}

		// GET: Posts/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null || _context.Posts == null)
			{
				return NotFound();
			}

			var post = await _context.Posts.FindAsync(id);
			if (post == null)
			{
				return NotFound();
			}
			ViewData["AuthorId"] = new SelectList(_context.AppUsers, "UserId", "UserId", post.AuthorId);
			ViewData["CategoryId"] = new SelectList(_context.PostCategories, "CategoryId", "CategoryId", post.CategoryId);
			return View(post);
		}

		// POST: Posts/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("PostId,AuthorId,CreatedDate,UpdateDate,Title,Content,PublishStatus,CategoryId")] Post post)
		{
			if (id != post.PostId)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(post);
					await _context.SaveChangesAsync();
					await _hubContext.Clients.All.SendAsync("LoadPosts", post);

				}
				catch (DbUpdateConcurrencyException)
				{
					if (!PostExists(post.PostId))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Index));
			}
			ViewData["AuthorId"] = new SelectList(_context.AppUsers, "UserId", "UserId", post.AuthorId);
			ViewData["CategoryId"] = new SelectList(_context.PostCategories, "CategoryId", "CategoryId", post.CategoryId);
			return View(post);
		}

		// GET: Posts/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null || _context.Posts == null)
			{
				return NotFound();
			}

			var post = await _context.Posts
				.Include(p => p.Author)
				.Include(p => p.Category)
				.FirstOrDefaultAsync(m => m.PostId == id);
			if (post == null)
			{
				return NotFound();
			}

			return View(post);
		}

		// POST: Posts/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int PostId)
		{
			if (_context.Posts == null)
			{
				return Problem("Entity set 'Assignment03PRN221Context.Posts'  is null.");
			}
			var post = await _context.Posts.FindAsync(PostId);
			if (post != null)
			{
				_context.Posts.Remove(post);
			}

			await _context.SaveChangesAsync();
			await _hubContext.Clients.All.SendAsync("LoadPosts", post);

			return RedirectToAction(nameof(Index));
		}

		private bool PostExists(int id)
		{
			return (_context.Posts?.Any(e => e.PostId == id)).GetValueOrDefault();
		}
	}
}
