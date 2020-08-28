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
        static FlagSinhron flag = new FlagSinhron(false);
       

        private static bool authorizationFlag = true;
        private IWebDriver wd;
        private TextBlock frontStatusBlock;
        static Parser p;
       
        public List<Repoz> db = new List<Repoz>();
        
        static Thread myThread;
        

        static GoCsPro goscpro;
        Steam steam;

        public Controller(TextBlock frontStatusBlock)
        {
            this.frontStatusBlock = frontStatusBlock;
            p = new Parser(flag, this.frontStatusBlock);


            var driverService = ChromeDriverService.CreateDefaultService();            
            driverService.HideCommandPromptWindow = true;
            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--disable-notifications");           
            wd = new ChromeDriver(driverService, options);
           

            p.db = db;
            p.Notify += s;


            p.timer = 7 * 60000;

            goscpro = new GoCsPro(wd, frontStatusBlock, db, flag);
            steam  = new Steam(wd, frontStatusBlock);
            
        }
        

        public void Start(string log, string pass, string code)
        {
            if (authorizationFlag)
            {               
                if (goscpro.AuthorizationWithCookie())
                {
                    myThread = new Thread(new ThreadStart(p.ParsePage));
                    myThread.Start();
                }
                else
                {
                    
                    if (steam.AuthorizationWithCookie())
                    {
                       if(goscpro.SteamAuthorization())
                        {
                            myThread = new Thread(new ThreadStart(p.ParsePage));
                            myThread.Start();
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
                if (log == "" || pass == "" || code == "" || log == null || pass == null || code == null)
                {
                    frontStatusBlock.Text += "\n" + "Введие корректные данные дя авторизации";
                }
                else
                {
                    if (steam.AuthorizationWithLogPass(log, pass, code))
                    {
                        if (goscpro.SteamAuthorization())
                        {
                            myThread = new Thread(new ThreadStart(p.ParsePage));
                            myThread.Start();
                        }
                    }
                }
            }
        }
        void s()
        {
            Thread myThread1 = new Thread(new ThreadStart(goscpro.b));
            myThread1.Start();
        }

        public void Exit()
        {
            wd.Close();
            wd.Quit();           
        }
    }
}
