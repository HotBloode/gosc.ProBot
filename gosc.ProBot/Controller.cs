using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace gosc.ProBot
{
    class Controller
    {
        private static bool authorizationFlag = true;
        private IWebDriver wd;
        private TextBlock frontStatusBlock;
        public Controller(TextBlock frontStatusBlock)
        {
            this.frontStatusBlock = frontStatusBlock;


            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            
            wd = new ChromeDriver(driverService, new ChromeOptions());            
            
        }

        private List<String> statusList = new List<string>()
        {
            "Попытка авторизоваться на сайте через Печеньки",
            "Не удалось авторизоваться на сайте при помощи Печенек",
            "",
           
            ""
        };

        private void outStatus(int statusCode)
        {
            frontStatusBlock.Text += "\n" + statusList[statusCode];
        }

        public void Start(string log, string pass, string code)
        {
            Steam steam = new Steam(wd, frontStatusBlock);
            GoCsPro goscpro = new GoCsPro(wd, frontStatusBlock);

            if (authorizationFlag)
            {
               
                if (goscpro.AuthorizationWithCookie())
                {
                    //
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

        public void Exit()
        {
            wd.Close();
            wd.Quit();           
        }
    }
}
