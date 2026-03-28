# Mocking a .NET tesztelésben – Oktatási útmutató

## A BookStore API projekt alapján

---

## 1. Mi az a Mocking?

A **mocking** egy tesztelési technika, amellyel valódi függőségeket (adatbázis, HTTP hívás, fájlrendszer) **hamis, vezérelt objektumokra** cseréljük le.

```
Valódi kód:                      Teszteléskor:
┌──────────────┐                 ┌──────────────┐
│ BookService  │ → adatbázis     │ BookService  │ → Mock objektum
│              │   MySQL         │              │   (memóriában)
└──────────────┘                 └──────────────┘
```

### Miért van szükség rá?

| Probléma                           | Megoldás Mock-kal               |
|------------------------------------|---------------------------------|
| Nincs mindig adatbázis a CI/CD-n   | Mock visszaad adatot adatbázis nélkül |
| Az adatbázis-hívás lassú           | Mock azonnal visszatér          |
| Adatbázis állapota tesztenként változik | Mock mindig előre meghatározott adatot ad |
| Nehéz hibát szimulálni             | Mock bármilyen kivételt dobhat  |

---

## 2. Az Interfész mint kapocs

A mocking **csak akkor működik**, ha a kód **interfészen keresztül** kommunikál a függőségeivel.

```csharp
// ❌ ROSSZ - nem lehet mock-olni:
public class BookService
{
    private readonly BookRepository _repository; // konkrét osztály!

    public BookService()
    {
        _repository = new BookRepository(); // "beégett" függőség
    }
}

// ✅ JÓ - interfész + Dependency Injection:
public class BookService
{
    private readonly IBookRepository _repository; // interfész!

    public BookService(IBookRepository repository) // kívülről kapja
    {
        _repository = repository;
    }
}
```

A tesztben az interfészhez adunk **Mock implementációt**:

```csharp
var mockRepo = new Mock<IBookRepository>(); // mock a valódi repo helyett
var service = new BookService(mockRepo.Object); // átadjuk a service-nek
```

---

## 3. A Moq könyvtár – főbb fogalmak

### 3.1 Mock létrehozása

```csharp
var mockRepo = new Mock<IBookRepository>();
```

A `mockRepo.Object` egy `IBookRepository` típusú objektum, amely **alapból nem csinál semmit** – minden metódusa `null`-t vagy `default`-ot ad vissza.

---

### 3.2 Setup – Mit adjon vissza?

A `Setup` meghatározza, hogy egy adott metódus hívásra mit kapunk vissza:

```csharp
// Pontosan meghatározott argumentumra:
mockRepo
    .Setup(repo => repo.GetByIdAsync(1))
    .ReturnsAsync(new Book { Id = 1, Title = "Clean Code" });

// Bármilyen argumentumra (It.IsAny):
mockRepo
    .Setup(repo => repo.CreateAsync(It.IsAny<Book>()))
    .ReturnsAsync(new Book { Id = 42, Title = "Új könyv" });

// Null visszaadása (nem létező elem):
mockRepo
    .Setup(repo => repo.GetByIdAsync(999))
    .ReturnsAsync((Book?)null);
```

#### `It.IsAny<T>()` – "Bármire illeszkedj"

```csharp
// Csak az id=1-re illeszkedik:
.Setup(repo => repo.DeleteAsync(1))

// Bármilyen int értékre illeszkedik:
.Setup(repo => repo.DeleteAsync(It.IsAny<int>()))
```

---

### 3.3 Verify – Meghívódott-e a metódus?

A `Verify` ellenőrzi, hogy az adott metódust **meghívták-e** és **hányszor**:

```csharp
// Pontosan egyszer lett meghívva:
mockRepo.Verify(repo => repo.DeleteAsync(1), Times.Once);

// Legalább egyszer:
mockRepo.Verify(repo => repo.GetAllAsync(), Times.AtLeastOnce);

// Soha nem lett meghívva:
mockRepo.Verify(repo => repo.DeleteAsync(It.IsAny<int>()), Times.Never);

// Pontosan N-szer:
mockRepo.Verify(repo => repo.GetAllAsync(), Times.Exactly(3));
```

---

### 3.4 Callback – Oldalmellékhatás szimulálása

```csharp
var capturedBook = (Book?)null;

mockRepo
    .Setup(repo => repo.CreateAsync(It.IsAny<Book>()))
    .Callback<Book>(book => capturedBook = book) // elkapjuk az argumentumot
    .ReturnsAsync((Book b) => { b.Id = 1; return b; });

// A teszt után megvizsgálhatjuk:
capturedBook.Should().NotBeNull();
capturedBook!.Title.Should().Be("Várt cím");
```

---

### 3.5 Kivétel dobása

```csharp
mockRepo
    .Setup(repo => repo.GetAllAsync())
    .ThrowsAsync(new Exception("Adatbázis hiba!"));

// Tesztben:
var act = async () => await service.GetAllBooksAsync();
await act.Should().ThrowAsync<Exception>()
    .WithMessage("Adatbázis hiba!");
```

---

## 4. A projekt rétegei és mit mock-olunk

```
┌─────────────────────────────────────────────────────────┐
│                    HTTP Kérés                           │
└─────────────────────────────────────────────────────────┘
                          │
┌─────────────────────────────────────────────────────────┐
│              BooksController                            │
│  Függőség: IBookService                                 │
│  Teszteléskor: Mock<IBookService>                       │
└─────────────────────────────────────────────────────────┘
                          │
┌─────────────────────────────────────────────────────────┐
│              BookService                                │
│  Függőség: IBookRepository                              │
│  Teszteléskor: Mock<IBookRepository>                    │
└─────────────────────────────────────────────────────────┘
                          │
┌─────────────────────────────────────────────────────────┐
│              BookRepository                             │
│  Függőség: AppDbContext (MySQL)                         │
│  Teszteléskor: InMemory DB vagy külön integrációs teszt │
└─────────────────────────────────────────────────────────┘
                          │
┌─────────────────────────────────────────────────────────┐
│              MySQL adatbázis                            │
└─────────────────────────────────────────────────────────┘
```

---

## 5. Tesztelési minták a projektben

### 5.1 Service Unit teszt (BookServiceTests.cs)

A service-ben az **IBookRepository** interfész van mock-olva:

```csharp
[Fact]
public async Task CreateBookAsync_ShouldCallRepositoryAndReturnCreatedBook()
{
    // 1. Mock létrehozása
    var mockRepo = new Mock<IBookRepository>();

    // 2. Setup: a CreateAsync bármilyen Book-ot elfogad és visszaad egyet
    mockRepo
        .Setup(repo => repo.CreateAsync(It.IsAny<Book>()))
        .ReturnsAsync((Book b) => { b.Id = 42; return b; });

    // 3. A service-t a mock-kal hozzuk létre
    var service = new BookService(mockRepo.Object);

    // 4. Végrehajtás
    var result = await service.CreateBookAsync(new BookCreateDto
    {
        Title = "Új Könyv", Author = "Szerző",
        ISBN = "978-0000000000", Price = 1990, PublishedYear = 2024
    });

    // 5. Ellenőrzés
    result.Id.Should().Be(42);
    result.Title.Should().Be("Új Könyv");

    // 6. Verify: a CreateAsync pontosan egyszer hívódott meg
    mockRepo.Verify(repo => repo.CreateAsync(It.IsAny<Book>()), Times.Once);
}
```

### 5.2 Controller Unit teszt (BooksControllerTests.cs)

A controller-ben az **IBookService** interfész van mock-olva:

```csharp
[Fact]
public async Task GetById_WhenBookNotExists_ShouldReturn404()
{
    // 1. IBookService mock-ja
    var mockService = new Mock<IBookService>();

    // 2. Setup: nem létező id-re null-t ad
    mockService
        .Setup(s => s.GetBookByIdAsync(999))
        .ReturnsAsync((Book?)null);

    // 3. Controller létrehozása a mock service-szel
    var controller = new BooksController(mockService.Object);

    // 4. Végrehajtás
    var result = await controller.GetById(999);

    // 5. Ellenőrzés: 404-et vártunk
    result.Should().BeOfType<NotFoundObjectResult>()
          .Which.StatusCode.Should().Be(404);
}
```

---

## 6. A tesztek futtatása

```bash
# Az összes teszt futtatása
dotnet test

# Részletes kimenet
dotnet test --verbosity normal

# Csak egy adott teszt osztály
dotnet test --filter "FullyQualifiedName~BookServiceTests"

# Kód lefedettség mérése
dotnet test --collect:"XPlat Code Coverage"
```

**Várt eredmény:**
```
Test Run Successful.
Total tests: 16
     Passed: 16
     Failed: 0
```

---

## 7. Összefoglalás

| Fogalom       | Leírás                                                  |
|---------------|---------------------------------------------------------|
| **Mock**      | Hamis objektum, amely interfészt implementál            |
| **Setup**     | Meghatározza mit adjon vissza egy adott hívásra         |
| **Verify**    | Ellenőrzi hogy és hányszor hívták meg a metódust        |
| **It.IsAny**  | Bármilyen argumentumra illeszkedő matcher               |
| **Times**     | Meghatározza a hívások elvárt számát                    |
| **DI**        | Dependency Injection – az interfész "befecskendezése"   |

**Az arany szabály:** Ha a kód interfészen keresztül kommunikál, akkor **tesztelési időben bármit berakhatunk** az interfész mögé – akár egy Mock objektumot is.
