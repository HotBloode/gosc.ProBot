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
        
        private IWebDriver wd;
        public List<Repoz> db;


        //Логрование в txt
        Loger loger;

        public GoCsPro(IWebDriver inDrive,List<Repoz> db, FlagSinhron flag, Loger loger )
        {
            //Инициаоизируем логер, wd, БД и Флаг синхронизации
            this.loger = loger;            
            wd = inDrive;            
            this.db = db;            
            this.flag = flag;
        }

        //Список сообщений
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

        //
        public bool SteamAuthorization()
        {
            loger.WrireLog(statusList[5]);
            
            wd.Navigate().GoToUrl("https://gocs5.pro/");
            wd.FindElement(By.ClassName("auth__logo")).Click();
            wd.FindElement(By.ClassName("modal__btn")).Click();

            try
            {
                IWebElement elemForCheck = wd.FindElement(By.ClassName("OpenID_loggedInAccount"));
                loger.WrireLog(statusList[7] += elemForCheck.Text);                             
            }
            catch
            {
                loger.WrireLog(statusList[6]);
                return false;
            }

            wd.FindElement(By.Id("imageLogin")).Click();

            IWebElement elemForCheck1 = wd.FindElement(By.ClassName("header-account__name"));
            loger.WrireLog(statusList[3] += elemForCheck1.Text);           


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
            loger.WrireLog(statusList[8]);

            return true;
        }

        //Ф-ия авторизации при помощи печенек сайта
        public bool AuthorizationWithCookie()
        {
            loger.WrireLog(statusList[0]);

            //Если файла с Печеньками вообще нет, то идём на ***
            if (!File.Exists("CookesGoCsPro.json"))
            {
                loger.WrireLog(statusList[1]);
                return false;
            }
            else
            {
                //Если файл есть, то пытаемся вытащить данные из него
                List<TmpCookie> n = JsonConvert.DeserializeObject<List<TmpCookie>>(File.ReadAllText("CookesGoCsPro.json"));

                //Проверка файла на пустоту
                if (n == null|| n.Count == 0)
                {
                    loger.WrireLog(statusList[2]);
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
                    
                    //Проверяем появилось ли имя пользователя и делаем выводы об авторизации
                    try
                    {
                        IWebElement elemForCheck = wd.FindElement(By.ClassName("header-account__name"));
                        loger.WrireLog(statusList[3] += elemForCheck.Text);              
                        return true;
                    }
                    catch
                    {
                        loger.WrireLog(statusList[4]);
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
                            loger.WrireLog(statusList[10] + db[i].Code);
                            //frontStatusBlock.Dispatcher.Invoke(new Action(() => frontStatusBlock.Text += db[i].Code));

                            var x = wd.FindElement(By.ClassName("fortune__btn-wrapper"));
                            x.Click();

                            Thread.Sleep(2000);
                            db[i].flag = true;
                        }
                        else
                        {                           
                            loger.WrireLog(statusList[9] + db[i].Code);

                            //frontStatusBlock.Dispatcher.Invoke(new Action(() => frontStatusBlock.Text += db[i].Code));
                            Thread.Sleep(2000);
                            db[i].flag = true;
                        }
                    }
                    else if (wd.PageSource.Contains("fortune__btn-wrapper"))
                    {
                        loger.WrireLog(statusList[11]);                        
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
