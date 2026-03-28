// MOCKING BEMUTATÓ - BooksController tesztek
// =============================================================
// A controller tesztelésekor az IBookService-t mock-oljuk.
// Így a controller HTTP logikája önállóan tesztelhető,
// függetlenül az adatbázistól és az üzleti logikától.
// =============================================================

using BookStoreApi.Models;

namespace BookStoreTests.Controllers
{

    internal class BookControllerTests
    {
        public static List<Book> GetSampleBook() =>
        [
            new () { Id = 1, Title = "Clean Code", Author = "Robert C. Martin", ISBN = "978-0132350884", Price= 8990, PublishedYear = 2008},
            new () { Id = 2, Title = "The Programmer", Author = "David Thomas", ISBN = "978-0135957059", Price= 9990, PublishedYear = 2019}
        ];
    }
}
