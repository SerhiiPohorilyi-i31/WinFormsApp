using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WinFormsApp
{
    // Контекст для роботи з базою даних SQLite
    public class MediaContext : DbContext
    {
        // Колекція всіх медіа (аудіо та відео)
        public DbSet<Media> Media { get; set; }

        // Налаштування підключення до SQLite
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=media.db");
        }

        // Налаштування моделі для спадкування
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Налаштування Table Per Hierarchy (TPH) для класу Media
            modelBuilder.Entity<Media>()
                .HasDiscriminator<string>("MediaType") // Дискримінатор для визначення типу
                .HasValue<Audio>("Audio") // Записи Audio матимуть MediaType = "Audio"
                .HasValue<Video>("Video"); // Записи Video матимуть MediaType = "Video"
        }
    }
}