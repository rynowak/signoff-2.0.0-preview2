using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignoffApp.Models
{
    public class Blog
    {
        public int Id { get; set; }
        public Blog MyBlog { get; set; }
        public Blog InverseBlog { get; set; }
    }
}
