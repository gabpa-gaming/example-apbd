using EFCore_CodeFirst_Test_Example.DTOs;
using EFCore_CodeFirst_Test_Example.Entities;
using EFCore_CodeFirst_Test_Example.Exceptions;
using EFCore_CodeFirst_Test_Example.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EFCore_CodeFirst_Test_Example.Services;

public class DbService(DatabaseContext ctx) : IDbService
{
    public async Task<ICollection<BookResponse>> GetAllBooksAsync(string? title, CancellationToken cancellationToken)
    {
        return await ctx.Books
            // filtr opcjonalny - jeśli title == null, warunek zawsze true
            // dzięki temu nie trzeba robić osobnych zapytań
            .Where(e => title == null || e.Title.Contains(title))
            // projekcja do DTO
            .Select(e => new BookResponse(
                e.Id,
                e.Title,
                $"{e.AuthorFirstName} {e.AuthorLastName}",
                e.Genres.Select(g => g.Name).ToList(),
                e.Rentals.Select(r => new RentalResponse(
                    $"{r.User.FirstName} {r.User.LastName}",
                    r.User.Phone,
                    r.User.Email,
                    r.RentedAt,
                    r.ReturnedAt
                )).ToList()
            )).ToListAsync(cancellationToken); // <- wykonanie zapytania na bazie
    }

    public async Task<BookResponse> GetBookAsync(int id, CancellationToken cancellationToken)
    {
        return await ctx.Books
            // where book.id = @id
            .Where(e => e.Id == id)
            .Select(e => new BookResponse(
                e.Id,
                e.Title,
                $"{e.AuthorFirstName} {e.AuthorLastName}",
                e.Genres.Select(g => g.Name).ToList(),
                e.Rentals.Select(r => new RentalResponse(
                    $"{r.User.FirstName} {r.User.LastName}",
                    r.User.Phone,
                    r.User.Email,
                    r.RentedAt,
                    r.ReturnedAt
                )).ToList()
            )).FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Book with id {id} not found"); // <- jeżeli książka nie istnieje,
                                                                                // rzucamy wyjątek
    }

    public async Task AddBookAsync(BookRequest request, CancellationToken cancellationToken)
    {
        List<Genre> genres = [];
        
        // Sprawdzenie, czy wszystkie podane gatunki, istnieją w bazie danych. Jeżeli nie, to je tworzymy.
        foreach (var genreName in request.Genres)
        {
            var genre = await ctx.Genres.FirstOrDefaultAsync(g => g.Name == genreName, cancellationToken);
            if (genre is null)
            {
                genre = new Genre
                {
                    Name = genreName
                };
                await ctx.Genres.AddAsync(genre, cancellationToken);
            }
            genres.Add(genre);
        }

        // Sprawdzenie, czy książka o danym tytule nie istnieje już w bazie
        if (await ctx.Books.AnyAsync(e => e.Title.Trim().ToLower() == request.Title.Trim().ToLower(), cancellationToken))
        {
            throw new ConflictException($"Book with title {request.Title} already exists");
        }

        var book = new Book
        {
            Title = request.Title,
            AuthorFirstName = request.AuthorFirstName,
            AuthorLastName = request.AuthorLastName,
            Genres = genres
        };
        
        await ctx.Books.AddAsync(book, cancellationToken);
        
        // Zapisanie danych w bazie. W ramach transakcji wykonają się tutaj wszystkie inserty gatunków i insert książki.
        await ctx.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateBookAsync(int id, BookRequest request, CancellationToken cancellationToken)
    {
        // Sprawdzenie, czy książka o danym tytule nie istnieje już w bazie
        if (await ctx.Books.AnyAsync(e => e.Title.Trim().ToLower() == request.Title.Trim().ToLower(), cancellationToken))
        {
            throw new ConflictException($"Book with title {request.Title} already exists");
        }
        
        var book = await ctx.Books
            // jawne zrobienie joina na tabelce, w celu umożliwienia usunięcia wszystkich przypisanych gatunków w prosty sposób
            .Include(e => e.Genres)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        
        // Sprawdzenie, czy książka o danym id istnieje w bazie
        if (book is null)
        {
            throw new NotFoundException($"Book with id {id} not found");
        }

        List<Genre> genres = [];
        
        // Sprawdzenie, czy wszystkie podane gatunki, istnieją w bazie danych. Jeżeli nie, to je tworzymy.
        foreach (var genreName in request.Genres)
        {
            var genre = await ctx.Genres.FirstOrDefaultAsync(g => g.Name == genreName, cancellationToken);
            if (genre is null)
            {
                genre = new Genre
                {
                    Name = genreName
                };
                await ctx.Genres.AddAsync(genre, cancellationToken);
            }
            genres.Add(genre);
        }
        
        // Usunięcie wszystkich przypisanych wcześniej gatunków książki
        book.Genres.Clear();
        
        book.Title = request.Title;
        book.AuthorFirstName = request.AuthorFirstName;
        book.AuthorLastName = request.AuthorLastName;
        // Przypisanie nowych gatunków do książki
        book.Genres = genres;
        
        // Pojedyncze savechangesasync = wszystkie powyższe operacje wykonują się w ramach jednej transakcji
        await ctx.SaveChangesAsync(cancellationToken); 
    }

    public async Task DeleteBookAsync(int id, CancellationToken cancellationToken)
    {
        var transaction = await ctx.Database.BeginTransactionAsync(cancellationToken); // jawna transakcja
        try
        {

            // Sprawdzenie, czy książka została oddana.
            var existingRentals = await ctx.Rentals
                .AnyAsync(e => e.BookId == id && e.ReturnedAt == null, cancellationToken);
            if (existingRentals)
            {
                throw new ConflictException($"cannot delete the book {id} because it is currently rented");
            }

            await ctx.Rentals.Where(e => e.BookId == 1).ExecuteDeleteAsync(cancellationToken);
            var removedRows = await ctx.Books.Where(e => e.Id == id).ExecuteDeleteAsync(cancellationToken);

            // sprawdzenie, czy książka w ogóle istnieje w bazie, patrząc na ilość usuniętych w bazie rekordów
            if (removedRows == 0)
            {
                throw new NotFoundException($"Book with id {id} not found");
            }
            
            await transaction.CommitAsync(cancellationToken); // zapisanie transakcji po wykonaniu wszystkich operacji
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken); // wycofanie transakcji, jeżeli wystąpi jakiś błąd
            throw;
        }
    }
}