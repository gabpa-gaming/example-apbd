namespace EFCore_CodeFirst_Test_Example.Entities;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }

    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}