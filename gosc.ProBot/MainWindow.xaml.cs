using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace gosc.ProBot
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Steam steamAcc = new Steam(statusBlock); ;
            steamAcc.Authorization(pass.Text, login.Text, codeT.Text);

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            RsaParameters rsaParam = new RsaParameters
            {
                Exponent = t1.Text,
                Modulus = t2.Text,
                Password = pass.Text
            };
            // var encrypted = steamAcc.EncryptPassword(rsaParam);
            // Console.WriteLine(encrypted);
        }
    }
}
