using System.Windows;

namespace Supervision.Styles.ProjectStyles
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class ComboBoxStyles : ResourceDictionary
    {
        private void ComboBox_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            e.Handled = !((System.Windows.Controls.ComboBox)sender).IsDropDownOpen;
        }
    }
}