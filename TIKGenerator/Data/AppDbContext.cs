using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TIKGenerator.Models;

namespace TIKGenerator.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Signal> Signals { get; set; }
        public DbSet<SignalGroup> SignalGroups { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SignalGroup>()
                .HasMany(g => g.Signals)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
