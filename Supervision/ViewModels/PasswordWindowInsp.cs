using System;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows;

using Supervision.ViewModels;
using Supervision.Views.EntityViews;
using DataLayer;

namespace Supervision.Views
{
    public partial class PasswordWindowInsp : Form
    {
        MainViewModel mainView;

        public PasswordWindowInsp(MainViewModel mainView)
        {
            InitializeComponent();
            InitializeMyControl();
            this.mainView = mainView;
        }

        private void InitializeMyControl()
        {
            // Set to no text.
            textBox1.Text = "";
            // The password character is an asterisk.
            textBox1.PasswordChar = '*';
            // The control will allow no more than 14 characters.
            textBox1.MaxLength = 20;
        }



        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Equals("пензаперсонал20"))
            {
                mainView.InspKey = true;
                var w = new InspectorView();
                w.DataContext = InspectorVM.LoadVM(new DataContext());
                if (MainViewModel.HelpMode == true)
                {
                    System.Windows.MessageBox.Show("Раздел содержащий редактируемый список инженеров. Нажимая на названия столбцов, можно осуществлять сортировку списка. " +
                        "Данный список выводится в поле \"Инженер\" таблиц операций.\n\n" +
                        "Кнопка \"Добавить\" создаст готовую для заполнения строку в списке.", "Раздел \"Персонал\"");
                }
                this.Hide();
            }
            else
            {
                System.Windows.MessageBox.Show("Пароль неверный!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
