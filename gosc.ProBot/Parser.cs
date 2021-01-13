using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace gosc.ProBot
{
    class Parser
    {

        FlagSinhron flag;

        public delegate void ParsHandler();
        public event ParsHandler Notify;
       

        //Логрование в txt
        Loger loger;

        private List<String> statusList = new List<string>()
        {
            "Ошибка c интернет подключением! Проверьте подключение. Парсер перезапустится через 7 минут!",
            "Ошибка (Сообщите о ней разработчику). Программа перезапустится через 7 минут>> ",
            "Файла со стартовой страницей нет!",
            "Файла с Печеньками пустой!",
            "Найдено новых кодов: "
        };

        public Parser(FlagSinhron flag, Loger loger)
        {
            this.flag = flag;
            this.loger = loger;            
        }

        //Полная urд парса
        string url;
        //Переменная таймера
        public int timer = 420000;
        //Базовая ссылка на форум
        string baseUrl = "https://miped.ru/f/threads/promokody-dlja-sajta-gocs-pro-obschaja-tema.67618/page-";
        //Список с кодами
        public List<Repoz> db;

        bool globalFlag = false;
        //Начальная страница для парса
        int lastPage;
        //Конечная страница для парса
        int startPage;

        //Подгрузка последней страници котоорую парсили
        bool LUrl()
        {
            //Если Файл не существует или пустой то false
            if(!File.Exists("Url.json"))
            {
                loger.WrireLog(statusList[2]);
                return false;
            }
            else
            {
                if(File.ReadAllText("Url.json")== "")
                {
                    loger.WrireLog(statusList[3]);
                    return false;
                }
                else
                {
                    startPage = Convert.ToInt32(File.ReadAllText("Url.json"));
                    return true;
                }  
            }            
        }

        public async void ParsePage()
        {            
            while (true)
            {
                if(!flag.Value)
                {

                try
                {                    
                    globalFlag = false;
                    url = baseUrl + 1;
                    //Получаем html страницу
                    var source = await GetHtmlPage();
                    var domParser = new HtmlParser();
                    var doc = await domParser.ParseDocumentAsync(source);
                    var list = new List<string>();

                    //Ищем последнюю страницу с кодами на форуме
                    lastPage = Convert.ToInt32(doc.QuerySelector("input[max]").Attributes[5].Value);

                     //Подгружаем последнюю страницу которую спарсили  
                     //Если файла нет или он пустой, то стартовую страницу берём 10ю с края!
                     if(!LUrl())
                     {
                         startPage = lastPage - 10;
                         loger.WrireLog("Стартовая страница 10я c края: " + startPage +". Край: "+ lastPage);
                     }

                    //Перебираем все стр. от начальной до последней
                    for (int i = startPage; i <= lastPage; i++)
                    {
                        //Формируем полную ссылку для парса
                        url = baseUrl + i;

                        //Вызываем функцию парса. Если были новые коды, то возвращает true
                        bool flag = await ParseCode();

                        //Проверка флага "новизны"
                        if (flag)
                        {
                            //Поднятие главного флага, если за 1 подход парса есть новые коды
                            globalFlag = true;
                        }
                    }

                    
                    //Записывем номер последней пропарсеной стр. в файл
                    File.WriteAllText("Url.json", Convert.ToString(lastPage));

                    //Если  гл. флаг поднят
                    if (globalFlag)
                    {
                        Notify?.Invoke();
                    }

                    Thread.Sleep(timer);
                }
                catch (HttpRequestException e)
                {
                        loger.WrireLog("Сообщение с ошибкой: " + e.Message);
                        loger.WrireLog("Тип ошибки: " + e.GetType());
                        loger.WrireLog("Ссылка на решение: " + e.HelpLink);
                        loger.WrireLog("Имя решения: " + e.Source);
                        loger.WrireLog("Метод вернувший исключение: " + e.TargetSite);

                        loger.WrireLog(statusList[0]);

                        //Усыпляем поток
                        Thread.Sleep(timer);
                }
                catch (Exception e)
                {
                        loger.WrireLog("Сообщение с ошибкой: "+e.Message);
                        loger.WrireLog("Тип ошибки: "+e.GetType());
                        loger.WrireLog("Ссылка на решение: "+ e.HelpLink);
                        loger.WrireLog("Имя решения: "+e.Source);
                        loger.WrireLog("Метод вернувший исключение: " + e.TargetSite);


                        loger.WrireLog(statusList[1]);
                        //Усыпляем поток
                        Thread.Sleep(timer);
                }
                }
                else
                {
                    Thread.Sleep(60000);
                }
            }

        }

        //Прлучение HTML стр.
        async Task<string> GetHtmlPage()
        {
            //Код стр.
            string source = null;

            //КЛиент
            HttpClient client = new HttpClient();

            //Скачиваем стр.
            var respose = await client.GetAsync(url);
            if (respose != null && respose.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //Сохраняем и отдаём стр.
                source = await respose.Content.ReadAsStringAsync();
            }

            return source;
        }

        //Ф-ия самого парсера
        async Task<bool> ParseCode()
        {
            int count = 0;

            //Флаг новизны
            bool flagAdd = false;
            var source = await GetHtmlPage();//Получаем HTML код

            var domParser = new HtmlParser();

            var doc = await domParser.ParseDocumentAsync(source);

            var list = new List<string>();

            //Парсим посты со стр.
            var items = doc.QuerySelectorAll(".bbWrapper");

           
            foreach (var item in items)
            {
                list.Add(item.OuterHtml.Substring(10));
            }
            //Регулярка под коды
            string x = @"(([A-Z]|\d){7,8}\s*(<br>|</div|</b>|\n|\s|,))";
            Regex regex = new Regex(x);

            MatchCollection matches;

            foreach (var mes in list)
            {
                matches = regex.Matches(mes);
                Console.WriteLine(matches.Count);
                //Парсим регуляркой
                foreach (var match in matches)
                {                    
                   
                    string code = ((match.ToString().Split('<')[0].Replace("\n", "")).Replace(",", "")).Replace(" ", "");
                    //Удаляем пробелы и всякие скобочки, а так же чекаем дубликаты
                    if (CheckDublicate(code))
                    {
                        
                        //Добавляем в базу
                        db.Add(new Repoz(code, false));
                        count++;
                        //Поднимаем флаг уникальности
                        flagAdd = true;
                    }
                }
            }
            if(count!=0)
            {
                loger.WrireLog(statusList[4] + count);
            }
            
            return flagAdd;
        }

        //Проверка кодов на дубликаты в списке
        bool CheckDublicate(string code)
        {
            for (int i = 0; i < db.Count; i++)
            {
                if (db[i].Code == code)
                {
                    return false;
                }
            }
            return true;
        }


    }
}
