using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Drawing;

namespace WinFormsApp
{
    public partial class MainForm : Form
    {
        // Колекція для зберігання медіа (аудіо та відео) у пам’яті
        private LinkedList<Media> mediaList = new LinkedList<Media>();
        // Українська локалізація для форматування цін (наприклад, 159,99 грн)
        private readonly CultureInfo UkrainianCulture = CultureInfo.GetCultureInfo("uk-UA");
        // Елементи UI: таблиця, вибір типу, поля пошуку, критерій
        private DataGridView grid;
        private ComboBox cmbType;
        private ComboBox cmbField;
        private TextBox txtCriterion;

        // Конструктор: ініціалізація UI, завантаження даних, відображення всіх медіа
        public MainForm()
        {
            InitializeUI(); // Створення програмного UI
            LoadFromDatabase(); // Завантаження з SQLite
            ShowAllMedia(); // Відображення всіх матеріалів
            // Встановлення значень за замовчуванням для пошуку
            cmbType.SelectedIndex = 2; // "Усі"
            cmbField.SelectedIndex = 3; // "Без фільтра"
        }

        // Ініціалізація UI: створення кнопок, таблиці, елементів пошуку
        private void InitializeUI()
        {
            Text = "Каталог медіа"; // Заголовок форми
            Size = new Size(900, 700); // Розмір форми

            // Масив кнопок для операцій (додавання, видалення, сортування, пошук)
            var buttons = new[]
            {
                new Button { Text = "Додати аудіо", Location = new Point(10, 10), Size = new Size(100, 30) },
                new Button { Text = "Додати відео", Location = new Point(120, 10), Size = new Size(100, 30) },
                new Button { Text = "Видалити", Location = new Point(230, 10), Size = new Size(100, 30) },
                new Button { Text = "Редагувати", Location = new Point(340, 10), Size = new Size(100, 30) },
                new Button { Text = "Відео за ціною", Location = new Point(450, 10), Size = new Size(100, 30) },
                new Button { Text = "Прямий порядок", Location = new Point(560, 10), Size = new Size(120, 30) },
                new Button { Text = "Зворотний порядок", Location = new Point(690, 10), Size = new Size(130, 30) }
            };

            // Прив’язка обробників подій до кнопок
            buttons[0].Click += (s, e) => AddAudio();
            buttons[1].Click += (s, e) => AddVideo();
            buttons[2].Click += (s, e) => RemoveMedia();
            buttons[3].Click += (s, e) => EditMedia();
            buttons[4].Click += (s, e) => ShowVideosByPrice();
            buttons[5].Click += (s, e) => ShowForwardOrder();
            buttons[6].Click += (s, e) => ShowBackwardOrder();

            // Елементи пошуку: тип, поле, критерій
            var lblType = new Label { Text = "Тип:", Location = new Point(10, 50), Size = new Size(50, 20) };
            cmbType = new ComboBox { Location = new Point(60, 50), Size = new Size(150, 20) };
            cmbType.Items.AddRange(new[] { "Аудіо", "Відео", "Усі" }); // Вибір типу медіа

            var lblField = new Label { Text = "Поле:", Location = new Point(220, 50), Size = new Size(50, 20) };
            cmbField = new ComboBox { Location = new Point(270, 50), Size = new Size(150, 20) };
            cmbField.Items.AddRange(new[] { "Назва", "Виконавець (аудіо)", "Режисер (відео)", "Без фільтра" }); // Вибір поля для пошуку

            var lblCriterion = new Label { Text = "Критерій:", Location = new Point(430, 50), Size = new Size(60, 20) };
            txtCriterion = new TextBox { Location = new Point(490, 50), Size = new Size(150, 20) }; // Поле для введення критерію

            // Кнопка пошуку
            var btnSearch = new Button { Text = "Пошук", Location = new Point(650, 50), Size = new Size(100, 30) };
            btnSearch.Click += (s, e) => SearchMedia(cmbType.SelectedIndex, cmbField.SelectedIndex, txtCriterion.Text);

            // Таблиця для відображення медіа
            grid = new DataGridView
            {
                Location = new Point(10, 90),
                Size = new Size(860, 560),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, // Автоматична ширина стовпців
                ReadOnly = true, // Тільки для перегляду
                ScrollBars = ScrollBars.Both // Горизонтальна та вертикальна прокрутка
            };
            grid.DataBindingComplete += (s, e) => ConfigureGridColumns(); // Налаштування стовпців після прив’язки даних

            // Додавання всіх елементів на форму
            Controls.AddRange(buttons);
            Controls.AddRange(new Control[] { lblType, cmbType, lblField, cmbField, lblCriterion, txtCriterion, btnSearch, grid });
        }

        // Завантаження даних із SQLite у mediaList
        private void LoadFromDatabase()
        {
            try
            {
                using (var context = new MediaContext())
                {
                    context.Database.EnsureCreated(); // Створення бази, якщо не існує
                    mediaList.Clear();
                    mediaList = new LinkedList<Media>(context.Media.ToList()); // Завантаження всіх медіа
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження даних: {ex.Message}");
            }
        }

        // Збереження mediaList у SQLite
        private void SaveToDatabase()
        {
            try
            {
                using (var context = new MediaContext())
                {
                    context.Media.RemoveRange(context.Media); // Очистка бази
                    context.Media.AddRange(mediaList); // Додавання нових даних
                    context.SaveChanges(); // Збереження змін
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження даних: {ex.Message}");
            }
        }

        // Форматування медіа для DataGridView (усі методи відображення використовують цей метод)
        private object MapMediaToGrid(Media m)
        {
            var audio = m as Audio;
            var video = m as Video;
            return new
            {
                Тип = audio != null ? "Аудіо" : "Відео",
                Код = m.Code,
                Назва = m.Title,
                Формат = m.Format,
                Рік = m.Year,
                Ціна = m.Price.ToString("N2", UkrainianCulture) + " грн", // Форматування ціни (наприклад, 159,99 грн)
                Автор = audio?.Author ?? "", // Порожнє, якщо не аудіо
                Виконавець = audio?.Performer ?? "",
                Тривалість = audio != null ? $"{audio.Duration / 60} хв {audio.Duration % 60} сек" : "", // Форматування тривалості
                Режисер = video?.Director ?? "", // Порожнє, якщо не відео
                Актор = video?.MainActor ?? "",
                Ціна_зі_знижкою = m.CalculateDiscountedPrice().ToString("N2", UkrainianCulture) + " грн"
            };
        }

        // Налаштування стовпців DataGridView (ширина, видимість)
        private void ConfigureGridColumns()
        {
            if (grid.Columns.Count == 0) return; // Захист від помилок, якщо стовпці ще не створені
            // Словник із ширинами стовпців
            var columnWidths = new Dictionary<string, int>
            {
                { "Тип", 80 }, { "Код", 80 }, { "Назва", 120 }, { "Формат", 80 }, { "Рік", 60 },
                { "Ціна", 100 }, { "Автор", 100 }, { "Виконавець", 100 }, { "Тривалість", 100 },
                { "Режисер", 100 }, { "Актор", 100 }, { "Ціна_зі_знижкою", 120 }
            };
            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (columnWidths.TryGetValue(col.Name, out int width))
                {
                    col.MinimumWidth = width; // Встановлення мінімальної ширини
                    if (col.Name == "Режисер" || col.Name == "Актор")
                        col.Visible = true; // Гарантія видимості для відео
                }
            }
        }

        // Форма для додавання аудіо
        private void AddAudio()
        {
            var form = new Form { Text = "Додати аудіо", Size = new Size(400, 400), StartPosition = FormStartPosition.CenterParent };
            // Поля для введення даних
            var fields = new Dictionary<string, TextBox>
            {
                { "Код", new TextBox { Location = new Point(150, 20), Width = 200 } },
                { "Назва", new TextBox { Location = new Point(150, 50), Width = 200 } },
                { "Автор", new TextBox { Location = new Point(150, 80), Width = 200 } },
                { "Виконавець", new TextBox { Location = new Point(150, 110), Width = 200 } },
                { "Тривалість (сек)", new TextBox { Location = new Point(150, 140), Width = 200 } },
                { "Формат", new TextBox { Location = new Point(150, 170), Width = 200 } },
                { "Рік", new TextBox { Location = new Point(150, 200), Width = 200 } },
                { "Ціна (грн)", new TextBox { Location = new Point(150, 230), Width = 200 } }
            };
            // Додавання міток для полів
            int labelY = 20;
            foreach (var field in fields)
            {
                form.Controls.Add(new Label
                {
                    Text = field.Key + ":",
                    Location = new Point(20, labelY),
                    Size = new Size(120, 20),
                    TextAlign = ContentAlignment.MiddleRight
                });
                labelY += 30;
            }
            // Кнопка OK для збереження
            var btnOk = new Button { Text = "OK", Location = new Point(150, 260), Width = 200 };
            btnOk.Click += (s, e) =>
            {
                try
                {
                    // Валідація: усі поля заповнені
                    if (fields.Any(f => string.IsNullOrWhiteSpace(f.Value.Text)))
                    {
                        MessageBox.Show("Заповніть усі поля!");
                        return;
                    }
                    string code = fields["Код"].Text;
                    // Перевірка унікальності коду
                    if (mediaList.Any(m => m.Code == code))
                    {
                        MessageBox.Show("Матеріал із таким кодом уже існує!");
                        return;
                    }
                    // Валідація тривалості
                    if (!int.TryParse(fields["Тривалість (сек)"].Text, out int duration) || duration <= 0)
                    {
                        MessageBox.Show("Тривалість має бути позитивним числом!");
                        return;
                    }
                    // Валідація року
                    if (!int.TryParse(fields["Рік"].Text, out int year) || year < 1900 || year > DateTime.Now.Year)
                    {
                        MessageBox.Show($"Рік має бути між 1900 і {DateTime.Now.Year}!");
                        return;
                    }
                    // Валідація ціни
                    if (!double.TryParse(fields["Ціна (грн)"].Text, NumberStyles.Any, UkrainianCulture, out double price) || price < 0)
                    {
                        MessageBox.Show("Ціна має бути невід’ємним числом (наприклад, 9,99)!");
                        return;
                    }
                    // Створення нового аудіо
                    var audio = new Audio(code, fields["Назва"].Text, fields["Автор"].Text, fields["Виконавець"].Text,
                        duration, fields["Формат"].Text, year, price);
                    mediaList.AddLast(audio);
                    SaveToDatabase(); // Збереження в базу
                    ShowAllMedia(); // Оновлення таблиці
                    form.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}");
                }
            };
            form.Controls.AddRange(fields.Values.ToArray()); // Конвертація ValueCollection у масив Control[]
            form.Controls.Add(btnOk);
            form.AcceptButton = btnOk; // OK при натисканні Enter
            form.ShowDialog();
        }

        // Форма для додавання відео (аналогічно аудіо, але без тривалості)
        private void AddVideo()
        {
            var form = new Form { Text = "Додати відео", Size = new Size(400, 350), StartPosition = FormStartPosition.CenterParent };
            var fields = new Dictionary<string, TextBox>
            {
                { "Код", new TextBox { Location = new Point(150, 20), Width = 200 } },
                { "Назва", new TextBox { Location = new Point(150, 50), Width = 200 } },
                { "Режисер", new TextBox { Location = new Point(150, 80), Width = 200 } },
                { "Головний актор", new TextBox { Location = new Point(150, 110), Width = 200 } },
                { "Формат", new TextBox { Location = new Point(150, 140), Width = 200 } },
                { "Рік", new TextBox { Location = new Point(150, 170), Width = 200 } },
                { "Ціна (грн)", new TextBox { Location = new Point(150, 200), Width = 200 } }
            };
            int labelY = 20;
            foreach (var field in fields)
            {
                form.Controls.Add(new Label
                {
                    Text = field.Key + ":",
                    Location = new Point(20, labelY),
                    Size = new Size(120, 20),
                    TextAlign = ContentAlignment.MiddleRight
                });
                labelY += 30;
            }
            var btnOk = new Button { Text = "OK", Location = new Point(150, 230), Width = 200 };
            btnOk.Click += (s, e) =>
            {
                try
                {
                    if (fields.Any(f => string.IsNullOrWhiteSpace(f.Value.Text)))
                    {
                        MessageBox.Show("Заповніть усі поля!");
                        return;
                    }
                    string code = fields["Код"].Text;
                    if (mediaList.Any(m => m.Code == code))
                    {
                        MessageBox.Show("Матеріал із таким кодом уже існує!");
                        return;
                    }
                    if (!int.TryParse(fields["Рік"].Text, out int year) || year < 1900 || year > DateTime.Now.Year)
                    {
                        MessageBox.Show($"Рік має бути між 1900 і {DateTime.Now.Year}!");
                        return;
                    }
                    if (!double.TryParse(fields["Ціна (грн)"].Text, NumberStyles.Any, UkrainianCulture, out double price) || price < 0)
                    {
                        MessageBox.Show("Ціна має бути невід’ємним числом (наприклад, 9,99)!");
                        return;
                    }
                    var video = new Video(code, fields["Назва"].Text, fields["Режисер"].Text, fields["Головний актор"].Text,
                        fields["Формат"].Text, year, price);
                    mediaList.AddLast(video);
                    SaveToDatabase();
                    ShowAllMedia();
                    form.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}");
                }
            };
            form.Controls.AddRange(fields.Values.ToArray()); // Конвертація ValueCollection у масив Control[]
            form.Controls.Add(btnOk);
            form.AcceptButton = btnOk;
            form.ShowDialog();
        }

        // Видалення медіа за кодом
        private void RemoveMedia()
        {
            // Введення коду через діалогове вікно
            string code = InputDialog.Show("Видалити матеріал", "Введіть код матеріалу:");
            if (string.IsNullOrWhiteSpace(code)) return;
            // Пошук медіа за кодом
            var node = mediaList.FirstOrDefault(m => m.Code == code);
            if (node == null)
            {
                MessageBox.Show("Матеріал не знайдено!");
                return;
            }
            mediaList.Remove(node); // Видалення з колекції
            SaveToDatabase(); // Оновлення бази
            ShowAllMedia(); // Оновлення таблиці
        }

        // Редагування медіа (ціна та виконавець/режисер)
        private void EditMedia()
        {
            string code = InputDialog.Show("Редагувати матеріал", "Введіть код матеріалу:");
            if (string.IsNullOrWhiteSpace(code)) return;
            var media = mediaList.FirstOrDefault(m => m.Code == code);
            if (media == null)
            {
                MessageBox.Show("Матеріал не знайдено!");
                return;
            }
            // Універсальна форма для аудіо та відео
            var form = new Form { Text = media is Audio ? "Редагувати аудіо" : "Редагувати відео", Size = new Size(300, 150), StartPosition = FormStartPosition.CenterParent };
            var lblPrice = new Label { Text = "Ціна (грн):", Location = new Point(10, 20), Size = new Size(100, 20) };
            var txtPrice = new TextBox { Location = new Point(110, 20), Width = 160, Text = media.Price.ToString("N2", UkrainianCulture) };
            var lblField = new Label { Text = media is Audio ? "Виконавець:" : "Режисер:", Location = new Point(10, 50), Size = new Size(100, 20) };
            var txtField = new TextBox { Location = new Point(110, 50), Width = 160, Text = media is Audio audio ? audio.Performer : ((Video)media).Director };
            var btnOk = new Button { Text = "OK", Location = new Point(110, 80), Width = 160 };
            btnOk.Click += (s, e) =>
            {
                try
                {
                    // Валідація ціни
                    double price = 0; // Ініціалізація за замовчуванням
                    if (!string.IsNullOrEmpty(txtPrice.Text) &&
                        !double.TryParse(txtPrice.Text, NumberStyles.Any, UkrainianCulture, out price) || price < 0)
                    {
                        MessageBox.Show("Ціна має бути невід’ємним числом (наприклад, 9,99)!");
                        return;
                    }
                    media.Price = price;
                    // Оновлення виконавця (аудіо) або режисера (відео)
                    if (media is Audio audio && !string.IsNullOrEmpty(txtField.Text))
                        audio.Performer = txtField.Text;
                    else if (media is Video video && !string.IsNullOrEmpty(txtField.Text))
                        video.Director = txtField.Text;
                    SaveToDatabase();
                    ShowAllMedia();
                    form.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}");
                }
            };
            form.Controls.AddRange(new Control[] { lblPrice, txtPrice, lblField, txtField, btnOk });
            form.AcceptButton = btnOk;
            form.ShowDialog();
        }

        // Відображення всіх медіа в таблиці
        private void ShowAllMedia()
        {
            if (!mediaList.Any())
            {
                grid.DataSource = null; // Очистка таблиці, якщо даних немає
                return;
            }
            grid.DataSource = mediaList.Select(MapMediaToGrid).ToList(); // Форматування та відображення
        }

        // Відображення відео, відсортованих за ціною
        private void ShowVideosByPrice()
        {
            var videos = mediaList.OfType<Video>().OrderBy(v => v.Price).ToList();
            if (!videos.Any())
            {
                grid.DataSource = null;
                return;
            }
            grid.DataSource = videos.Select(MapMediaToGrid).ToList();
        }

        // Прямий порядок (аналогічно ShowAllMedia)
        private void ShowForwardOrder()
        {
            ShowAllMedia();
        }

        // Зворотний порядок
        private void ShowBackwardOrder()
        {
            if (!mediaList.Any())
            {
                grid.DataSource = null;
                return;
            }
            grid.DataSource = mediaList.Reverse().Select(MapMediaToGrid).ToList();
        }

        // Пошук медіа за типом, полем, критерієм
        private void SearchMedia(int typeIndex, int fieldIndex, string criterion)
        {
            // Визначення типу фільтрації
            bool filterByAudio = typeIndex == 0;
            bool filterByVideo = typeIndex == 1;
            bool showAll = typeIndex == 2;
            if (!filterByAudio && !filterByVideo && !showAll)
                return;

            var results = mediaList.AsEnumerable(); // Початкова колекція
            // Фільтрація за типом
            if (filterByAudio)
                results = results.OfType<Audio>();
            else if (filterByVideo)
                results = results.OfType<Video>();

            // Фільтрація за критерієм
            if (!string.IsNullOrEmpty(criterion))
            {
                criterion = criterion.ToLower(); // Ігнорування регістру
                switch (fieldIndex)
                {
                    case 0: // Пошук за назвою
                        results = results.Where(m => m.Title.ToLower().Contains(criterion));
                        break;
                    case 1: // Пошук за виконавцем (лише аудіо)
                        if (filterByAudio || showAll)
                            results = results.OfType<Audio>().Where(a => a.Performer.ToLower().Contains(criterion));
                        else
                            return;
                        break;
                    case 2: // Пошук за режисером (лише відео)
                        if (filterByVideo || showAll)
                            results = results.OfType<Video>().Where(v => v.Director.ToLower().Contains(criterion));
                        else
                            return;
                        break;
                    case 3: // Пошук по всіх полях
                        results = results.Where(m => FilterByAllFields(m, criterion));
                        break;
                }
            }

            // Відображення результатів
            grid.DataSource = results.Any() ? results.Select(MapMediaToGrid).ToList() : null;
        }

        // Фільтрація по всіх полях для пошуку "Без фільтра"
        private bool FilterByAllFields(Media m, string criterion)
        {
            // Перевірка назви
            if (m.Title.ToLower().Contains(criterion))
                return true;
            // Перевірка полів аудіо
            if (m is Audio audio && (audio.Performer.ToLower().Contains(criterion) || audio.Author.ToLower().Contains(criterion)))
                return true;
            // Перевірка полів відео
            if (m is Video video && (video.Director.ToLower().Contains(criterion) || video.MainActor.ToLower().Contains(criterion)))
                return true;
            return false;
        }
    }
}