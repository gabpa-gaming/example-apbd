using EFCore_CodeFirst_Test_Example.Entities;
using Microsoft.EntityFrameworkCore;

namespace EFCore_CodeFirst_Test_Example.Infrastructure;

public class DatabaseContext(DbContextOptions opt, IConfiguration configuration) : DbContext(opt)
{
    public virtual DbSet<Book> Books { get; set; }
    public virtual DbSet<Genre> Genres { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Rental> Rentals { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(configuration["DB:DefaultSchema"]);
        
        modelBuilder.Entity<Book>(opt =>
        {
            opt.HasKey(x => x.Id);
            
            opt.Property(x => x.Title)
                .HasMaxLength(100)
                .IsRequired();
            
            opt.Property(x => x.AuthorFirstName)
                .HasMaxLength(50)
                .IsRequired();
            
            opt.Property(x => x.AuthorLastName)
                .HasMaxLength(200)
                .IsRequired();

            opt.HasMany(x => x.Genres)
                .WithMany(x => x.Books)
                .UsingEntity<Dictionary<string, object>>(
                    "BookGenres",
                    x => x.HasOne<Genre>()
                        .WithMany()
                        .HasForeignKey("GenreId"),
                    x => x.HasOne<Book>()
                        .WithMany()
                        .HasForeignKey("BookId")
                );
        });

        modelBuilder.Entity<Genre>(opt =>
        {
            opt.HasKey(x => x.Id);
            
            opt.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();
        });

        modelBuilder.Entity<User>(opt =>
        {
            opt.HasKey(x => x.Id);
            
            opt.Property(x => x.FirstName)
                .HasMaxLength(50)
                .IsRequired();
            
            opt.Property(x => x.LastName)
                .HasMaxLength(50)
                .IsRequired();

            opt.Property(x => x.Phone)
                .HasColumnType("char(9)");

            opt.Property(x => x.Email)
                .HasMaxLength(200);
        });

        modelBuilder.Entity<Rental>(opt =>
        {
            opt.HasKey(x => x.RentalId);
            
            opt.Property(x => x.RentedAt)
                .IsRequired();
            
            opt.HasOne(x => x.Book)
                .WithMany(x => x.Rentals)
                .HasForeignKey(x => x.BookId);
            
            opt.HasOne(x => x.User)
                .WithMany(x => x.Rentals)
                .HasForeignKey(x => x.UserId);
        });
    }
}