using mshtml;
using System;
using System.Windows;
using System.Windows.Navigation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

namespace gosc.ProBot
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool InternetGetCookieEx(
        string url,
        string cookieName,
        StringBuilder cookieData,
        ref int size,
        Int32 dwFlags,
        IntPtr lpReserved);
        static bool flag = false;

        private const Int32 InternetCookieHttponly = 0x2000;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Steam steamAcc = new Steam(statusBlock);
            steamAcc.b = wb;

            dynamic activeX = wb.GetType().InvokeMember("ActiveXInstance",
                    BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, wb, new object[] { });

            activeX.Silent = true;




            steamAcc.Authorization(PassBox.Text, LogBox.Text, CodeBox.Text);

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Steam steamAcc = new Steam(statusBlock);
            RsaParameters rsaParam = new RsaParameters
            {
                Exponent = t1.Text,
                Modulus = t2.Text,
                Password = PassBox.Text
            };
            var encrypted = steamAcc.EncryptPassword(rsaParam);
             Console.WriteLine(encrypted);
        }

        private void MyBrowser_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            var document = (IHTMLDocument3)wb.Document;
            
            if (document.documentElement.outerHTML.Contains("Сообщество Steam"))
            {
                document.getElementById("imageLogin").click();
            }
            else
            {               
                if (!flag)
                {
                    wb.Navigate("https://gocs5.pro");
                    flag = true;
                }
                else
                {
                    List <Cookie> li = new List<Cookie>();
                    Uri x = new Uri("https://gocs5.pro");
                    var a = GetUriCookieContainer(x);                   
                    var b = a.GetCookies(x).Cast<Cookie>();

                    Cookie c;
                    object z;
                    foreach (var t in b)
                    {
                        c = new Cookie();
                        c.Comment = t.Comment;
                        c.CommentUri = t.CommentUri;
                        c.HttpOnly = t.HttpOnly;
                        c.Discard = t.Discard;
                        c.Domain = t.Domain;
                        c.Expired = t.Expired;
                        c.Expires = t.Expires;
                        c.Name = t.Name;
                        c.Path = t.Path;
                        c.Port = t.Port;
                        c.Secure = t.Secure;
                        c.Value = t.Value;
                        c.Version = t.Version;
                        li.Add(c);
                    }

                    File.WriteAllText("Cookes1.json", JsonConvert.SerializeObject(li, Newtonsoft.Json.Formatting.Indented));



                }


            } 
        }

        public static CookieContainer GetUriCookieContainer(Uri uri)
        {
            CookieContainer cookies = null;
            // Determine the size of the cookie
            int datasize = 8192 * 16;
            StringBuilder cookieData = new StringBuilder(datasize);
            if (!InternetGetCookieEx(uri.ToString(), null, cookieData, ref datasize, InternetCookieHttponly, IntPtr.Zero))
            {
                if (datasize < 0)
                    return null;
                // Allocate stringbuilder large enough to hold the cookie
                cookieData = new StringBuilder(datasize);
                if (!InternetGetCookieEx(
                    uri.ToString(),
                    null, cookieData,
                    ref datasize,
                    InternetCookieHttponly,
                    IntPtr.Zero))
                    return null;
            }
            if (cookieData.Length > 0)
            {
                cookies = new CookieContainer();
                cookies.SetCookies(uri, cookieData.ToString().Replace(';', ','));
            }
            return cookies;
        }
    }
}
