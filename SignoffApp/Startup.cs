using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SignoffApp.Models;

namespace SignoffApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddDbContext<SignoffAppContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("SignoffAppContext")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<SignoffAppContext>();

                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var famalies = new List<PersonFamily>
                {
                    new PersonFamily
                    {
                        LastName = "Garrison",
                    },
                    new PersonFamily
                    {
                        LastName = "Cartman",
                    },
                    new PersonFamily
                    {
                        LastName = "McCormick",
                    },
                    new PersonFamily
                    {
                        LastName = "Broflovski",
                    },
                    new PersonFamily
                    {
                        LastName = "Marsh",
                    },
                };
                var teachers = new List<PersonTeacher>
                {
                    new PersonTeacher {Name = "Ms. Frizzle"},
                    new PersonTeacher {Name = "Mr. Garrison", Family = famalies[0]},
                    new PersonTeacher {Name = "Mr. Hat", Family = famalies[0]},
                };
                var students = new List<PersonKid>
                {
                    new PersonKid {Name = "Arnold", Grade = 2, Teacher = teachers[0]},
                    new PersonKid {Name = "Phoebe", Grade = 2, Teacher = teachers[0]},
                    new PersonKid {Name = "Wanda", Grade = 2, Teacher = teachers[0]},

                    new PersonKid {Name = "Eric", Grade = 4, Teacher = teachers[1], Family = famalies[1]},
                    new PersonKid {Name = "Kenny", Grade = 4, Teacher = teachers[1], Family = famalies[2]},
                    new PersonKid {Name = "Kyle", Grade = 4, Teacher = teachers[1], Family = famalies[3]},
                    new PersonKid {Name = "Stan", Grade = 4, Teacher = teachers[1], Family = famalies[4]},
                };

                context.Person2.AddRange(teachers);
                context.Person2.AddRange(students);
                context.SaveChanges();

                var teachersTask = context.Person2.OfType<PersonTeacher>()
                    .Include(m => m.Students)
                    .ThenInclude(m => m.Family)
                    .ThenInclude(m => m.Members)
                    .ToListAsync();

                //teachersTask.Wait();
            }
        }
    }
}
