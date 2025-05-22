using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace WinFormsApp
{
    /// <summary>
    /// Статичний клас, що містить точку входу для програми.
    /// Відповідає за ініціалізацію та запуск Windows Forms програми.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// Головна точка входу програми.
        /// Налаштовує візуальні стилі, запускає головну форму та обробляє критичні помилки.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Увімкнення сучасних візуальних стилів для елементів керування Windows Forms
            Application.EnableVisualStyles();

            // Налаштування режиму рендерингу тексту (false для сумісності з GDI+)
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Запуск головної форми програми (MainForm)
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                // Обробка будь-яких неперехоплених винятків, що можуть виникнути під час виконання програми
                // Виведення детальної інформації про помилку (повідомлення та стек викликів) у модальному вікні
                MessageBox.Show(
                    $"Критична помилка програми: {ex.Message}\nStackTrace: {ex.StackTrace}",
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}