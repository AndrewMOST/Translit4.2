using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Net.Http;
using System.Net;

namespace CW42
{
    class Program
    {
        // Ну директория тут, ну шо.
        const string directory = "../../../Книги";

        // Ну и такой вот путь (туть).
        const string uri = "https://www.gutenberg.org/files/1342/1342-0.txt";

        // Словарь соответствия.
        static private readonly Dictionary<char, string> Letters = new Dictionary<char, string>
        {
            ['A'] = "А",
            ['B'] = "Б",
            ['C'] = "Ц",
            ['D'] = "Д",
            ['E'] = "Е",
            ['F'] = "Ф",
            ['G'] = "Г",
            ['H'] = "Х",
            ['I'] = "И",
            ['J'] = "Ж",
            ['K'] = "К",
            ['L'] = "Л",
            ['M'] = "М",
            ['N'] = "Н",
            ['O'] = "О",
            ['P'] = "П",
            ['Q'] = "КУ",
            ['R'] = "Р",
            ['S'] = "С",
            ['T'] = "Т",
            ['U'] = "У",
            ['V'] = "В",
            ['W'] = "У",
            ['X'] = "КС",
            ['Y'] = "Ы",
            ['Z'] = "З",
            ['a'] = "а",
            ['b'] = "б",
            ['c'] = "ц",
            ['d'] = "д",
            ['e'] = "е",
            ['f'] = "ф",
            ['g'] = "г",
            ['h'] = "х",
            ['i'] = "и",
            ['j'] = "ж",
            ['k'] = "к",
            ['l'] = "л",
            ['m'] = "м",
            ['n'] = "н",
            ['o'] = "о",
            ['p'] = "п",
            ['q'] = "ку",
            ['r'] = "р",
            ['s'] = "с",
            ['t'] = "т",
            ['u'] = "у",
            ['v'] = "в",
            ['w'] = "у",
            ['x'] = "кс",
            ['y'] = "ы",
            ['z'] = "з",
        };

        static void Main(string[] args)
        {
            Conventional();

            MultiThread();

            GetAndProcess(uri, directory);
        }

        /// <summary>
        /// Чтение, транслитерация и запись файла.
        /// </summary>
        /// <param name="filename">Путь к файлу</param>
        /// <param name="pairs">Словаарь соответствия</param>
        static void ReadReplaceAndWrite(string filename, Dictionary<char, string> pairs)
        {
            // Получаем тут непосредственное имя файла.
            string text = "";

            string[] pathsplit = filename.Split(new char[] { '/', '\\' });
            string thename = pathsplit.Last();

            // Количество изначальных символов.
            int initialnum = 0;

            // Читаем-с.
            try
            {
                text = File.ReadAllText(filename);
                // Записываем количество символов.
                initialnum = text.Length;
            }
            catch (Exception e) when (e is IOException || e is System.Security.SecurityException || e is AccessViolationException)
            {
                Console.WriteLine($"Error while reading file {filename}");
            }

            // Транслитерируем.
            text = Translit(text, pairs);

            // Записываем в новый файл.
            try
            {
                pathsplit[pathsplit.Length - 1] = "new_" + pathsplit[pathsplit.Length - 1];
                File.WriteAllText(string.Join("/", pathsplit), text);
            }
            catch (Exception e) when (e is IOException || e is System.Security.SecurityException || e is AccessViolationException)
            {
                Console.WriteLine($"Error while Writing file");
            }

            // Выводим нужную инфу.
            Console.WriteLine($"File {thename} went from {initialnum} to {text.Length} characters.");
        }

        /// <summary>
        /// Обычная, последовательная обработка файлов в директории.
        /// </summary>
        static void Conventional()
        {
            // Таймер для подсчета времени.
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            // Транслитим каждый файл.
            try
            {
                foreach (var file in Directory.GetFiles(directory))
                {
                    if (!file.Contains("new"))
                    {
                        ReadReplaceAndWrite(file, Letters);
                    }
                }
            }
            catch (Exception e) when (e is IOException || e is System.Security.SecurityException || e is AccessViolationException)
            {
                Console.WriteLine("Please help");
            }

            // Останавливаем таймер, выводим инфу.
            stopwatch.Stop();

            Console.WriteLine($"Files processed consequently in {stopwatch.ElapsedMilliseconds / (double) 1000} seconds.\n");
        }

        /// <summary>
        /// Параллелизированная обработка файлов.
        /// </summary>
        static void MultiThread()
        {
            // Лист задач обработки.
            List<Task> tasks = new List<Task>();

            Stopwatch stopwatch = new Stopwatch();

            //Добавляем таски для каждого файла.
            try
            {
                foreach (var file in Directory.GetFiles(directory))
                {
                    if (!file.Contains("new"))
                    {
                        tasks.Add(Task.Run(() => ReadReplaceAndWrite(file, Letters)));
                    }
                }
            }
            catch (Exception e) when (e is IOException || e is System.Security.SecurityException || e is AccessViolationException)
            {
                Console.WriteLine("Please help");
            }

            // Делаем общий таск, который подождет, пока завершится все внутри, и запускаем.
            Task MegaTask = Task.WhenAll(tasks);

            stopwatch.Start();

            MegaTask.Wait();

            stopwatch.Stop();

            // Выводим инфу.
            Console.WriteLine($"Files processed asynchronously in {stopwatch.ElapsedMilliseconds / (double) 1000} seconds.\n");
        }

        /// <summary>
        /// Получение файла по ссылке и обработка.
        /// </summary>
        /// <param name="url">Ссылка на страницу</param>
        /// <param name="dir">Директория записи</param>
        static void GetAndProcess(string url, string dir)
        {
            // Реквест по ссылке.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";


            string responsefromserver = "";
            bool ifsuccessful = true;

            // Отправляем запрос, получаем ответ, profit.
            try
            {
                WebResponse response = request.GetResponse();

                Stream dataStream;

                using (dataStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(dataStream);
                    responsefromserver = reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                ifsuccessful = false;
                Console.WriteLine("Error while getting text from web");
            }

            Stopwatch stopwatch = new Stopwatch();

            // Если успешно скачали, запускаем обработ(очку).
            if (ifsuccessful)
            {
                stopwatch.Start();

                int initialsymbols = responsefromserver.Length;

                string text = Translit(responsefromserver, Letters);

                // Записываем готовый файл.
                try
                {
                    File.WriteAllText(dir + "/new_book_from_web.txt", text);
                }
                catch (Exception e) when (e is IOException || e is System.Security.SecurityException || e is AccessViolationException)
                {
                    Console.WriteLine($"Error while Writing file");
                }

                Console.WriteLine($"Downloaded book went from {initialsymbols} to {text.Length} characters.");

                stopwatch.Stop();
            }

            // Выводим инфу.
            Console.WriteLine($"Downloaded book was processed in {stopwatch.ElapsedMilliseconds / (double) 1000} seconds.");
        }

        /// <summary>
        /// Транслитерация текста.
        /// </summary>
        /// <param name="text">Изначальный текст</param>
        /// <param name="pairs">Словарь соответствия</param>
        /// <returns></returns>
        static string Translit(string text, Dictionary<char, string> pairs)
        {
            // Создаем список строк.
            List<string> textlist = new List<string>();

            // Пробегаем по данному тексту, проверяем, является ли чар буквой,
            // Проверяем, есть ли в словаре, заменяем (или нет).
            for (int i = 0; i < text.Length; i++)
            {
                char current = text[i];

                if (Char.IsLetter(current))
                {
                    if (pairs.ContainsKey(current))
                    {
                        textlist.Add(pairs[current]);
                    }
                }
                else
                {
                    textlist.Add(current.ToString());
                }
            }

            // Сгоняем все в строку, возвращаем.
            return string.Join("", textlist.ToArray());
        }
    }
}
