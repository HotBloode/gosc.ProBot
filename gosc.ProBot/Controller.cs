using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace gosc.ProBot
{
    class Controller
    {
        private static bool authorizationFlag = true;
        private IWebDriver wd;
        private TextBlock frontStatusBlock;
        static Parser p = new Parser();
        public List<Repoz> db = new List<Repoz>();
        
        static Thread myThread = new Thread(new ThreadStart(p.ParsePage));
        static Thread myThread1;

        static GoCsPro goscpro;
        Steam steam;

        public Controller(TextBlock frontStatusBlock)
        {
            this.frontStatusBlock = frontStatusBlock;


            var driverService = ChromeDriverService.CreateDefaultService();            
            driverService.HideCommandPromptWindow = true;
            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--disable-notifications");
            wd = new ChromeDriver(driverService, options);

            p.db = db;
            p.Notify += s;


            p.timer = 7 * 60000;

            goscpro = new GoCsPro(wd, frontStatusBlock, db);
            steam  = new Steam(wd, frontStatusBlock);
            myThread1 = new Thread(new ThreadStart(goscpro.b));
        }
        

        public void Start(string log, string pass, string code)
        {
            if (authorizationFlag)
            {
               
                if (goscpro.AuthorizationWithCookie())
                {
                    myThread.Start();
                }
                else
                {
                    
                    if (steam.AuthorizationWithCookie())
                    {
                        goscpro.SteamAuthorization();
                        //
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
                        goscpro.SteamAuthorization();
                        //
                    }
                }
            }
        }
        void s()
        {
           myThread1.Start();
        }

        public void Exit()
        {
            wd.Close();
            wd.Quit();           
        }
    }
}
