using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using EFSaving.Basics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EFSample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting");

            using (var db = new BloggingContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }

            #region Basic

            using (var db = new BloggingContext())
            {
                var blog = new Blog { Url = "http://sample.com" };
                db.Blogs.Add(blog);
                db.SaveChanges();

                Console.WriteLine(blog.BlogId + ": " + blog.Url);
            }

            using (var db = new BloggingContext())
            {
                var blog = db.Blogs.First();
                blog.Url = "http://sample.com/blog";
                db.SaveChanges();

                Console.WriteLine(blog.BlogId + ": " + blog.Url);
            }

            using (var db = new BloggingContext())
            {
                var blog = db.Blogs.First();
                db.Blogs.Remove(blog);
                db.SaveChanges();

                Console.WriteLine(blog.BlogId + ": " + blog.Url);
            }

            using (var db = new BloggingContext())
            {
                db.Blogs.Add(new Blog { Url = "http://sample.com/blog_one" });
                db.Blogs.Add(new Blog { Url = "http://sample.com/blog_two" });
                db.SaveChanges();

                var firstBlog = db.Blogs.First();
                firstBlog.Url = "";

                var lastBlog = db.Blogs.Last();
                db.Blogs.Remove(lastBlog);

                db.SaveChanges();

                Console.WriteLine(firstBlog.BlogId + ": " + firstBlog.Url);
                Console.WriteLine(lastBlog.BlogId + ": " + lastBlog.Url);
            }

            #endregion

            #region Related

            using (var context = new BloggingContext())
            {
                var blog = new Blog
                {
                    Url = "http://blogs.msdn.com/dotnet",
                    Posts = new List<Post>
                    {
                        new Post { Title = "Intro to C#" },
                        new Post { Title = "Intro to VB.NET" },
                        new Post { Title = "Intro to F#" }
                    }
                };

                context.Blogs.Add(blog);
                context.SaveChanges();

                Console.WriteLine($"Post Count: {context.Posts.Count()}");
            }

            using (var context = new BloggingContext())
            {
                var blog = context.Blogs.Include(b => b.Posts).First();
                var post = new Post { Title = "Intro to EF Core" };

                blog.Posts.Add(post);
                context.SaveChanges();

                Console.WriteLine($"Post Count: {context.Posts.Count()}");
            }

            // This has workarounds
            using (var context = new BloggingContext())
            {
                var blog = new Blog { Url = "http://blogs.msdn.com/visualstudio" };
                var post = context.Posts.First();

                // Workaround
                blog.Posts = blog.Posts ?? new List<Post>();

                blog.Posts.Add(post);
                context.SaveChanges();

                Console.WriteLine($"Post Count: {context.Posts.Count()}");
            }

            using (var context = new BloggingContext())
            {
                var blog = context.Blogs.Include(b => b.Posts).First();
                var post = blog.Posts.First();

                blog.Posts.Remove(post);
                context.SaveChanges();

                Console.WriteLine($"Post Count: {context.Posts.Count()}");
            }

            using (var db = new BloggingContext())
            {
                var blog = db.Blogs.Include(b => b.Posts).First();

                db.Remove(blog);
                db.SaveChanges();

                Console.WriteLine($"Post Count: {db.Posts.Count()}");
            }

            using (var db = new BloggingContext())
            {
                var blog = db.Blogs.First();
                db.Remove(blog);
                db.SaveChanges();
            }

            #endregion

            #region Concurrency

            // Ensure database is created and has a person in it
            using (var context = new PersonContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                context.People.Add(new Person { FirstName = "John", LastName = "Doe" });
                context.SaveChanges();
            }

            using (var context = new PersonContext())
            {
                // Fetch a person from database and change phone number
                var person = context.People.Single(p => p.PersonId == 1);
                person.PhoneNumber = "555-555-5555";

                // Change the persons name in the database (will cause a concurrency conflict)
                context.Database.ExecuteSqlCommand("UPDATE dbo.People SET FirstName = 'Jane' WHERE PersonId = 1");

                try
                {
                    // Attempt to save changes to the database
                    context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    foreach (var entry in ex.Entries)
                    {
                        if (entry.Entity is Person)
                        {
                            // Using a NoTracking query means we get the entity but it is not tracked by the context
                            // and will not be merged with existing entities in the context.
                            var databaseEntity = context.People.AsNoTracking().Single(p => p.PersonId == ((Person)entry.Entity).PersonId);
                            var databaseEntry = context.Entry(databaseEntity);

                            foreach (var property in entry.Metadata.GetProperties())
                            {
                                var proposedValue = entry.Property(property.Name).CurrentValue;
                                var originalValue = entry.Property(property.Name).OriginalValue;
                                var databaseValue = databaseEntry.Property(property.Name).CurrentValue;

                                // TODO: Logic to decide which value should be written to database
                                // entry.Property(property.Name).CurrentValue = <value to be saved>;

                                // Update original values to
                                entry.Property(property.Name).OriginalValue = databaseEntry.Property(property.Name).CurrentValue;
                            }
                        }
                        else
                        {
                            throw new NotSupportedException("Don't know how to handle concurrency conflicts for " + entry.Metadata.Name);
                        }
                    }

                    // Retry the save operation
                    context.SaveChanges();
                }
            }

            #endregion

            #region Transactions

            using (var context = new BloggingContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    context.Blogs.Add(new Blog { Url = "http://blogs.msdn.com/dotnet" });
                    context.SaveChanges();

                    context.Blogs.Add(new Blog { Url = "http://blogs.msdn.com/visualstudio" });
                    context.SaveChanges();

                    var blogs = context.Blogs
                        .OrderBy(b => b.Url)
                        .ToList();

                    // Commit transaction if all commands succeed, transaction will auto-rollback
                    // when disposed if either commands fails
                    transaction.Commit();
                }
            }

            using (var context1 = new BloggingContext())
            {
                using (var transaction = context1.Database.BeginTransaction())
                {
                    context1.Blogs.Add(new Blog { Url = "http://blogs.msdn.com/dotnet" });
                    context1.SaveChanges();

                    using (var context2 = new BloggingContext(context1.Database.GetDbConnection()))
                    {
                        context2.Database.UseTransaction(transaction.GetDbTransaction());

                        var blogs = context2.Blogs
                            .OrderBy(b => b.Url)
                            .ToList();
                    }

                    // Commit transaction if all commands succeed, transaction will auto-rollback
                    // when disposed if either commands fails
                    transaction.Commit();
                }
            }

            var connection = new SqlConnection(BloggingContext.CS);
            connection.Open();

            using (var transaction = connection.BeginTransaction())
            {
                // Run raw ADO.NET command in the transaction
                var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = "DELETE FROM dbo.Blogs";
                command.ExecuteNonQuery();

                using (var context = new BloggingContext(connection))
                {
                    context.Database.UseTransaction(transaction);
                    context.Blogs.Add(new Blog { Url = "http://blogs.msdn.com/dotnet" });
                    context.SaveChanges();
                }

                // Commit transaction if all commands succeed, transaction will auto-rollback
                // when disposed if either commands fails
                transaction.Commit();
            }

            #endregion

            #region Async

            DoAsync().GetAwaiter().GetResult();

            #endregion

            #region Update

            using (var db = new EmployeeContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }

            using (var db = new EmployeeContext())
            {
                db.Employees.Add(new Employee { Name = "John Doe" });
                db.Employees.Add(new Employee { Name = "Jane Doe", EmploymentStarted = new DateTime(2000, 1, 1) });
                db.SaveChanges();

                foreach (var employee in db.Employees)
                {
                    Console.WriteLine(employee.EmployeeId + ": " + employee.Name + ", " + employee.EmploymentStarted);
                }
            }

            using (var db = new EmployeeContext())
            {
                db.Database.OpenConnection();
                try
                {
                    db.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.Employees ON");
                    db.SaveChanges();
                    db.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.Employees OFF");
                }
                finally
                {
                    db.Database.CloseConnection();
                }


                foreach (var employee in db.Employees)
                {
                    Console.WriteLine(employee.EmployeeId + ": " + employee.Name);
                }
            }

            using (var db = new EmployeeContext())
            {
                var john = db.Employees.Single(e => e.Name == "John Doe");
                john.Salary = 200;

                var jane = db.Employees.Single(e => e.Name == "Jane Doe");
                jane.Salary = 200;
                jane.LastPayRaise = DateTime.Today.AddDays(-7);

                db.SaveChanges();

                foreach (var employee in db.Employees)
                {
                    Console.WriteLine(employee.EmployeeId + ": " + employee.Name + ", " + employee.LastPayRaise);
                }
            }

            #endregion
        }

        private static async Task DoAsync()
        {
            using (var db = new BloggingContext())
            {
                var blog = new Blog { Url = "http://blogs.msdn.com/dotnet" };
                db.Blogs.Add(blog);
                await db.SaveChangesAsync();
            }
        }
    }
}
