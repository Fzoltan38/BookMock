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
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

    }
}
