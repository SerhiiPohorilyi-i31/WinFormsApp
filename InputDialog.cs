using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;

namespace WinFormsApp
{
    // Кастомна форма для введення тексту
    public class InputDialog : Form
    {
        private TextBox txtInput; // Поле для введення тексту
        private Button btnOk; // Кнопка підтвердження
        private Button btnCancel; // Кнопка скасування

        // Властивість для отримання введеного тексту
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string InputText { get; private set; }

        // Конструктор із параметрами заголовка та підказки
        public InputDialog(string title, string prompt)
        {
            InitializeComponents(title, prompt); // Налаштування елементів форми
        }

        // Налаштування елементів форми
        private void InitializeComponents(string title, string prompt)
        {
            // Налаштування форми
            this.Text = title;
            this.Size = new System.Drawing.Size(300, 150);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // Підказка для користувача
            var lblPrompt = new Label
            {
                Text = prompt,
                Location = new System.Drawing.Point(10, 20),
                Size = new System.Drawing.Size(260, 20)
            };

            // Текстове поле для введення
            txtInput = new TextBox
            {
                Location = new System.Drawing.Point(10, 40),
                Size = new System.Drawing.Size(260, 20)
            };

            // Кнопка "OK"
            btnOk = new Button
            {
                Text = "OK",
                Location = new System.Drawing.Point(110, 70),
                Size = new System.Drawing.Size(75, 30)
            };
            btnOk.Click += (s, e) =>
            {
                // Збереження введеного тексту та закриття форми
                InputText = txtInput.Text;
                DialogResult = DialogResult.OK;
                Close();
            };

            // Кнопка "Скасувати"
            btnCancel = new Button
            {
                Text = "Скасувати",
                Location = new System.Drawing.Point(195, 70),
                Size = new System.Drawing.Size(75, 30)
            };
            btnCancel.Click += (s, e) =>
            {
                // Скасування введення
                InputText = null;
                DialogResult = DialogResult.Cancel;
                Close();
            };

            // Додавання елементів на форму
            this.Controls.AddRange(new Control[] { lblPrompt, txtInput, btnOk, btnCancel });
            this.AcceptButton = btnOk; // OK при натисканні Enter
            this.CancelButton = btnCancel; // Скасування при натисканні Esc
        }

        // Статичний метод для відображення форми та отримання тексту
        public static string Show(string title, string prompt)
        {
            using (var dialog = new InputDialog(title, prompt))
            {
                return dialog.ShowDialog() == DialogResult.OK ? dialog.InputText : null;
            }
        }
    }
}