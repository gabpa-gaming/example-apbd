using System.ComponentModel.DataAnnotations;

namespace EFCore_CodeFirst_Test_Example.DTOs;

public record BookRequest(
    [Required, MaxLength(100)] string Title,
    [Required, MaxLength(50)] string AuthorFirstName,
    [Required, MaxLength(200)] string AuthorLastName,
    [Required] ICollection<string> Genres
);