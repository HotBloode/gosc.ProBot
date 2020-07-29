using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Globalization;
using System.Windows.Controls;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Numerics;

namespace gosc.ProBot
{
    class Steam
    {
        List<String> statusList = new List<string>()
            {
            "Авторизация в Steam успешна",
            "Загрузка Печенек",
            "Подгрузка Steam печенек успешна",
            "Попытка авторизация при помощи Печенек",
            "Печеньки устарели. Введите логин, пароль и актуальный код для повторой авторизации",
            "Работа над авторизацией",
            "Авторизация в Steam не удалась. Проверьте лог, пасс и убедитесь в актуальном коде аутентификации",
            "Сохраняем Печеньки",
            "Файл с печеньками не найден. Авторизуйтесь.",
            "",
            "",
            };

        TextBlock statusBlock;
        List<Cookie> cookieList = new List<Cookie>();
        public static bool authorizationFlag = true;

        string Password;
        public string Login;
        public string Code;
        public HttpClient client;
        public string result;
        public HttpResponseMessage request;
        public RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        CookieContainer cookieContainer;
        HttpClientHandler msgHandler;


        public Steam(TextBlock frontStatusBlock)
        {

            statusBlock = frontStatusBlock;
            cookieContainer = new CookieContainer();
            msgHandler = new HttpClientHandler();
            msgHandler.CookieContainer = cookieContainer;
            client = new HttpClient(msgHandler);


            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("DNT", "1");
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            client.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.89 Safari/537.36");

            client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
            client.DefaultRequestHeaders.Add("donotcache", "155524323");
        }

        private async Task<bool> AuthorizationWithCookie()
        {
            //Вывод сообщения о работе
            outStatus(1);

            if (!File.Exists("Cookes.json"))
            {
                authorizationFlag = false;
                outStatus(8);
                return false;                
            }
            else
            {

                //Считывание файла с Печеньками в список и добавление его в контейнер с печеньками
                cookieList = JsonConvert.DeserializeObject<List<Cookie>>(File.ReadAllText("Cookes.json"));
                foreach (var v in cookieList)
                {
                    cookieContainer.Add(v);
                }

                //Вывод сообщения о работе
                outStatus(3);

                //Делаем запрос на стим, авось печеньки работают
                request = await client.GetAsync($"https://steamcommunity.com");
                //Получаем html страницу в ответ
                result = await request.Content.ReadAsStringAsync();

                //Проверяем на авторизацию (Проверяем если ли кнока выйти)
                if (result.Contains("Logout"))
                {
                    outStatus(2);
                    authorizationFlag = true;
                    return true;
                }
                else
                {
                    outStatus(4);
                    authorizationFlag = false;
                    return false;
                }
            }
            
        }

        public async void Authorization(string password, string login, string code)
        {
            if (authorizationFlag)
            {
                if (!await AuthorizationWithCookie())
                {
                    return;
                }
            }
            else
            {
                if (await AuthorizationWithLogPass(password, login, code))
                {
                    AuthorizationWithCookie();
                }
                else
                {
                    return;
                }
            }
        }
        private async Task<bool> AuthorizationWithLogPass(string password, string login, string code)
        {
            //Вывод сообщения о работе
            outStatus(5);

            //Получаем логин, парол и код
            Code = code;
            Password = password;
            Login = login;


            //Первый запрос стиму
            request = await client.GetAsync($"https://steamcommunity.com/login/getrsakey?username=" + Login);
            //Ответ от запроса
            result = await request.Content.ReadAsStringAsync();

            //Вытаскиваем из етвета ключи шифрования
            RsaKey rsaKey = JsonConvert.DeserializeObject<RsaKey>(result);

            //Заполняем объект класса ключами которые вытащили
            RsaParameters rsaParam = new RsaParameters
            {
                Exponent = rsaKey.publickey_exp,
                Modulus = rsaKey.publickey_mod,
                Password = Password
            };

            //Переменная для расшифровки пароля
            var encrypted = string.Empty;

            //Проверка на заполненость
            while (encrypted.Length < 2 || encrypted.Substring(encrypted.Length - 2) != "==")
            {
                //RSA шифрование пароля
                encrypted = EncryptPassword(rsaParam);
            }

            //Создание и заполнение библиотеки для отправки в следующем запросе
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("username", Login);
            data.Add("password", encrypted);
            data.Add("twofactorcode", Code);
            data.Add("emailauth", "");
            data.Add("loginfriendlyname", "");
            data.Add("captchagid", "-1");
            data.Add("captcha_text", "");
            data.Add("emailsteamid", "");
            data.Add("rsatimestamp", rsaKey.timestamp);
            data.Add("remember_login", "true");

            //Отправка 2го запроса с нужными данными
            request = await client.PostAsync($"https://steamcommunity.com/login/dologin/", new FormUrlEncodedContent(data));

            //Ответ
            result = await request.Content.ReadAsStringAsync();

            //Достаём результаты авторизации
            LoginResult loginResult = JsonConvert.DeserializeObject<LoginResult>(result);

            //Проверка флага авторизации в результатах
            if (loginResult.success)
            {

                //Вывод сообщения о работе
                outStatus(0);

                //Вытаскиваем нужные нам Печеньки
                IEnumerable<Cookie> responseCookies = cookieContainer.GetCookies(new Uri("https://steamcommunity.com/login/dologin")).Cast<Cookie>();

                //Чистим список старых пеенек
                cookieList.Clear();

                Cookie tmp;

                //Перебираем данные полученных печенек и пихаем их в список
                foreach (var cookie in responseCookies)
                {
                    tmp = new Cookie();
                    tmp.Comment = cookie.Comment;
                    tmp.CommentUri = cookie.CommentUri;
                    tmp.HttpOnly = cookie.HttpOnly;
                    tmp.Discard = cookie.Discard;
                    tmp.Domain = cookie.Domain;
                    tmp.Expired = cookie.Expired;
                    tmp.Expires = cookie.Expires;
                    tmp.Name = cookie.Name;
                    tmp.Path = cookie.Path;
                    tmp.Port = cookie.Port;
                    tmp.Secure = cookie.Secure;
                    tmp.Value = cookie.Value;
                    tmp.Version = cookie.Version;
                    cookieList.Add(tmp);
                }

                Console.WriteLine("Successfully logged in.");
                Console.WriteLine(result);

                //Сохраняем полученные Печеньки в json
                File.WriteAllText("Cookes.json", JsonConvert.SerializeObject(cookieList, Formatting.Indented));
                authorizationFlag = true;
                outStatus(7);
                return true;
            }
            else
            {
                //Вывод сообщения о работе
                outStatus(6);
                authorizationFlag = false;

                Console.WriteLine("Couldn't login...");
                Console.WriteLine(result);
                return false;
            }
        }
        public string EncryptPassword(RsaParameters rsaParam)
        {
            // Convert the public keys to BigIntegers
            var modulus = CreateBigInteger(rsaParam.Modulus);
            var exponent = CreateBigInteger(rsaParam.Exponent);

            // (modulus.ToByteArray().Length - 1) * 8
            //modulus has 256 bytes multiplied by 8 bits equals 2048
            var encryptedNumber = Pkcs1Pad2(rsaParam.Password, (2048 + 7) >> 3);

            // And now, the RSA encryption
            encryptedNumber = BigInteger.ModPow(encryptedNumber, exponent, modulus);

            //Reverse number and convert to base64
            var encryptedString = Convert.ToBase64String(encryptedNumber.ToByteArray().Reverse().ToArray());

            return encryptedString;
        }
        public static BigInteger CreateBigInteger(string hex)
        {
            return BigInteger.Parse("00" + hex, NumberStyles.AllowHexSpecifier);
        }
        public static BigInteger Pkcs1Pad2(string data, int keySize)
        {
            if (keySize < data.Length + 11)
                return new BigInteger();

            var buffer = new byte[256];
            var i = data.Length - 1;

            while (i >= 0 && keySize > 0)
            {
                buffer[--keySize] = (byte)data[i--];
            }

            // Padding, I think
            var random = new Random();
            buffer[--keySize] = 0;
            while (keySize > 2)
            {
                buffer[--keySize] = (byte)random.Next(1, 256);
                //buffer[--keySize] = 5;
            }

            buffer[--keySize] = 2;
            buffer[--keySize] = 0;

            Array.Reverse(buffer);

            return new BigInteger(buffer);
        }

        /// <summary>
        /// Функция для вывода сообщений на форму
        /// </summary>
        /// <param name="statusCode">Код сообщения</param>
        public void outStatus(int statusCode)
        {
            statusBlock.Text += "\n" + statusList[statusCode];
        }
    }
}