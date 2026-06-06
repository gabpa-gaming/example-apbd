namespace EFCore_CodeFirst_Test_Example.Entities;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AuthorFirstName { get; set; } = string.Empty;
    public string AuthorLastName { get; set; } = string.Empty;

    public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();
    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}