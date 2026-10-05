using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EF_core_1
{
    internal class Context : DbContext
    {
        public Context()
        {
            // any query from database will not be tracked
            // ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        // DbSets
        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Branch> Branches { get; set; }

        // connection string through this function
        // btw this class that ends with Builder is one of the important design patterns
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Data source = localhost\SQLEXPRESS; initial catalog = Intake46Core; Integrated security = true; TrustServerCertificate=True");

            // optionsBuilder.LogTo(Log => Debug.WriteLine(Log));
            // optionsBuilder.UseLazyLoadingProxies(true);  // true by default

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Fluent API
            modelBuilder.Entity<Department>()
                .ToTable("Department"); //change column name

            modelBuilder.Entity<Department>()
                .Property(d => d.Name)
                .IsRequired(true); // required prop

            modelBuilder.Entity<Attendance>()
                .HasKey(k => new { k.EmployeeID, k.Date });

            modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.Deleted);

            modelBuilder.Entity<Department>()
                .Property<bool>("Deleted")
                .IsRequired(true)
                .HasDefaultValue(false);

            foreach (var item in modelBuilder.Model.GetEntityTypes())
            {
                modelBuilder.Entity(item.ClrType)
                    .Property<bool>("Deleted")
                    .IsRequired(true)
                    .HasDefaultValue(false);

                modelBuilder.Entity(item.ClrType)
                    .Property<DateTime>("CreatedDate")
                    .IsRequired(true)
                    .HasDefaultValueSql("GETDATE()");
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}
