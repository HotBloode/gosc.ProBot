using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using static gosc.ProBot.MainWindow;
using System.Windows.Controls;

namespace gosc.ProBot
{
    class Steam
    {
        private List<String> statusList = new List<string>()
            {
            "Авторизация в Steam успешна",
            "Загрузка Печенек",
            "Подгрузка Steam Печенек успешна",
            "Попытка авторизация в Steam помощи Печенек",
            "Печеньки устарели. Введите логин, пароль и актуальный код для повторой авторизации",
            "Работа над авторизацией",
            "Авторизация в Steam не удалась. Проверьте лог, пасс и убедитесь в актуальном коде аутентификации",
            "Сохраняем Печеньки",
            "Файл с Печеньками Steam не найден. Авторизуйтесь.",
            "Файл с Печеньками Steam пустой",
            "Введите логин, пароль и актуальный код для повторой авторизации",
            };

        private void outStatus(int statusCode)
        {
            frontStatusBlock.Text += "\n" + statusList[statusCode];
        }

        private TextBlock frontStatusBlock;
        private IWebDriver wd;
        public Steam(IWebDriver inDrive, TextBlock frontStatusBlock)
        {
            wd = inDrive;
            this.frontStatusBlock = frontStatusBlock;
        }         

        public bool AuthorizationWithLogPass(string login, string password, string autcode)
        {
            //Переходим на стим
            wd.Navigate().GoToUrl("https://steamcommunity.com/login");
            outStatus(5);
            //Ищем поля логина, пароля и заполняем их
            var log = wd.FindElement(By.Id("steamAccountName"));
            var pas = wd.FindElement(By.Id("steamPassword"));
            log.SendKeys(login);
            pas.SendKeys(password);

            //Ищем кнопку и нажимаем на неё
            var batt = wd.FindElement(By.Id("SteamLogin"));
            batt.Click();
            Thread.Sleep(2000);

            //Ищем поле вводаждя кода и заполняем
            var code = wd.FindElement(By.Id("twofactorcode_entry"));
            code.SendKeys(autcode);

            try
            {
                //Ищем блок с кнопками и выбираемм нужную нам для клика
                IWebElement butDiv = wd.FindElement(By.Id("login_twofactorauth_buttonset_entercode"));
                var x = butDiv.FindElement(By.ClassName("auth_button_h3"));
                x.Click();
                Thread.Sleep(2000);
            }
            catch
            {
                outStatus(6);
                return false;
            }                
            

            var cookiesFromWd = wd.Manage().Cookies.AllCookies;
            List<TmpCookie> cookieList =  new List<TmpCookie>();
            foreach(var tmp in cookiesFromWd)
            {
                TmpCookie t = new TmpCookie(tmp.Name,tmp.Value,tmp.Domain,tmp.Path,tmp.Expiry);
                cookieList.Add(t);
            }
            File.WriteAllText("Cookes.json", JsonConvert.SerializeObject(cookieList, Formatting.Indented));

            outStatus(7);
            return true;
        }

        public bool AuthorizationWithCookie()
        {
            outStatus(3);
            if (!File.Exists("Cookes.json"))
            {
                outStatus(8);
                return false;
            }
            else
            {
                outStatus(1);
                //Грузим печеньки
                List<TmpCookie> n = JsonConvert.DeserializeObject<List<TmpCookie>>(File.ReadAllText("Cookes.json"));

                if (n == null || n.Count == 0)
                {
                    outStatus(9);
                    outStatus(10);
                    return false;
                }

                outStatus(2);

                //Переходим в стим
                wd.Navigate().GoToUrl("https://steamcommunity.com");                
                
                //Пушим печеньки в драйвер
                foreach (var c in n)
                {
                    Cookie newCo = new Cookie(c.Name, c.Value, c.Domain, c.Path, c.Expiry);
                    wd.Manage().Cookies.AddCookie(newCo);
                }

                //И снова переходим в стим
                wd.Navigate().GoToUrl("https://steamcommunity.com");
                Thread.Sleep(2000);

                IWebElement elemForCheck;
                try
                {
                    elemForCheck = wd.FindElement(By.Id("account_pulldown"));                    
                    outStatus(0);
                }
                catch
                {
                    outStatus(4);
                    return false;
                }
                return true;
            }
        }

    }
}