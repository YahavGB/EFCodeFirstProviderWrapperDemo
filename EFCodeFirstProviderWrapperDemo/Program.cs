using System;
using System.IO;
using System.Linq;
using EFProviderWrapperToolkit;
using EFTracingProvider;
using EFCachingProvider.Caching;
using EFCachingProvider;
using System.Data.Entity;

namespace EFCodeFirstProviderWrapperDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            //System.Data.Entity.Database.SetInitializer<SampleDbContext>(new SampleDbContext.SampleDbContextInitializationStrategy());
            System.Data.Entity.Database.SetInitializer<SampleDbContext>(null);

            /* Note: Unlike the original examples - EFTracing and EFCaching are being registered in
             * ExtendedDbContext. So there's no need to register them here. */

            SimpleCachingDemo();

            Console.ReadKey();
        }

        /// <summary>
        /// In this demo we are running a set of queries 3 times and logging SQL commands to the console.
        /// Note that queries are actually executed only in the first pass, while in second and third they are fulfilled
        /// completely from the cache.
        /// </summary>
        private static void SimpleCachingDemo()
        {
            ICache cache = new InMemoryCache();
            CachingPolicy cachingPolicy = CachingPolicy.CacheAll;

            // log SQL from all connections to the console
            // I'm commenting this line because this option was set to true
            // in case of "Debug mode" in ExtendedDbContext
            //EFTracingProviderConfiguration.LogToConsole = true;

            for (int i = 0; i < 3; ++i)
            {
                Console.WriteLine();
                Console.WriteLine("*** Pass #{0}...", i);
                Console.WriteLine();
                using (var context = new SampleDbContext())
                {
                    // set up caching
                    context.Cache = cache;
                    context.CachingPolicy = cachingPolicy;

                    Console.WriteLine("Loading account...");
                    var account = (from a in context.Accounts.Include(p => p.Posts)
                                   where a.FullName == "Mai Johnston"
                                   select a).First();

                    Console.WriteLine("Account name: {0}", account.FullName);
                    Console.WriteLine("Order count: {0}", account.Posts.Count);
                }
            }

            Console.WriteLine();

            //Console.WriteLine("*** Cache statistics: Hits:{0} Misses:{1} Hit ratio:{2}% Adds:{3} Invalidations:{4}",
            //    cache.CacheHits,
            //    cache.CacheMisses,
            //    100.0 * cache.CacheHits / (cache.CacheHits + cache.CacheMisses),
            //    cache.CacheItemAdds,
            //    cache.CacheItemInvalidations);
        }

        /// <summary>
        /// In this demo we are running a set of queries and updates and 3 times and logging SQL commands to the console.
        /// Notice how performing an update on Customer table causes the cache entry to be invalidated so we get 
        /// a query in each pass. Because we aren't modifying OrderDetails table, the collection of order details
        /// for the customer doesn't require a query in second and third pass.
        /// </summary>
        private static void CacheInvalidationDemo()
        {
            var cache = new InMemoryCache();

            for (int i = 0; i < 3; ++i)
            {
                Console.WriteLine();
                Console.WriteLine("*** Pass #{0}...", i);
                Console.WriteLine();
                using (var context = new SampleDbContext())
                {
                    // set up caching
                    context.Cache = cache;
                    context.CachingPolicy = CachingPolicy.CacheAll;

                    Console.WriteLine("Loading account...");
                    var account = context.Accounts.Include(a => a.Posts).First(a => a.FullName == "Josh Smith");
                    Console.WriteLine("Account name: {0}", account.FullName);

                    account.FullName = "Change" + Environment.TickCount;

                    for (int o = 0; o < 10; ++o)
                    {
                        var post = new Post()
                        {
                            PostIdentifier = Guid.NewGuid(),
                            PostTitle = "Post" + o,
                            PostMessage = "Post " + o + " message"
                        };

                        account.Posts.Add(post);
                    }

                    Console.WriteLine("Posts count: {0}", account.Posts.Count);
                    context.SaveChanges();
                }
            }

            Console.WriteLine();
        }

    }
}
