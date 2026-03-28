// MOCKING BEMUTATÓ - BooksController tesztek
// =============================================================
// A controller tesztelésekor az IBookService-t mock-oljuk.
// Így a controller HTTP logikája önállóan tesztelhető,
// függetlenül az adatbázistól és az üzleti logikától.
// =============================================================

using BookStoreApi.Controllers;
using BookStoreApi.DTOs;
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

        // -------------------------------------------------------
        // 2. TESZT: GET /api/books/{id} - 200 OK meglévő könyvnél
        // 

        [Fact]
        public async Task GetByID_WhennBookExists_SholdReturn200()
        {
            var mockService = new Mock<IBookService>();
            var book = GetSampleBook().First();

            mockService
                .Setup(s => s.GetBookByIdAsync(1))
                .ReturnsAsync(book);

            var controller = new BookController(mockService.Object);

            var actionResult = await controller.GetById(1);

            var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);

            var returnBook = okResult.Value.Should().BeOfType<Book>().Subject;

            returnBook.Id.Should().Be(1);
            returnBook.Title.Should().Be("Clean Code");

        }

        // -------------------------------------------------------
        // 3. TESZT: GET /api/books/{id} - 404 Not Found
        // -------------------------------------------------------

        [Fact]
        public async Task GetById_WhenBookExsits_SholudReturn404()
        {
            var mockService = new Mock<IBookService>();

                mockService
                    .Setup(s => s.GetBookByIdAsync(999))
                    .ReturnsAsync((Book?)null);

            var controller = new BookController(mockService.Object);

            var actionResult = await controller.GetById(999);

            var notFoundREsult = actionResult.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundREsult.StatusCode.Should().Be(404);
        }

        // -------------------------------------------------------
        // 4. TESZT: POST /api/books - 201 Created
        // -------------------------------------------------------

        [Fact]
        public async Task Create_ShouldReturn201WithCreateBook() 
        {
            var mockService = new Mock<IBookService>();

            var dto = new BookCreateDto
            {
                Title = "Új könyv",
                Author = "Teszt Szerző",
                ISBN = "978-1111111111",
                Price = 3990,
                PublishedYear = 2024
            };

            var createdBook = new Book
            {
                Id  = 10,
                Title = dto.Title,
                Author = dto.Author,
                ISBN = dto.ISBN,
                Price = dto.Price,
                PublishedYear= dto.PublishedYear,
            };

            mockService
                .Setup(s => s.CreateBookAsync(dto))
                .ReturnsAsync(createdBook);

            var controller = new BookController(mockService.Object);

            var actionResult = await controller.Create(dto);

            var createdResult = actionResult.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.StatusCode.Should().Be(201);

            var returnedBook = createdResult.Value.Should().BeOfType<Book>().Subject;
            returnedBook.Id.Should().Be(10);
            returnedBook.Title.Should().Be("Új könyv");

        }

        // -------------------------------------------------------
        // 5. TESZT: PUT /api/books/{id} - 200 OK sikeres frissítésnél
        // -------------------------------------------------------

        [Fact]
        public async Task Update_WhenBookExists_ShouldReturn200()
        {
            var mockService = new Mock<IBookService>();

            var dto = new BookUpdateDto
            {
                Title = "Frissített Cím",
                Author = "Frissített Szerző",
                ISBN = "978-22222222222",
                Price = 6990,
                PublishedYear = 2022
            };

            var updateBook = new Book
            {
                Id = 1,
                Title = dto.Title,
                Author = dto.Author,
            };

            mockService
                .Setup(s => s.UpdateBookAsync(1, dto))
                .ReturnsAsync(updateBook);

            var controller = new BookController(mockService.Object);

            var actionResult = await controller.Update(1, dto);

            var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);

        }

        // -------------------------------------------------------
        // 6. TESZT: DELETE /api/books/{id} - 204 No Content
        // -------------------------------------------------------
        [Fact]
        public async Task Delete_WhenBookExsits_ShouldReturn204() 
        {
            var mockService = new Mock<IBookService>();

            mockService
                .Setup(s => s.DeleteBookAsync(1))
                .ReturnsAsync(true);

            var controller = new BookController(mockService.Object);

            var actionResult = await controller.Delete(1);

            var noContentResult = actionResult.Should().BeOfType<NoContentResult>().Subject;

            noContentResult.StatusCode.Should().Be(204);

            // Ellenőrizzük, hogy a service metódusa valóban meghívódott
            mockService.Verify(s => s.DeleteBookAsync(1), Times.Once);

        }
    }
}
