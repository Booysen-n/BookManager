using BookManager.Data;
using BookManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookManager.Controllers
{
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;
        public BookController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var books = await _context.Books.Include(b => b.Publisher)
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .ToListAsync();
            return View();
        }

        //Details : View details of a specific book
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var book = await _context.Books.Include(b => b.Publisher)
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }
        //Show the form to create a new book
        public IActionResult Create()
        {
            ViewBag.PublisherId = new SelectList(_context.Publishers, "Id", "Name");
            ViewBag.Authors = new MultiSelectList(_context.Authors, "Id", "Name");
            return View();
        }
        //Save the new book to the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book, int[] selectedAuthors)
        {
            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();
                foreach (var authorId in selectedAuthors)
                {
                    var bookAuthor = new BookAuthor { BookId = book.Id, AuthorId = authorId };
                    _context.Add(bookAuthor);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.PublisherId = new SelectList(_context.Publishers, "Id", "Name");
            ViewBag.Authors = new MultiSelectList(_context.Authors, "Id", "Name");
            return View(book);
        }
    }
}
