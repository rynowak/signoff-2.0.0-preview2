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

        public DbSet<SignoffApp.Models.Person> Person { get; set; }

        //public DbSet<Blog> Blogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Blog>().HasOne(e => e.MyBlog).WithOne(e => e.InverseBlog).HasForeignKey<Blog>(e => e.Id);
            base.OnModelCreating(modelBuilder);
        }
    }
}
