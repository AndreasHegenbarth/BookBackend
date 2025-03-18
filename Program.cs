var builder = WebApplication.CreateBuilder(args);


// Dependency Injection für Repository als Singleton
builder.Services.AddSingleton<IBookRepository, BookRepository>();

// CORS hinzufügen
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(b =>
    {
        b.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost");
        b.AllowAnyHeader();
    });
});

// GraphQL Schema und Resolver registrieren (Einbindung des GraphQL-Server über die HotChocolate-Bibliothek)
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>();


// Baut die Anwendung
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}


app.UseHttpsRedirection(); // Best Practice für Sicherheit Erzwingt HTTPS, um die Kommunikation abzusichern.
app.UseCors();


// GraphQL-Endpunkt direkt auf oberster Ebene registrieren. Registriert die GraphQL-API-Route (/graphql).
app.MapGraphQL();
app.Run();


// Interface für das Repository (Dependency Inversion)
public interface IBookRepository
{
    IEnumerable<Book> GetBooks();
    Book AddBook(string title, string author); 
    Book? UpdateBookTitle(int id, string newTitle); // weiter Methode

}


// Implementierung des Repositories (Einfach austauschbar). Erstellt eine einfache In-Memory-Datenbank (Liste _books).
public class BookRepository : IBookRepository
{
    private readonly List<Book> _books =
    [
        new Book(1, "GraphQL für Einsteiger", "Max Mustermann", DateTime.Now),
        new Book(2, "GraphQL in der Praxis", "Lisa Musterfrau", DateTime.Now)
    ];

    public IEnumerable<Book> GetBooks() => _books;

    public Book AddBook(string title, string author)
    {
        var newBook = new Book(_books.Count + 1, title, author, DateTime.Now);
        _books.Add(newBook); 
        return newBook;
    }
    public Book? UpdateBookTitle(int id, string newTitle) // Titel aktualisieren
    {
        var book = _books.FirstOrDefault(b => b.Id == id);
        if (book == null) return null; // Falls Buch nicht gefunden wird

        var updatedBook = book with { Title = newTitle }; // Neuen Record erstellen
        _books[_books.FindIndex(b => b.Id == id)] = updatedBook; // Buch ersetzen

        return updatedBook;
    }
}


// GraphQL Query-Resolver (Dependency Injection). Liest B�cher aus dem Repository.
public class Query(IBookRepository repository)
{
    public IEnumerable<Book> GetBooks() => repository.GetBooks();
}


// GraphQL Mutation-Resolver (Dependency Injection). F�gt neue B�cher �ber GraphQL hinzu.
public class Mutation(IBookRepository repository)
{
    public Book AddBook(string title, string author) => repository.AddBook(title, author);

    public Book? UpdateBookTitle(int id, string newTitle)
    {
        return repository.UpdateBookTitle(id, newTitle);
    }
}


// Datenmodell f�r B�cher
public record Book(int Id, string Title, string Author, DateTime Date);
