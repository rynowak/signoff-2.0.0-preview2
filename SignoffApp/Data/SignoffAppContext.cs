using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SignoffApp.Models
{
    public class SignoffAppContext : DbContext
    {
        public SignoffAppContext (DbContextOptions<SignoffAppContext> options)
            : base(options)
        {
        }

        public DbSet<Person> Person { get; set; }

        public DbSet<Person2> Person2 { get; set; }

        //public DbSet<Blog> Blogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Blog>().HasOne(e => e.MyBlog).WithOne(e => e.InverseBlog).HasForeignKey<Blog>(e => e.Id);

            modelBuilder.Entity<PersonTeacher>().HasBaseType<Person2>();
            modelBuilder.Entity<PersonKid>().HasBaseType<Person2>();
            modelBuilder.Entity<PersonFamily>();

            modelBuilder.Entity<PersonKid>(entity =>
            {
                entity.Property("Discriminator").HasMaxLength(63);
                entity.HasIndex("Discriminator");

                entity.HasOne(m => m.Teacher)
                    .WithMany(m => m.Students)
                    .HasForeignKey(m => m.TeacherId)
                    .HasPrincipalKey(m => m.Id)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
