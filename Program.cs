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
    /// ��������� ����, �� ������ ����� ����� ��� ��������.
    /// ³������ �� ����������� �� ������ Windows Forms ��������.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// ������� ����� ����� ��������.
        /// ��������� ������� ����, ������� ������� ����� �� �������� ������� �������.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // ��������� �������� ��������� ����� ��� �������� ��������� Windows Forms
            Application.EnableVisualStyles();

            // ������������ ������ ���������� ������ (false ��� �������� � GDI+)
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // ������ ������� ����� �������� (MainForm)
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                // ������� ����-���� �������������� �������, �� ������ ��������� �� ��� ��������� ��������
                // ��������� �������� ���������� ��� ������� (����������� �� ���� �������) � ���������� ���
                MessageBox.Show(
                    $"�������� ������� ��������: {ex.Message}\nStackTrace: {ex.StackTrace}",
                    "�������",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}