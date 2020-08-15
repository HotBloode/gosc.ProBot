using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

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


            IWebDriver wd = new ChromeDriver();
            wd.Navigate().GoToUrl("https://steamcommunity.com/login");
            var log = wd.FindElement(By.Id("steamAccountName"));
            var pas = wd.FindElement(By.Id("steamPassword"));
            log.SendKeys("log");
            pas.SendKeys("pass");
            var batt = wd.FindElement(By.Id("SteamLogin"));
            batt.Click();

            Thread.Sleep(2000);
            var code = wd.FindElement(By.Id("twofactorcode_entry"));
            code.SendKeys("R6HY5");

            var butDiv = wd.FindElement(By.Id("login_twofactorauth_buttonset_entercode"));
            var x = butDiv.FindElement(By.ClassName("auth_button_h3"));
            x.Click();
            Thread.Sleep(2000);
            var cookies = wd.Manage().Cookies.AllCookies;

            wd.Close();

            IWebDriver wd1 = new ChromeDriver();

            wd1.Navigate().GoToUrl("https://steamcommunity.com");
            foreach (var c in cookies)
            {
                wd1.Manage().Cookies.AddCookie(c);
            }
            wd1.Navigate().GoToUrl("https://steamcommunity.com");

            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            
        }

        private void MyBrowser_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            
        }       

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }
    }
}
