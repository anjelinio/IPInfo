using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Text;

namespace IPInfo.Persistent
{
    public class IPDetailsDataContext : DbContext
    {
        public IPDetailsDataContext() : base() { }

        public IPDetailsDataContext(DbContextOptions<IPDetailsDataContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(!optionsBuilder.IsConfigured)
            { 
                optionsBuilder.UseSqlServer();    
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<IPDetails>().HasKey(prop => prop.Ip);
        }

        public DbSet<IPDetails> Details { get; set; }
    }
}
