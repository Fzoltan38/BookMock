using BookStoreApi.Data;
using BookStoreApi.Interfaces;
using BookStoreApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStoreApi.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly AppDbContext _context;

        public BookRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Book> CreateAsync(Book book)
        {
            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();
            return book;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book is null) return false;

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
           return await _context.Books.AnyAsync(book => book.Id == id);
        }

        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await _context.Books
                .OrderBy(t=> t.Title)
                .ToListAsync();
        }

        public async Task<Book> GetByIdAsync(int id)
        {
            return await _context.Books.FindAsync(id);
        }

        public async Task<Book> UpdateAsync(int id, Book book)
        {
            var exist = await _context.Books.FindAsync(id);
            if (exist is null) return null;
            
            exist.Title = book.Title;
            exist.Author = book.Author;
            exist.ISBN = book.ISBN;
            exist.Price = book.Price;
            exist.PublishedYear = book.PublishedYear;

            await _context.SaveChangesAsync();
            return exist;
        }
    }
}
