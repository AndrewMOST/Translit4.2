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
        const string directory = "../../../Книги";
        const string uri = "https://www.gutenberg.org/files/1342/1342-0.txt";

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

            GetFileFromWeb(uri, "../../../");
        }

        static void ReadReplaceAndWrite(string filename, Dictionary<char, string> pairs)
        {
            string text = "";

            string[] pathsplit = filename.Split(new char[] { '/', '\\' });
            string thename = pathsplit.Last();

            int initialnum = 0;

            try
            {
                text = File.ReadAllText(filename);
                initialnum = text.Length;
            }
            catch (Exception e) when (e is IOException || e is System.Security.SecurityException || e is AccessViolationException)
            {
                Console.WriteLine($"Error while reading file {filename}");
            }

            text = Translit(text, pairs);

            try
            {
                pathsplit[pathsplit.Length - 1] = "new_" + pathsplit[pathsplit.Length - 1];
                File.WriteAllText(string.Join("/", pathsplit), text);
            }
            catch (Exception e) when (e is IOException || e is System.Security.SecurityException || e is AccessViolationException)
            {
                Console.WriteLine($"Error while Writing file");
            }

            Console.WriteLine($"File {thename} went from {initialnum} to {text.Length} characters.");
        }

        static void Conventional()
        {
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

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

            stopwatch.Stop();

            Console.WriteLine($"Files processed consequently in {stopwatch.ElapsedMilliseconds / (double) 1000} seconds.\n");
        }

        static void MultiThread()
        {
            List<Task> tasks = new List<Task>();

            Stopwatch stopwatch = new Stopwatch();
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

            Task MegaTask = Task.WhenAll(tasks);

            stopwatch.Start();

            MegaTask.Wait();

            stopwatch.Stop();

            Console.WriteLine($"Files processed asynchronously in {stopwatch.ElapsedMilliseconds / (double) 1000} seconds.\n");
        }

        static void GetFileFromWeb(string url, string path)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";


            string responsefromserver = "";
            bool ifsuccessful = true;

            try
            {
                WebResponse response = request.GetResponse();

                Stream dataStream;

                using (dataStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(dataStream);
                    responsefromserver = reader.ReadToEnd();
                }
                Console.WriteLine("Finished!");
            }
            catch (Exception)
            {
                ifsuccessful = false;
                Console.WriteLine("Error while getting text from web");
            }

            Stopwatch stopwatch = new Stopwatch();

            if (ifsuccessful)
            {
                stopwatch.Start();

                string text = Translit(responsefromserver, Letters);

                try
                {
                    File.WriteAllText(directory + "/new_book_from_web.txt", text);
                }
                catch (Exception e) when (e is IOException || e is System.Security.SecurityException || e is AccessViolationException)
                {
                    Console.WriteLine($"Error while Writing file");
                }

                stopwatch.Stop();
            }

            Console.WriteLine($"Downloaded book was processed in {stopwatch.ElapsedMilliseconds / (double) 1000} seconds.");
        }

        static string Translit(string text, Dictionary<char, string> pairs)
        {
            List<string> textlist = new List<string>();

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

            return string.Join("", textlist.ToArray());
        }
    }
}
