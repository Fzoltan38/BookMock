using BookStoreApi.DTOs;
using BookStoreApi.Interfaces;
using BookStoreApi.Models;

namespace BookStoreApi.Services
{
    /// <summary>
    /// IBookService interfész implementációja - üzleti logika réteg.
    /// Az IBookRepository interfészen keresztül kommunikál az adatbázissal,
    /// ezért a repository könnyen cserélhető Mock-ra teszteléskor.
    /// </summary>
    public class BookService : IBookService
    {
        private readonly IBookRepository _repository;

        public BookService(IBookRepository repository)
        {
            _repository = repository;
        }

        public async Task<Book> CreateBookAsync(BookCreateDto bookCreateDto)
        {
            var book = new Book
            {
                Title = bookCreateDto.Title,
                Author = bookCreateDto.Author,
                ISBN = bookCreateDto.ISBN,
                Price = bookCreateDto.Price,
                PublishedYear = bookCreateDto.PublishedYear,
                CreatedAt = DateTime.UtcNow
            };

            return await _repository.CreateAsync(book);
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
           return await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Book>> GetAllBooksASync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Book> GetBookByIdAsync(int id)
        {
           return await _repository.GetByIdAsync(id);
        }

        public async Task<Book> UpdateBookAsync(int id, BookCreateDto bookCreateDto)
        {
            var book = new Book
            {
                Title = bookCreateDto.Title,
                Author = bookCreateDto.Author,
                ISBN = bookCreateDto.ISBN,
                Price = bookCreateDto.Price,
                PublishedYear = bookCreateDto.PublishedYear,
                CreatedAt = DateTime.UtcNow
            };

            return await _repository.UpdateAsync(id, book);
        }
    }
}
