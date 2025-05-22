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
        // �������� ��� ��������� ���� (���� �� ����) � �����
        private LinkedList<Media> mediaList = new LinkedList<Media>();
        // ��������� ���������� ��� ������������ ��� (���������, 159,99 ���)
        private readonly CultureInfo UkrainianCulture = CultureInfo.GetCultureInfo("uk-UA");
        // �������� UI: �������, ���� ����, ���� ������, �������
        private DataGridView grid;
        private ComboBox cmbType;
        private ComboBox cmbField;
        private TextBox txtCriterion;

        // �����������: ����������� UI, ������������ �����, ����������� ��� ����
        public MainForm()
        {
            InitializeUI(); // ��������� ����������� UI
            LoadFromDatabase(); // ������������ � SQLite
            ShowAllMedia(); // ³���������� ��� ��������
            // ������������ ������� �� ������������� ��� ������
            cmbType.SelectedIndex = 2; // "��"
            cmbField.SelectedIndex = 3; // "��� �������"
        }

        // ����������� UI: ��������� ������, �������, �������� ������
        private void InitializeUI()
        {
            Text = "������� ����"; // ��������� �����
            Size = new Size(900, 700); // ����� �����

            // ����� ������ ��� �������� (���������, ���������, ����������, �����)
            var buttons = new[]
            {
                new Button { Text = "������ ����", Location = new Point(10, 10), Size = new Size(100, 30) },
                new Button { Text = "������ ����", Location = new Point(120, 10), Size = new Size(100, 30) },
                new Button { Text = "��������", Location = new Point(230, 10), Size = new Size(100, 30) },
                new Button { Text = "����������", Location = new Point(340, 10), Size = new Size(100, 30) },
                new Button { Text = "³��� �� �����", Location = new Point(450, 10), Size = new Size(100, 30) },
                new Button { Text = "������ �������", Location = new Point(560, 10), Size = new Size(120, 30) },
                new Button { Text = "��������� �������", Location = new Point(690, 10), Size = new Size(130, 30) }
            };

            // �������� ��������� ���� �� ������
            buttons[0].Click += (s, e) => AddAudio();
            buttons[1].Click += (s, e) => AddVideo();
            buttons[2].Click += (s, e) => RemoveMedia();
            buttons[3].Click += (s, e) => EditMedia();
            buttons[4].Click += (s, e) => ShowVideosByPrice();
            buttons[5].Click += (s, e) => ShowForwardOrder();
            buttons[6].Click += (s, e) => ShowBackwardOrder();

            // �������� ������: ���, ����, �������
            var lblType = new Label { Text = "���:", Location = new Point(10, 50), Size = new Size(50, 20) };
            cmbType = new ComboBox { Location = new Point(60, 50), Size = new Size(150, 20) };
            cmbType.Items.AddRange(new[] { "����", "³���", "��" }); // ���� ���� ����

            var lblField = new Label { Text = "����:", Location = new Point(220, 50), Size = new Size(50, 20) };
            cmbField = new ComboBox { Location = new Point(270, 50), Size = new Size(150, 20) };
            cmbField.Items.AddRange(new[] { "�����", "���������� (����)", "������� (����)", "��� �������" }); // ���� ���� ��� ������

            var lblCriterion = new Label { Text = "�������:", Location = new Point(430, 50), Size = new Size(60, 20) };
            txtCriterion = new TextBox { Location = new Point(490, 50), Size = new Size(150, 20) }; // ���� ��� �������� �������

            // ������ ������
            var btnSearch = new Button { Text = "�����", Location = new Point(650, 50), Size = new Size(100, 30) };
            btnSearch.Click += (s, e) => SearchMedia(cmbType.SelectedIndex, cmbField.SelectedIndex, txtCriterion.Text);

            // ������� ��� ����������� ����
            grid = new DataGridView
            {
                Location = new Point(10, 90),
                Size = new Size(860, 560),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, // ����������� ������ ��������
                ReadOnly = true, // ҳ���� ��� ���������
                ScrollBars = ScrollBars.Both // ������������� �� ����������� ���������
            };
            grid.DataBindingComplete += (s, e) => ConfigureGridColumns(); // ������������ �������� ���� �������� �����

            // ��������� ��� �������� �� �����
            Controls.AddRange(buttons);
            Controls.AddRange(new Control[] { lblType, cmbType, lblField, cmbField, lblCriterion, txtCriterion, btnSearch, grid });
        }

        // ������������ ����� �� SQLite � mediaList
        private void LoadFromDatabase()
        {
            try
            {
                using (var context = new MediaContext())
                {
                    context.Database.EnsureCreated(); // ��������� ����, ���� �� ����
                    mediaList.Clear();
                    mediaList = new LinkedList<Media>(context.Media.ToList()); // ������������ ��� ����
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"������� ������������ �����: {ex.Message}");
            }
        }

        // ���������� mediaList � SQLite
        private void SaveToDatabase()
        {
            try
            {
                using (var context = new MediaContext())
                {
                    context.Media.RemoveRange(context.Media); // ������� ����
                    context.Media.AddRange(mediaList); // ��������� ����� �����
                    context.SaveChanges(); // ���������� ���
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"������� ���������� �����: {ex.Message}");
            }
        }

        // ������������ ���� ��� DataGridView (�� ������ ����������� �������������� ��� �����)
        private object MapMediaToGrid(Media m)
        {
            var audio = m as Audio;
            var video = m as Video;
            return new
            {
                ��� = audio != null ? "����" : "³���",
                ��� = m.Code,
                ����� = m.Title,
                ������ = m.Format,
                г� = m.Year,
                ֳ�� = m.Price.ToString("N2", UkrainianCulture) + " ���", // ������������ ���� (���������, 159,99 ���)
                ����� = audio?.Author ?? "", // ������, ���� �� ����
                ���������� = audio?.Performer ?? "",
                ��������� = audio != null ? $"{audio.Duration / 60} �� {audio.Duration % 60} ���" : "", // ������������ ���������
                ������� = video?.Director ?? "", // ������, ���� �� ����
                ����� = video?.MainActor ?? "",
                ֳ��_�_������� = m.CalculateDiscountedPrice().ToString("N2", UkrainianCulture) + " ���"
            };
        }

        // ������������ �������� DataGridView (������, ��������)
        private void ConfigureGridColumns()
        {
            if (grid.Columns.Count == 0) return; // ������ �� �������, ���� ������� �� �� �������
            // ������� �� �������� ��������
            var columnWidths = new Dictionary<string, int>
            {
                { "���", 80 }, { "���", 80 }, { "�����", 120 }, { "������", 80 }, { "г�", 60 },
                { "ֳ��", 100 }, { "�����", 100 }, { "����������", 100 }, { "���������", 100 },
                { "�������", 100 }, { "�����", 100 }, { "ֳ��_�_�������", 120 }
            };
            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (columnWidths.TryGetValue(col.Name, out int width))
                {
                    col.MinimumWidth = width; // ������������ �������� ������
                    if (col.Name == "�������" || col.Name == "�����")
                        col.Visible = true; // ������� �������� ��� ����
                }
            }
        }

        // ����� ��� ��������� ����
        private void AddAudio()
        {
            var form = new Form { Text = "������ ����", Size = new Size(400, 400), StartPosition = FormStartPosition.CenterParent };
            // ���� ��� �������� �����
            var fields = new Dictionary<string, TextBox>
            {
                { "���", new TextBox { Location = new Point(150, 20), Width = 200 } },
                { "�����", new TextBox { Location = new Point(150, 50), Width = 200 } },
                { "�����", new TextBox { Location = new Point(150, 80), Width = 200 } },
                { "����������", new TextBox { Location = new Point(150, 110), Width = 200 } },
                { "��������� (���)", new TextBox { Location = new Point(150, 140), Width = 200 } },
                { "������", new TextBox { Location = new Point(150, 170), Width = 200 } },
                { "г�", new TextBox { Location = new Point(150, 200), Width = 200 } },
                { "ֳ�� (���)", new TextBox { Location = new Point(150, 230), Width = 200 } }
            };
            // ��������� ���� ��� ����
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
            // ������ OK ��� ����������
            var btnOk = new Button { Text = "OK", Location = new Point(150, 260), Width = 200 };
            btnOk.Click += (s, e) =>
            {
                try
                {
                    // ��������: �� ���� ��������
                    if (fields.Any(f => string.IsNullOrWhiteSpace(f.Value.Text)))
                    {
                        MessageBox.Show("�������� �� ����!");
                        return;
                    }
                    string code = fields["���"].Text;
                    // �������� ���������� ����
                    if (mediaList.Any(m => m.Code == code))
                    {
                        MessageBox.Show("������� �� ����� ����� ��� ����!");
                        return;
                    }
                    // �������� ���������
                    if (!int.TryParse(fields["��������� (���)"].Text, out int duration) || duration <= 0)
                    {
                        MessageBox.Show("��������� �� ���� ���������� ������!");
                        return;
                    }
                    // �������� ����
                    if (!int.TryParse(fields["г�"].Text, out int year) || year < 1900 || year > DateTime.Now.Year)
                    {
                        MessageBox.Show($"г� �� ���� �� 1900 � {DateTime.Now.Year}!");
                        return;
                    }
                    // �������� ����
                    if (!double.TryParse(fields["ֳ�� (���)"].Text, NumberStyles.Any, UkrainianCulture, out double price) || price < 0)
                    {
                        MessageBox.Show("ֳ�� �� ���� ���䒺���� ������ (���������, 9,99)!");
                        return;
                    }
                    // ��������� ������ ����
                    var audio = new Audio(code, fields["�����"].Text, fields["�����"].Text, fields["����������"].Text,
                        duration, fields["������"].Text, year, price);
                    mediaList.AddLast(audio);
                    SaveToDatabase(); // ���������� � ����
                    ShowAllMedia(); // ��������� �������
                    form.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"�������: {ex.Message}");
                }
            };
            form.Controls.AddRange(fields.Values.ToArray()); // ����������� ValueCollection � ����� Control[]
            form.Controls.Add(btnOk);
            form.AcceptButton = btnOk; // OK ��� ��������� Enter
            form.ShowDialog();
        }

        // ����� ��� ��������� ���� (��������� ����, ��� ��� ���������)
        private void AddVideo()
        {
            var form = new Form { Text = "������ ����", Size = new Size(400, 350), StartPosition = FormStartPosition.CenterParent };
            var fields = new Dictionary<string, TextBox>
            {
                { "���", new TextBox { Location = new Point(150, 20), Width = 200 } },
                { "�����", new TextBox { Location = new Point(150, 50), Width = 200 } },
                { "�������", new TextBox { Location = new Point(150, 80), Width = 200 } },
                { "�������� �����", new TextBox { Location = new Point(150, 110), Width = 200 } },
                { "������", new TextBox { Location = new Point(150, 140), Width = 200 } },
                { "г�", new TextBox { Location = new Point(150, 170), Width = 200 } },
                { "ֳ�� (���)", new TextBox { Location = new Point(150, 200), Width = 200 } }
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
                        MessageBox.Show("�������� �� ����!");
                        return;
                    }
                    string code = fields["���"].Text;
                    if (mediaList.Any(m => m.Code == code))
                    {
                        MessageBox.Show("������� �� ����� ����� ��� ����!");
                        return;
                    }
                    if (!int.TryParse(fields["г�"].Text, out int year) || year < 1900 || year > DateTime.Now.Year)
                    {
                        MessageBox.Show($"г� �� ���� �� 1900 � {DateTime.Now.Year}!");
                        return;
                    }
                    if (!double.TryParse(fields["ֳ�� (���)"].Text, NumberStyles.Any, UkrainianCulture, out double price) || price < 0)
                    {
                        MessageBox.Show("ֳ�� �� ���� ���䒺���� ������ (���������, 9,99)!");
                        return;
                    }
                    var video = new Video(code, fields["�����"].Text, fields["�������"].Text, fields["�������� �����"].Text,
                        fields["������"].Text, year, price);
                    mediaList.AddLast(video);
                    SaveToDatabase();
                    ShowAllMedia();
                    form.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"�������: {ex.Message}");
                }
            };
            form.Controls.AddRange(fields.Values.ToArray()); // ����������� ValueCollection � ����� Control[]
            form.Controls.Add(btnOk);
            form.AcceptButton = btnOk;
            form.ShowDialog();
        }

        // ��������� ���� �� �����
        private void RemoveMedia()
        {
            // �������� ���� ����� �������� ����
            string code = InputDialog.Show("�������� �������", "������ ��� ��������:");
            if (string.IsNullOrWhiteSpace(code)) return;
            // ����� ���� �� �����
            var node = mediaList.FirstOrDefault(m => m.Code == code);
            if (node == null)
            {
                MessageBox.Show("������� �� ��������!");
                return;
            }
            mediaList.Remove(node); // ��������� � ��������
            SaveToDatabase(); // ��������� ����
            ShowAllMedia(); // ��������� �������
        }

        // ����������� ���� (���� �� ����������/�������)
        private void EditMedia()
        {
            string code = InputDialog.Show("���������� �������", "������ ��� ��������:");
            if (string.IsNullOrWhiteSpace(code)) return;
            var media = mediaList.FirstOrDefault(m => m.Code == code);
            if (media == null)
            {
                MessageBox.Show("������� �� ��������!");
                return;
            }
            // ����������� ����� ��� ���� �� ����
            var form = new Form { Text = media is Audio ? "���������� ����" : "���������� ����", Size = new Size(300, 150), StartPosition = FormStartPosition.CenterParent };
            var lblPrice = new Label { Text = "ֳ�� (���):", Location = new Point(10, 20), Size = new Size(100, 20) };
            var txtPrice = new TextBox { Location = new Point(110, 20), Width = 160, Text = media.Price.ToString("N2", UkrainianCulture) };
            var lblField = new Label { Text = media is Audio ? "����������:" : "�������:", Location = new Point(10, 50), Size = new Size(100, 20) };
            var txtField = new TextBox { Location = new Point(110, 50), Width = 160, Text = media is Audio audio ? audio.Performer : ((Video)media).Director };
            var btnOk = new Button { Text = "OK", Location = new Point(110, 80), Width = 160 };
            btnOk.Click += (s, e) =>
            {
                try
                {
                    // �������� ����
                    double price = 0; // ����������� �� �������������
                    if (!string.IsNullOrEmpty(txtPrice.Text) &&
                        !double.TryParse(txtPrice.Text, NumberStyles.Any, UkrainianCulture, out price) || price < 0)
                    {
                        MessageBox.Show("ֳ�� �� ���� ���䒺���� ������ (���������, 9,99)!");
                        return;
                    }
                    media.Price = price;
                    // ��������� ��������� (����) ��� �������� (����)
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
                    MessageBox.Show($"�������: {ex.Message}");
                }
            };
            form.Controls.AddRange(new Control[] { lblPrice, txtPrice, lblField, txtField, btnOk });
            form.AcceptButton = btnOk;
            form.ShowDialog();
        }

        // ³���������� ��� ���� � �������
        private void ShowAllMedia()
        {
            if (!mediaList.Any())
            {
                grid.DataSource = null; // ������� �������, ���� ����� ����
                return;
            }
            grid.DataSource = mediaList.Select(MapMediaToGrid).ToList(); // ������������ �� �����������
        }

        // ³���������� ����, ������������ �� �����
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

        // ������ ������� (��������� ShowAllMedia)
        private void ShowForwardOrder()
        {
            ShowAllMedia();
        }

        // ��������� �������
        private void ShowBackwardOrder()
        {
            if (!mediaList.Any())
            {
                grid.DataSource = null;
                return;
            }
            grid.DataSource = mediaList.Reverse().Select(MapMediaToGrid).ToList();
        }

        // ����� ���� �� �����, �����, �������
        private void SearchMedia(int typeIndex, int fieldIndex, string criterion)
        {
            // ���������� ���� ����������
            bool filterByAudio = typeIndex == 0;
            bool filterByVideo = typeIndex == 1;
            bool showAll = typeIndex == 2;
            if (!filterByAudio && !filterByVideo && !showAll)
                return;

            var results = mediaList.AsEnumerable(); // ��������� ��������
            // Գ�������� �� �����
            if (filterByAudio)
                results = results.OfType<Audio>();
            else if (filterByVideo)
                results = results.OfType<Video>();

            // Գ�������� �� �������
            if (!string.IsNullOrEmpty(criterion))
            {
                criterion = criterion.ToLower(); // ����������� �������
                switch (fieldIndex)
                {
                    case 0: // ����� �� ������
                        results = results.Where(m => m.Title.ToLower().Contains(criterion));
                        break;
                    case 1: // ����� �� ���������� (���� ����)
                        if (filterByAudio || showAll)
                            results = results.OfType<Audio>().Where(a => a.Performer.ToLower().Contains(criterion));
                        else
                            return;
                        break;
                    case 2: // ����� �� ��������� (���� ����)
                        if (filterByVideo || showAll)
                            results = results.OfType<Video>().Where(v => v.Director.ToLower().Contains(criterion));
                        else
                            return;
                        break;
                    case 3: // ����� �� ��� �����
                        results = results.Where(m => FilterByAllFields(m, criterion));
                        break;
                }
            }

            // ³���������� ����������
            grid.DataSource = results.Any() ? results.Select(MapMediaToGrid).ToList() : null;
        }

        // Գ�������� �� ��� ����� ��� ������ "��� �������"
        private bool FilterByAllFields(Media m, string criterion)
        {
            // �������� �����
            if (m.Title.ToLower().Contains(criterion))
                return true;
            // �������� ���� ����
            if (m is Audio audio && (audio.Performer.ToLower().Contains(criterion) || audio.Author.ToLower().Contains(criterion)))
                return true;
            // �������� ���� ����
            if (m is Video video && (video.Director.ToLower().Contains(criterion) || video.MainActor.ToLower().Contains(criterion)))
                return true;
            return false;
        }
    }
}