// =============================================================
// MOCKING BEMUTATÓ - BookService tesztek
// =============================================================
// Mit csinál a Mock?
//   A Mock<IBookRepository> egy "hamis" repository objektumot hoz létre,
//   amely valójában NEM csatlakozik MySQL adatbázishoz.
//   Meghatározzuk, hogy mit adjon vissza egyes metódus hívásokra (Setup),
//   majd ellenőrizzük, hogy a service helyesen használja-e (Verify).
//
// Miért jó ez?
//   - Nem kell futó MySQL adatbázis a tesztekhez
//   - A tesztek gyorsak és izoláltak
//   - Pontosan teszteljük az üzleti logikát, nem az adatbázist
// =============================================================

using BookStoreApi.DTOs;
using BookStoreApi.Interfaces;
using BookStoreApi.Models;
using BookStoreApi.Services;
using FluentAssertions;
using Moq;

namespace BookStore.Tests.Services;

public class BookServiceTests
{
    // -------------------------------------------------------
    // SEGÉDMÓDSZEREK - tesztelési adatok előkészítése
    // -------------------------------------------------------

    private static List<Book> GetSampleBooks() =>
    [
        new() { Id = 1, Title = "Clean Code", Author = "Robert C. Martin", ISBN = "978-0132350884", Price = 8990, PublishedYear = 2008 },
        new() { Id = 2, Title = "The Pragmatic Programmer", Author = "David Thomas", ISBN = "978-0135957059", Price = 9990, PublishedYear = 2019 }
    ];

    // -------------------------------------------------------
    // 1. TESZT: GetAllBooksAsync - összes könyv lekérése
    // -------------------------------------------------------

    [Fact]
    public async Task GetAllBooksAsync_ShouldReturnAllBooks()
    {
        // ARRANGE - Előkészítés
        // ----------------------
        // Mock létrehozása: egy "hamis" IBookRepository objektum
        var mockRepo = new Mock<IBookRepository>();

        // Setup: meghatározzuk mit adjon vissza a GetAllAsync() hívásra
        // A valódi adatbázis helyett ezt a listát kapja vissza a service
        mockRepo
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(GetSampleBooks());

        // A service-t a mock repository-val hozzuk létre (Dependency Injection)
        var service = new BookService(mockRepo.Object);

        // ACT - Végrehajtás
        // ------------------
        var result = await service.GetAllBooksASync();

        // ASSERT - Ellenőrzés
        // --------------------
        var books = result.ToList();
        books.Should().HaveCount(2);
        books.First().Title.Should().Be("Clean Code");

        // Verify: ellenőrizzük, hogy a GetAllAsync() pontosan egyszer lett meghívva
        mockRepo.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    // -------------------------------------------------------
    // 2. TESZT: GetBookByIdAsync - meglévő könyv
    // -------------------------------------------------------

    [Fact]
    public async Task GetBookByIdAsync_WhenBookExists_ShouldReturnBook()
    {
        // ARRANGE
        var mockRepo = new Mock<IBookRepository>();
        var expectedBook = GetSampleBooks().First();

        // Setup: az id=1 kérésre visszaadja a könyvet
        mockRepo
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(expectedBook);

        var service = new BookService(mockRepo.Object);

        // ACT
        var result = await service.GetBookByIdAsync(1);

        // ASSERT
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Title.Should().Be("Clean Code");
        result.Author.Should().Be("Robert C. Martin");
    }

    // -------------------------------------------------------
    // 3. TESZT: GetBookByIdAsync - nem létező könyv
    // -------------------------------------------------------

    [Fact]
    public async Task GetBookByIdAsync_WhenBookNotExists_ShouldReturnNull()
    {
        // ARRANGE
        var mockRepo = new Mock<IBookRepository>();

        // Setup: a nem létező id-re null-t ad vissza
        mockRepo
            .Setup(repo => repo.GetByIdAsync(999))
            .ReturnsAsync((Book?)null);

        var service = new BookService(mockRepo.Object);

        // ACT
        var result = await service.GetBookByIdAsync(999);

        // ASSERT
        result.Should().BeNull();
    }

    // -------------------------------------------------------
    // 4. TESZT: CreateBookAsync - új könyv létrehozása
    // -------------------------------------------------------

    [Fact]
    public async Task CreateBookAsync_ShouldCallRepositoryAndReturnCreatedBook()
    {
        // ARRANGE
        var mockRepo = new Mock<IBookRepository>();

        var dto = new BookCreateDto
        {
            Title = "Új Könyv",
            Author = "Teszt Szerző",
            ISBN = "978-1234567890",
            Price = 4990,
            PublishedYear = 2024
        };

        // Setup: a CreateAsync bármilyen Book objektummal hívva visszaadja a mentett könyvet
        // It.IsAny<Book>() -> "bármilyen Book objektumra illeszkedj"
        mockRepo
            .Setup(repo => repo.CreateAsync(It.IsAny<Book>()))
            .ReturnsAsync((Book b) =>
            {
                b.Id = 42; // Szimulált adatbázis által generált ID
                return b;
            });

        var service = new BookService(mockRepo.Object);

        // ACT
        var result = await service.CreateBookAsync(dto);

        // ASSERT
        result.Should().NotBeNull();
        result.Id.Should().Be(42);
        result.Title.Should().Be("Új Könyv");
        result.Author.Should().Be("Teszt Szerző");

        // Verify: a CreateAsync pontosan egyszer lett meghívva
        mockRepo.Verify(repo => repo.CreateAsync(It.IsAny<Book>()), Times.Once);
    }

    // -------------------------------------------------------
    // 5. TESZT: UpdateBookAsync - meglévő könyv módosítása
    // -------------------------------------------------------

    [Fact]
    public async Task UpdateBookAsync_WhenBookExists_ShouldReturnUpdatedBook()
    {
        // ARRANGE
        var mockRepo = new Mock<IBookRepository>();

        var dto = new BookUpdateDto
        {
            Title = "Módosított Cím",
            Author = "Módosított Szerző",
            ISBN = "978-9999999999",
            Price = 5990,
            PublishedYear = 2023
        };

        var updatedBook = new Book
        {
            Id = 1,
            Title = dto.Title,
            Author = dto.Author,
            ISBN = dto.ISBN,
            Price = dto.Price,
            PublishedYear = dto.PublishedYear
        };

        mockRepo
            .Setup(repo => repo.UpdateAsync(1, It.IsAny<Book>()))
            .ReturnsAsync(updatedBook);

        var service = new BookService(mockRepo.Object);

        // ACT
        var result = await service.UpdateBookAsync(1, dto);

        // ASSERT
        result.Should().NotBeNull();
        result!.Title.Should().Be("Módosított Cím");
        result.Price.Should().Be(5990);
    }

    // -------------------------------------------------------
    // 6. TESZT: UpdateBookAsync - nem létező könyv módosítása
    // -------------------------------------------------------

    [Fact]
    public async Task UpdateBookAsync_WhenBookNotExists_ShouldReturnNull()
    {
        // ARRANGE
        var mockRepo = new Mock<IBookRepository>();

        mockRepo
            .Setup(repo => repo.UpdateAsync(999, It.IsAny<Book>()))
            .ReturnsAsync((Book?)null);

        var service = new BookService(mockRepo.Object);

        // ACT
        var result = await service.UpdateBookAsync(999, new BookUpdateDto
        {
            Title = "X",
            Author = "X",
            ISBN = "X",
            Price = 1,
            PublishedYear = 2000
        });

        // ASSERT
        result.Should().BeNull();
    }

    // -------------------------------------------------------
    // 7. TESZT: DeleteBookAsync - sikeres törlés
    // -------------------------------------------------------

    [Fact]
    public async Task DeleteBookAsync_WhenBookExists_ShouldReturnTrue()
    {
        // ARRANGE
        var mockRepo = new Mock<IBookRepository>();

        mockRepo
            .Setup(repo => repo.DeleteAsync(1))
            .ReturnsAsync(true);

        var service = new BookService(mockRepo.Object);

        // ACT
        var result = await service.DeleteBookAsync(1);

        // ASSERT
        result.Should().BeTrue();
        mockRepo.Verify(repo => repo.DeleteAsync(1), Times.Once);
    }

    // -------------------------------------------------------
    // 8. TESZT: DeleteBookAsync - nem létező könyv törlése
    // -------------------------------------------------------

    [Fact]
    public async Task DeleteBookAsync_WhenBookNotExists_ShouldReturnFalse()
    {
        // ARRANGE
        var mockRepo = new Mock<IBookRepository>();

        mockRepo
            .Setup(repo => repo.DeleteAsync(999))
            .ReturnsAsync(false);

        var service = new BookService(mockRepo.Object);

        // ACT
        var result = await service.DeleteBookAsync(999);

        // ASSERT
        result.Should().BeFalse();
    }
}
