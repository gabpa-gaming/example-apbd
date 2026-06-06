using EFCore_CodeFirst_Test_Example.DTOs;
using EFCore_CodeFirst_Test_Example.Exceptions;
using EFCore_CodeFirst_Test_Example.Services;
using Microsoft.AspNetCore.Mvc;

namespace EFCore_CodeFirst_Test_Example.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController(IDbService service) : ControllerBase
{
    // Pobierz wszystkie książki z bazy danych. Dodaj opcjonalne filtrowanie po tytule.
    /*
     * Przykładowe zapytania:
     * GET api/books
     * GET api/books?title=władca
     */
    /*
     Wymagany format ciała odpowiedzi:
     [
         {
           "id": 1,
           "title": "Test",
           "author": "First Last",
           "genres": [
             "TestG",
             "testG2"
           ],
           "rentals": [
             {
               "userName": "TestUserName testUserLName",
               "userPhone": null,
               "userEmail": "k@k.com",
               "rentDate": "2026-05-25T13:52:18",
               "returnDate": null
             }
           ]
         },
         ...
       ]
     */
    [HttpGet]
    public Task<ICollection<BookResponse>> GetAllBooks([FromQuery] string? title, CancellationToken cancellationToken)
        => service.GetAllBooksAsync(title, cancellationToken);

    // Pobierz książke z bazy danych po jej Id
    /*
     * Przykładowe zapytania:
     * GET api/books/1
     * GET api/books/2
     */
    /*
     Wymagany format ciała odpowiedzi:
     {
       "id": 1,
       "title": "Test",
       "author": "First Last",
       "genres": [
         "TestG",
         "testG2"
       ],
       "rentals": [
         {
           "userName": "TestUserName testUserLName",
           "userPhone": null,
           "userEmail": "k@k.com",
           "rentDate": "2026-05-25T13:52:18",
           "returnDate": null
         }
       ]
     }
     */
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetBookAsync([FromRoute] int id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.GetBookAsync(id, cancellationToken));
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    // Dodaj nową książkę do bazy danych z przypisaniem do niej tytułów. Jeżeli gatunek nie istnieje, utwórz go.
    // Przed dodaniem książki, sprawdź, czy nie istnieje już w bazie książka o takim tytule.
    /*
     Wymagany format ciała zapytania:
     {
         "Title": "Moja ksiazka",
         "AuthorFirstName": "Testowy autor",
         "AuthorLastName": "Testowy autor",
         "Genres": ["Przygoda", "Akcja", "Tajemnica"]
     }
     */
    [HttpPost]
    public async Task<IActionResult> AddBookAsync([FromBody] BookRequest req, CancellationToken cancellationToken)
    {
        try
        {
            await service.AddBookAsync(req, cancellationToken);
            return Created();
        }
        catch (ConflictException e)
        {
            return Conflict(e.Message);
        }
    }

    // Zaktualizuj istniejącą w bazie danych książce. Podmień wszystkie informacje o niej wraz z przypisanymi gatunkami.
    // Jeżeli dowolny z podanych w zapytaniu gatunków nie istnieje, utwórz je.
    // Jeżeli w bazie istnieje już książka o nazwie, której chcemy użyć w ramach aktualizacji, należy przerwać wykonywanie zapytania.
    /*
     * Przykładowe zapytania:
     * PUT api/books/1
     */
    /*
     Wymagany format ciała zapytania:
     {
           "Title": "Moja ksiazka",
           "AuthorFirstName": "Testowy autor",
           "AuthorLastName": "Testowy autor",
           "Genres": ["Przygoda", "Akcja", "Tajemnica"]
       }
     */
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateBookAsync([FromRoute] int id, [FromBody] BookRequest req, CancellationToken cancellationToken)
    {
        try
        {
            await service.UpdateBookAsync(id, req, cancellationToken);
            return NoContent();
        }
        catch (ConflictException e)
        {
            return Conflict(e.Message);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    // Usuń istniejącą w bazie danych książkę. Jeżeli książka nie została jeszcze oddana z wypożyczenia, należy zatrzymać wykonywanie zapytania.
    // Załóż, że w bazie danych wyłączone jest kaskadowe usuwanie danych - usuń ręcznie dane książki z tabeli rentals oraz books.
    /*
     * Przykładowe zapytania:
     * DELETE api/books/1
     */
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBookAsync([FromRoute] int id, CancellationToken cancellationToken)
    {
        try
        {
            await service.DeleteBookAsync(id, cancellationToken);
            return NoContent();
        }
        catch (ConflictException e)
        {
            return Conflict(e.Message);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}