using EFCore_CodeFirst_Test_Example.DTOs;

namespace EFCore_CodeFirst_Test_Example.Services;

public interface IDbService
{
    Task<ICollection<BookResponse>> GetAllBooksAsync(string? title, CancellationToken cancellationToken);
    Task<BookResponse> GetBookAsync(int id, CancellationToken cancellationToken);
    Task AddBookAsync(BookRequest request, CancellationToken cancellationToken);
    Task UpdateBookAsync(int id, BookRequest request, CancellationToken cancellationToken);
    Task DeleteBookAsync(int id, CancellationToken cancellationToken);
}