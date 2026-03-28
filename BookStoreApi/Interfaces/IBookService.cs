using BookStoreApi.DTOs;
using BookStoreApi.Models;

namespace BookStoreApi.Interfaces
{
    /// <summary>
    /// Service interfész - definiálja az üzleti logika műveleteket.
    /// A controller EZT az interfészt használja, így a tesztekben könnyen helyettesíthető.
    /// </summary>
    public interface IBookService
    {
        Task<IEnumerable<Book>> GetAllBooksASync();
        Task<Book> GetBookByIdAsync(int id);
        Task<Book> CreateBookAsync(BookCreateDto bookCreateDto);
        Task<Book> UpdateBookAsync(int id, BookUpdateDto bookCreateDto);
        Task<bool> DeleteBookAsync(int id);

    }
}
