using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PatientsParser.Models;

namespace PatientsParser.DAL
{
    public partial class PatientsContext : DbContext
    {
        public virtual DbSet<Patients> Patients { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // use the DB connection provided as an environment variable 
            // or default to the hardcoded value
            optionsBuilder.UseSqlServer(
                Environment.GetEnvironmentVariable("DB_CONNECTION") ??
                @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Patients;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patients>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(e => e.Id).HasColumnName("Patient_ID");

                entity.Property(e => e.Age).HasColumnName("Age");

                entity.Property(e => e.FirstName)
                    .HasColumnName("FirstName ")
                    .HasColumnType("nvarchar(50)");

                entity.Property(e => e.LastName)
                    .HasColumnName("LastName ")
                    .HasColumnType("nvarchar(50)");
            });
        }
    }
}