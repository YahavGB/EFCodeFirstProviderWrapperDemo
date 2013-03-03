using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCodeFirstProviderWrapperDemo
{
    public class SampleDbContext : ExtendedDbContext
    {
        #region Db Tables

        public DbSet<Account> Accounts { get; set; }

        public DbSet<Post> Posts { get; set; }

        #endregion

        public SampleDbContext()
            : base("name=DefaultConnection", enableCaching: true, enableTracing: true, contextOwnsConnection: true)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            /* Define relations */
            modelBuilder.Entity<Account>().HasMany(a => a.Posts).WithRequired(p => p.PostAuthor).Map(m =>
            {
                m.MapKey("PostAuthorId");
            }).WillCascadeOnDelete(true);

            /* Base call */
            base.OnModelCreating(modelBuilder);
        }
        

        /** Using the seed method to add some sample data **/
        public class SampleDbContextInitializationStrategy : DropCreateDatabaseAlways<SampleDbContext>
        {
            protected override void Seed(SampleDbContext context)
            {
                /* Accounts */
                var account1 = new Account()
                {
                    FullName = "Josh Smith",
                    BirthDay = DateTime.Now.AddYears(-25),
                    Posts = new List<Post>()  
                };

                var account2 = new Account()
                {
                    FullName = "David Griffin",
                    BirthDay = DateTime.Now.AddYears(-35),
                    Posts = new List<Post>()
                };

                var account3 = new Account()
                {
                    FullName = "Mai Johnston",
                    BirthDay = DateTime.Now.AddYears(-20),
                    CompanyName = "Sampling Data, Inc.",
                    Posts = new List<Post>()
                };


                /* Posts */
                account1.Posts.Add(new Post()
                {
                    PostIdentifier = Guid.NewGuid(),
                    PostTitle = "Sample Post",
                    PostMessage = "Sample Post Content!"
                });

                account1.Posts.Add(new Post()
                {
                    PostIdentifier = Guid.NewGuid(),
                    PostTitle = "Sample Post 2",
                    PostMessage = "Sample Post Content 2!"
                });

                account3.Posts.Add(new Post()
                {
                    PostIdentifier = Guid.NewGuid(),
                    PostTitle = "Mai's Sample Post",
                    PostMessage = "Mai's Sample Post Content!"
                });
                
                account3.Posts.Add(new Post()
                {
                    PostIdentifier = Guid.NewGuid(),
                    PostTitle = "Mai's Sample Post 2",
                    PostMessage = "Mai's Sample Post Content 2!"
                });

                account3.Posts.Add(new Post()
                {
                    PostIdentifier = Guid.NewGuid(),
                    PostTitle = "Mai's Sample Post 3",
                    PostMessage = "Mai's Sample Post Content 3!"
                });

                /* Save */
                context.Accounts.Add(account1);
                context.Accounts.Add(account2);
                context.Accounts.Add(account3);
                context.SaveChanges();
            }
        }
    }
}
