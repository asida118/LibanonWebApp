using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace LibanonWebApp.Models.EntitiesConfiguration
{
    public class BookEntityConfiguration : EntityTypeConfiguration<Book>
    {
        public BookEntityConfiguration()
        {
            this.HasKey<int>(b => b.BookId);

            this.HasRequired(b => b.ISBN)
                .WithRequiredPrincipal(i => i.Book)
                .WillCascadeOnDelete(true);

            this.HasRequired(b => b.Owner)
                .WithRequiredPrincipal(o => o.Book)
                .WillCascadeOnDelete(true);

            this.HasRequired(b => b.Confirmation)
                .WithRequiredPrincipal(c => c.Book)
                .WillCascadeOnDelete(true);
        }
    }
}