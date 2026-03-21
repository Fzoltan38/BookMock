using BookStoreApi.Models;

namespace BookStoreApi.Interfaces
{
    /// <summary>
    /// Repository interfész - definiálja az adatbázis műveleteket.
    /// A mock tesztek EZT az interfészt utánozzák, nem a tényleges adatbázist.
    /// </summary>
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllAsync();
        Task<Book> GetByIdAsync(int id);
        Task<Book> CreateAsync(Book book);
        Task<Book> UpdateAsync(int id, Book book);
        Task<Book> DeleteAsync(int id);
        Task<Book> ExistsAsync(int id);

    }
}
