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

        public delegate void ParsHandler();
        public event ParsHandler Notify;

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
        void LUrl()
        {
            startPage = Convert.ToInt32(File.ReadAllText("Url.json"));
        }

        public async void ParsePage()
        {
            while (true)
            {
                try
                {
                    LUrl();
                    globalFlag = false;
                    url = baseUrl + startPage;
                    //Получаем html страницу
                    var source = await GetHtmlPage();
                    var domParser = new HtmlParser();
                    var doc = await domParser.ParseDocumentAsync(source);
                    var list = new List<string>();
                    //Ищем последнюю страницу с кодами на форуме
                    lastPage = Convert.ToInt32(doc.QuerySelector("input[max]").Attributes[5].Value);

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
                    MessageBox.Show($"Ошибка c интернет подключением! Проверьте подключение. Парсер перезапустится через указанную вами паузу!");
                    //Усыпляем поток
                    Thread.Sleep(timer);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Ошибка (Сообщите о ней разработчику). Программа перезапустится через указанную вами паузу>> {e.Message} {e.GetType()}");
                    //Усыпляем поток
                    Thread.Sleep(timer);
                }
            }

        }

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

        async Task<bool> ParseCode()
        {
            //Флаг новизны
            bool flagAdd = false;
            var source = await GetHtmlPage();//Получаем HTML код

            var domParser = new HtmlParser();

            var doc = await domParser.ParseDocumentAsync(source);

            var list = new List<string>();

            //Парсим посты со стр.
            var items = doc.QuerySelectorAll(".message-main");
            foreach (var item in items)
            {
                list.Add(item.OuterHtml.Substring(10));
            }
            //Регулярка под коды
            string x = @"(([A-Z]|\d){7,8}\s*(<br>|</div|</b>|\n))";
            Regex regex = new Regex(x);

            MatchCollection matches;

            foreach (var mes in list)
            {
                matches = regex.Matches(mes);

                //Парсим регуляркой
                foreach (var match in matches)
                {
                    //Удаляем пробелы и всякие скобочки, а так же чекаем дубликаты
                    if (CheckDublicate(match.ToString().Split('<')[0].Replace("\n", "")))
                    {
                        //Добавляем в базу
                        db.Add(new Repoz(match.ToString().Split('<')[0].Replace("\n", ""), false));
                        //Поднимаем флаг уникальности
                        flagAdd = true;
                    }
                }
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
