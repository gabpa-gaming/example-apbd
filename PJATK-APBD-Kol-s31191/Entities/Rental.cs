namespace EFCore_CodeFirst_Test_Example.Entities;

public class Rental
{
    public int RentalId { get; set; }
    public int BookId { get; set; }
    public int UserId { get; set; }
    public DateTime RentedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }

    public virtual Book Book { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}