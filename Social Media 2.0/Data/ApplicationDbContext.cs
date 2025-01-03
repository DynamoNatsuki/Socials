﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Social_Media_2._0.Models;

namespace Social_Media_2._0.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Broadcast> Broadcasts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // One-to-many between ApplicationUser and Broadcast with restrictive cascade
            modelBuilder.Entity<Broadcast>()
                .HasOne(b => b.User)
                .WithMany(u => u.Broadcasts)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevents cascading deletion

            // Many-to-many between ApplicationUser and Followers/ListeningTo
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.ListeningTo)
                .WithMany(u => u.Followers)
                .UsingEntity(j => j.ToTable("UserFollowers"));

            // Many-to-many for Broadcast Likes
            modelBuilder.Entity<Broadcast>()
                .HasMany(b => b.Likes)
                .WithMany()
                .UsingEntity(j => j.ToTable("BroadcastLikes"));
        }
    }
}
