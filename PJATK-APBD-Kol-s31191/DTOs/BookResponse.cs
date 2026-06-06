namespace EFCore_CodeFirst_Test_Example.DTOs;

public record BookResponse(
    int Id,
    string Title,
    string Author,
    List<string> Genres,
    List<RentalResponse> Rentals
);