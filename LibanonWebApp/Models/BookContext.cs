using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using LibanonWebApp.Models.EntitiesConfiguration;

namespace LibanonWebApp.Models
{
    public class BookContext : DbContext
    {
        public BookContext() :base("name=LibanonDBConnectionString")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<BookContext, LibanonWebApp.Migrations.Configuration>());
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<ISBN> ISBNs { get; set; }
        public DbSet<Owner> Owners { get; set; }
        public DbSet<Confirmation> Confirmations { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Configurations.Add(new BookEntityConfiguration());
        }
    }
}