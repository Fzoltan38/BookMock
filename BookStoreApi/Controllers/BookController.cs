using BookStoreApi.DTOs;
using BookStoreApi.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreApi.Controllers
{
    /// <summary>
    /// Books API végpontok - HTTP kérések kezelése.
    /// Az IBookService interfészen keresztül hívja a service réteget,
    /// ezért a service könnyen cserélhető Mock-ra teszteléskor.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        /// <summary>
        /// Összes könyv lekérése
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var books = await _bookService.GetAllBooksASync();
            return Ok(books);
        }

        /// <summary>
        /// Egy könyv lekérése ID alapján
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);

            if (book == null)
            {
                return NotFound(new { message = "Nincs találat!" });
            }

            return Ok(book);
        }

        /// <summary>
        /// Új könyv létrehozása
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(BookCreateDto bookCreateDto)
        {
            var book = await _bookService.CreateBookAsync(bookCreateDto);
            return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] BookUpdateDto bookUpdateDto)
        {
            var book = await _bookService.UpdateBookAsync(id);

        }
    }
}

