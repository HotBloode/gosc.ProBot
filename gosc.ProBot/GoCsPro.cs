using Newtonsoft.Json;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static gosc.ProBot.MainWindow;

namespace gosc.ProBot
{
    class GoCsPro
    {
        FlagSinhron flag;
        private TextBlock frontStatusBlock;
        private IWebDriver wd;
        public List<Repoz> db;
        public GoCsPro(IWebDriver inDrive, TextBlock frontStatusBlock, List<Repoz> db, FlagSinhron flag)
        {
            wd = inDrive;
            this.frontStatusBlock = frontStatusBlock;
            this.db = db;
            this.flag = flag;
        }

        private List<String> statusList = new List<string>()
        {
            "Попытка авторизоваться на сайте через Печеньки",
            "Файл с Печеньками от сайта отсутствует",
            "Файл с Печеньками от сайта пуст",
            "Вход на сайт успешен. Вы вошли под именем: ",
            "Устаревшие Печеньки от сайта",
            "Попытка авторизоваться на сайте через Steam",
            "Нет авторизации в Steam",
            "Авторизация на сайте при помощи Steam аккаунта с логином: ",
            "Сохраняем Печеньки от сайта",
            "Не рабочий код: ",
            "Рабочий код: ",
            "Активировали код с прошлого раза. Как вариант, вы закрыли программу на этапе активации!"
        };

        private void outStatus(int statusCode)
        {            
            frontStatusBlock.Dispatcher.Invoke(new Action(() => frontStatusBlock.Text += "\n" + statusList[statusCode]));           
        }
        public bool SteamAuthorization()
        {
            
            outStatus(5);
            wd.Navigate().GoToUrl("https://gocs5.pro/");
            wd.FindElement(By.ClassName("auth__logo")).Click();
            wd.FindElement(By.ClassName("modal__btn")).Click();

            try
            {
                IWebElement elemForCheck = wd.FindElement(By.ClassName("OpenID_loggedInAccount"));
                outStatus(7);
                frontStatusBlock.Text += elemForCheck.Text;

                
            }
            catch
            {
                outStatus(6);
                return false;
            }

            wd.FindElement(By.Id("imageLogin")).Click();

            IWebElement elemForCheck1 = wd.FindElement(By.ClassName("header-account__name"));
            outStatus(3);
            frontStatusBlock.Text += elemForCheck1.Text;


            var cookiesFromWd = wd.Manage().Cookies.AllCookies;
            List<TmpCookie> cookieList = new List<TmpCookie>();

            foreach (var tmp in cookiesFromWd)
            {
                if (tmp.Domain == "gocs5.pro")
                {
                    TmpCookie t = new TmpCookie(tmp.Name, tmp.Value, tmp.Domain, tmp.Path, tmp.Expiry);
                    cookieList.Add(t);
                }
            }
            File.WriteAllText("CookesGoCsPro.json", JsonConvert.SerializeObject(cookieList, Formatting.Indented));
            outStatus(8);

           

            return true;
        }

        public bool AuthorizationWithCookie()
        {
            outStatus(0);
            if (!File.Exists("CookesGoCsPro.json"))
            {
                outStatus(1);
                return false;
            }
            else
            {
                List<TmpCookie> n = JsonConvert.DeserializeObject<List<TmpCookie>>(File.ReadAllText("CookesGoCsPro.json"));

                if (n == null|| n.Count == 0)
                {
                    outStatus(2);
                    return false;
                }               
                else
                {
                    //Переходим на сайт
                    wd.Navigate().GoToUrl("https://gocs5.pro/");

                    //Пушим печеньки в драйвер
                    foreach (var c in n)
                    {
                        Cookie newCo = new Cookie(c.Name, c.Value, c.Domain, c.Path, c.Expiry);
                        wd.Manage().Cookies.AddCookie(newCo);
                    }                   
                    //И снова переходим на сайт
                    wd.Navigate().GoToUrl("https://gocs5.pro/");
                    Thread.Sleep(2000);                    
                    
                    try
                    {
                        IWebElement elemForCheck = wd.FindElement(By.ClassName("header-account__name"));
                        outStatus(3);
                        frontStatusBlock.Text += elemForCheck.Text;
                        return true;
                    }
                    catch
                    {
                        outStatus(4);
                        return false;
                    }
                }
            }
        }

        public async void b()
        {
            flag.Value = true;
            for (int i = db.Count - 1; i >-1; i--)
            {                
                if (db[i].flag == true)
                {
                    break;
                }
                else
                {
                    wd.Navigate().GoToUrl("https://gocs5.pro/bonus/wheel");
                    if (wd.PageSource.Contains("wheel-code__input"))
                    {
                        var elem = wd.FindElement(By.ClassName("wheel-code__input"));
                        elem.SendKeys(db[i].Code);
                        var key = wd.FindElement(By.ClassName("wheel-code__btn"));
                        key.Click();
                        Thread.Sleep(3000);

                        if (wd.PageSource.Contains("fortune__btn-wrapper"))
                        {
                            Thread.Sleep(2000);

                            outStatus(10);
                            frontStatusBlock.Dispatcher.Invoke(new Action(() => frontStatusBlock.Text += db[i].Code));

                            var x = wd.FindElement(By.ClassName("fortune__btn-wrapper"));
                            x.Click();

                            Thread.Sleep(2000);
                            db[i].flag = true;
                        }
                        else
                        {                            
                            outStatus(9);
                            frontStatusBlock.Dispatcher.Invoke(new Action(() => frontStatusBlock.Text += db[i].Code));
                            Thread.Sleep(2000);
                            db[i].flag = true;
                        }
                    }
                    else if (wd.PageSource.Contains("fortune__btn-wrapper"))
                    {
                        outStatus(11);                        
                        var x = wd.FindElement(By.ClassName("fortune__btn-wrapper"));
                        x.Click();
                        i--;
                        Thread.Sleep(3000);
                    }
                }
            }
            flag.Value =false;
            
        }
    }
}
