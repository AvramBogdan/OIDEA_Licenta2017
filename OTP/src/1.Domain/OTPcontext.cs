using System;
using System.Linq;
using OTP.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace OTP.Domain
{
    public class OTPContext : DbContext
    {

        public DbSet<User> Users { get; set; }



        public void Seed()
        {

            if (!Users.Any())
            {
                Users.Add(new User()
                {
                    Id = Guid.Parse("EBF73255-1252-4FD0-AAD5-7C9D61E38824"),
                    CompleteName = "Grivei",
                    UserName = "grivei",
                    Email = "grivei@mail.com",
                    Password = "griveiNo.1"
                });


                Users.Add(new User()
                {
                    Id = Guid.Parse("1A684DE2-CE5E-487E-A419-68C777B29831"),
                    CompleteName = "Azorel",
                    UserName = "azorel",
                    Email = "azorel@mail.com",
                    Password = "azorelNo.1"
                });

                SaveChanges();
            }
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("data source = (localdb)\\MSSQLLocalDB;Initial Catalog=OnlineToolPlatform;Integrated Security=True;MultipleActiveResultSets=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<User>().Property(b => b.RowVersion)
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        }

        public override int SaveChanges()
        {
            var changes = ChangeTracker.Entries<BaseClass>().Where(p => p.State == EntityState.Modified).Select(u => u.Entity);
            foreach (var change in changes)
            {
                Entry(change).Property(u => u.RowVersion).OriginalValue = new byte[] { 0, 0, 0, 0, 0, 0, 0, 120 };
                var a = Entry(change).Property(u => u.RowVersion).OriginalValue;
            }

            return base.SaveChanges();
        }
    }
}
