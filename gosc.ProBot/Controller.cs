using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace gosc.ProBot
{
    class Controller
    {
        private List<String> statusList = new List<string>()
        {
            "Ложимся спать (режим сна на ПК) и останавливаем потоки!",
            "Просыпаемся после Сна!",
            "Введие корректные данные дя авторизации",
            "Нажатие кнопки старт!",
            "Выход из программы!"
        };

        //Флаг взаимодействия потока Парсера и потока Активатора кодов. Пока не проверяться все коды, парсер не будет работать.
        static FlagSinhron flag = new FlagSinhron(false);
       

        private static bool authorizationFlag = true;

        //WebDriver для работы с Chrome
        private IWebDriver wd;

        //Поле для информирования пользователя
        private TextBlock frontStatusBlock;

        static Parser parser;

        //List with codes
        public List<Repoz> db = new List<Repoz>();
        
        static Thread ThreadForParser;
        Thread ThreadForActivate;

        static GoCsPro goscpro;
        Steam steam;

        //Логрование в txt
        Loger loger;   


        public Controller(TextBlock frontStatusBlock)
        {
            //Инициализируем логер
            loger = new Loger(frontStatusBlock);

            //Соеденяем блок формы и код, для информирования
            this.frontStatusBlock = frontStatusBlock;

            //Инициализируем парсер
            parser = new Parser(flag, loger);
            
            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            ChromeOptions options = new ChromeOptions();

            //off notifications
            options.AddArguments("--disable-notifications");

            //hide Chrome
            options.AddArguments("--headless");

            //Options wd
            wd = new ChromeDriver(driverService, options);            

            //Конектим базу парсера с основной
            parser.db = db;

            //Навешиваем событие для начала активации найденых кодов
            parser.Notify += StartActivateCodes;

            //ТАЙЧЕР МЕЖДУ ПОИСКОМ КОДОВ. эТО МИНИМАЛЬНАЯ ЗЕДАРЖКА ЗА КОТОРУЮ MIPED НЕ БЛОЧИТ)
            parser.timer = 7 * 60000;

            goscpro = new GoCsPro(wd, db, flag, loger);
            steam  = new Steam(wd, loger);            
        }
        

        public void Start(string log, string pass, string code)
        {
            loger.WrireLog(statusList[3]);

            if (authorizationFlag)
            {        
                //ПРобуем печеньки сайта
                if (goscpro.AuthorizationWithCookie())
                {
                    //Если всё ок, то пихаем парсер в поток и запускаем его
                    ThreadForParser = new Thread(new ThreadStart(parser.ParsePage));
                    ThreadForParser.Start();
                }
                else
                {
                    //Пробуем печеньки стима
                    //Если всё ок, то пробуем авторизоваться на сайте
                    if (steam.AuthorizationWithCookie())
                    {
                       if(goscpro.SteamAuthorization())
                        {
                            //Если всё ок, то пихаем парсер в поток и запускаем его
                            ThreadForParser = new Thread(new ThreadStart(parser.ParsePage));
                            ThreadForParser.Start();
                        }
                    }
                    else
                    {
                        authorizationFlag = false;

                        return;
                    }
                }
            }
            else
            {
                //Проверяем на заполненость и пустоту строк
                if (log == "" || pass == "" || code == "" || log == null || pass == null || code == null)
                {
                    loger.WrireLog(statusList[2]);
                }
                else
                {
                    //Попытка авторизациии в стиме по логину, паролю и коду
                    if (steam.AuthorizationWithLogPass(log, pass, code))
                    {
                        //Теперь по печенькам
                        if (goscpro.SteamAuthorization())
                        {
                            //Если всё ок, то пихаем парсер в поток и запускаем его
                            ThreadForParser = new Thread(new ThreadStart(parser.ParsePage));
                            ThreadForParser.Start();
                        }
                    }
                }
            }
        }


        void StartActivateCodes()
        {
            //Т.К после завершения работы потока от сам закрывается, то каждый раз создаём новый
            ThreadForActivate = new Thread(new ThreadStart(goscpro.b));
            ThreadForActivate.Start();
        }


        public void OnOut()
        {
            loger.WrireLog(statusList[0]);
            ThreadForParser.Suspend();
            if (flag.Value)
            {
                ThreadForActivate.Suspend();
            }
        }

        public void OnIn()
        {
            loger.WrireLog(statusList[1]);
            ThreadForParser.Resume();
            if (flag.Value)
            {
                ThreadForActivate.Resume();
            }           
        }      

        public void Exit()
        {
            loger.WrireLog(statusList[4]);
            wd.Close();
            wd.Quit();           
        }
    }
}
