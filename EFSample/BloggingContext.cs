using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace EFSaving.Basics
{
    public class Blog
    {
        public int BlogId { get; set; }
        public string Url { get; set; }

        public List<Post> Posts { get; set; }
    }

    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }

    public class BloggingContext : DbContext
    {
        public static readonly string CS = @"Server=(localdb)\mssqllocaldb;Database=EFSaving.Basics;Trusted_Connection=True;";

        private readonly DbConnection _connection;

        public BloggingContext()
        {
        }

        public BloggingContext(DbConnection connection)
        {
            _connection = connection;
        }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (_connection == null)
            {
                optionsBuilder.UseSqlServer(CS);
            }
            else
            {
                optionsBuilder.UseSqlServer(_connection);
            }
        }
    }

    public class PersonContext : DbContext
    {
        public DbSet<Person> People { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFSaving.Concurrency;Trusted_Connection=True;");
        }
    }

    public class Person
    {
        public int PersonId { get; set; }

        [ConcurrencyCheck]
        public string FirstName { get; set; }

        [ConcurrencyCheck]
        public string LastName { get; set; }

        public string PhoneNumber { get; set; }
    }

    public class Employee
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; }
        public DateTime EmploymentStarted { get; set; }
        public int Salary { get; set; }
        public DateTime? LastPayRaise { get; set; }
    }

    public class EmployeeContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFSaving.ExplicitValuesGenerateProperties;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region EmploymentStarted
            modelBuilder.Entity<Employee>()
                .Property(b => b.EmploymentStarted)
                .HasDefaultValueSql("CONVERT(date, GETDATE())");
            #endregion

            #region LastPayRaise
            modelBuilder.Entity<Employee>()
                .Property(b => b.LastPayRaise)
                .ValueGeneratedOnAddOrUpdate();

            modelBuilder.Entity<Employee>()
                .Property(b => b.LastPayRaise)
                .Metadata.AfterSaveBehavior = Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Save;
            #endregion
        }
    }
}