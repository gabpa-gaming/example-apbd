namespace EFCore_CodeFirst_Test_Example.DTOs;

public record RentalResponse(
    string UserName,
    string? UserPhone,
    string? UserEmail,
    DateTime RentDate,
    DateTime? ReturnDate
);