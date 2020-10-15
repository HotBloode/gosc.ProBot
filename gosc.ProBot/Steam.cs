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
        //Логрование в txt
        Loger loger;

        //Список сообщений
        private List<String> statusList = new List<string>()
            {
           /*0 */ "Авторизация в Steam успешна",
           /*1 */ "Загрузка Печенек",
           /*2 */ "Подгрузка Steam Печенек успешна",
           /*3 */ "Попытка авторизация в Steam помощи Печенек",
           /*4 */ "Печеньки устарели. Введите логин, пароль и актуальный код для повторой авторизации",
           /*5 */ "Работа над авторизацией",
           /*6 */ "Авторизация в Steam не удалась. Проверьте лог, пасс и убедитесь в актуальном коде аутентификации",
           /*7 */ "Сохраняем Печеньки",
           /*8 */ "Файл с Печеньками Steam не найден. Авторизуйтесь.",
           /*9 */ "Файл с Печеньками Steam пустой",
           /*10 */ "Введите логин, пароль и актуальный код для повторой авторизации",
            };
        
        private IWebDriver wd;

        public Steam(IWebDriver inDrive, Loger loger)
        {
            //Инициализация логера и wd
            this.loger = loger;
            wd = inDrive;            
        }         

        //Авторизация в Steam по логину, пассу и коду
        public bool AuthorizationWithLogPass(string login, string password, string autcode)
        {
            //Переходим на стим
            wd.Navigate().GoToUrl("https://steamcommunity.com/login");

            loger.WrireLog(statusList[5]);

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

                //Устали, спип
                Thread.Sleep(2000);
            }
            catch
            {
                loger.WrireLog(statusList[6]);

                return false;
            }                
            
            //Вытаскиваем из Драйвера печеньки
            var cookiesFromWd = wd.Manage().Cookies.AllCookies;

            List<TmpCookie> cookieList =  new List<TmpCookie>();
            
            //Пихаем в список нужные нам печеньки
            foreach(var tmp in cookiesFromWd)
            {
                //Закостылили свой класс печенек, ибо родной работает криво
                TmpCookie t = new TmpCookie(tmp.Name,tmp.Value,tmp.Domain,tmp.Path,tmp.Expiry);

                cookieList.Add(t);
            }
            //Сохраняем печеньки на будущее
            File.WriteAllText("Cookes.json", JsonConvert.SerializeObject(cookieList, Formatting.Indented));

            loger.WrireLog(statusList[7]);

            return true;
        }

        //Попытка авторизоваться в стиме при помощи печенек
        public bool AuthorizationWithCookie()
        {
            loger.WrireLog(statusList[3]);

            //Проверяем существование файла с Печеньками
            if (!File.Exists("Cookes.json"))
            {
                loger.WrireLog(statusList[8]);

                return false;
            }
            else
            {
                loger.WrireLog(statusList[1]);

                //Грузим печеньки
                List<TmpCookie> n = JsonConvert.DeserializeObject<List<TmpCookie>>(File.ReadAllText("Cookes.json"));

                //Проверяем пустоту файла с печерьками
                if (n == null || n.Count == 0)
                {

                    loger.WrireLog(statusList[9]);
                    loger.WrireLog(statusList[10]);

                    return false;
                }

                loger.WrireLog(statusList[2]);

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

                //Чуть чуть устали и спим
                Thread.Sleep(2000);


                IWebElement elemForCheck;
                //Проверяем авторизацию по наличию кнопки
                try
                {
                    elemForCheck = wd.FindElement(By.Id("account_pulldown"));
                    loger.WrireLog(statusList[0]);
                }
                catch
                {
                    //нЭма кнопки
                    loger.WrireLog(statusList[4]);

                    return false;
                }

                //Есть кнопка
                return true;
            }
        }

    }
}