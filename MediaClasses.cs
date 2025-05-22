using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace WinFormsApp
{
    // Абстрактний базовий клас для всіх медіа
    public abstract class Media
    {
        [Key] // Позначення Code як первинного ключа
        public string Code { get; set; } // Унікальний код
        public string Title { get; set; } // Назва
        public string Format { get; set; } // Формат (MP3, MP4 тощо)
        public int Year { get; set; } // Рік випуску
        public double Price { get; set; } // Ціна

        // Конструктор
        protected Media(string code, string title, string format, int year, double price)
        {
            Code = code;
            Title = title;
            Format = format;
            Year = year;
            Price = price;
        }

        // Віртуальний метод для обчислення знижкової ціни
        public virtual double CalculateDiscountedPrice()
        {
            return Price; // За замовчуванням без знижки
        }

        // Форматоване виведення інформації
        public override string ToString()
        {
            return $"Код: {Code}, Назва: {Title}, Формат: {Format}, Рік: {Year}, Ціна: {Price:F2}";
        }
    }

    // Похідний клас для аудіо записів
    public class Audio : Media
    {
        public string Author { get; set; } // Автор
        public string Performer { get; set; } // Виконавець
        public int Duration { get; set; } // Тривалість у секундах

        // Конструктор
        public Audio(string code, string title, string author, string performer, int duration, string format, int year, double price)
            : base(code, title, format, year, price)
        {
            Author = author;
            Performer = performer;
            Duration = duration;
        }

        // Знижка 10% для аудіо
        public override double CalculateDiscountedPrice()
        {
            return Price * 0.9;
        }

        // Форматоване виведення інформації про аудіо
        public override string ToString()
        {
            return $"{base.ToString()}, Автор: {Author}, Виконавець: {Performer}, Тривалість: {Duration / 60} хв {Duration % 60} сек, Знижкова ціна: {CalculateDiscountedPrice():F2}";
        }
    }

    // Похідний клас для відеоматеріалів
    public class Video : Media
    {
        public string Director { get; set; } // Режисер
        public string MainActor { get; set; } // Головний актор

        // Конструктор
        public Video(string code, string title, string director, string mainActor, string format, int year, double price)
            : base(code, title, format, year, price)
        {
            Director = director;
            MainActor = mainActor;
        }

        // Знижка 15% для відео
        public override double CalculateDiscountedPrice()
        {
            return Price * 0.85;
        }

        // Форматоване виведення інформації про відео
        public override string ToString()
        {
            return $"{base.ToString()}, Режисер: {Director}, Головний актор: {MainActor}, Знижкова ціна: {CalculateDiscountedPrice():F2}";
        }
    }
}
