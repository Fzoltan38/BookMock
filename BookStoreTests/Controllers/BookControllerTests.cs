// MOCKING BEMUTATÓ - BooksController tesztek
// =============================================================
// A controller tesztelésekor az IBookService-t mock-oljuk.
// Így a controller HTTP logikája önállóan tesztelhető,
// függetlenül az adatbázistól és az üzleti logikától.
// =============================================================

using BookStoreApi.Controllers;
using BookStoreApi.Interfaces;
using BookStoreApi.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BookStoreTests.Controllers
{

    public class BookControllerTests
    {
        public static List<Book> GetSampleBook() =>
        [
            new () { Id = 1, Title = "Clean Code", Author = "Robert C. Martin", ISBN = "978-0132350884", Price= 8990, PublishedYear = 2008},
            new () { Id = 2, Title = "The Programmer", Author = "David Thomas", ISBN = "978-0135957059", Price= 9990, PublishedYear = 2019}
        ];

        // -------------------------------------------------------
        // 1. TESZT: GET /api/books - 200 OK és lista visszaadás
        // -------------------------------------------------------

        [Fact]
        public async Task GetAll_ShouldReturn200WithBooks()
        {
            var mockService = new Mock<IBookService>();

            mockService
                .Setup(s => s.GetAllBooksASync())
                .ReturnsAsync(GetSampleBook);

            var controller = new BookController(mockService.Object);

            var actionResult = await controller.GetAll();

            var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);

            var books = okResult.Value.Should().BeAssignableTo<IEnumerable<Book>>().Subject;
            books.Should().HaveCount(2);

        }
    }
}
