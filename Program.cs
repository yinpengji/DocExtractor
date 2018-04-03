using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MIMER.RFC2045;

namespace Docextractor
{
    class EndOfLineStrategy : MIMER.IEndCriteriaStrategy
    {
        #region IEndCriteriaStrategy Members

        public bool IsEndReached(char[] data, int size)
        {
            int previous, current;

            if (size > 0)
            {
                previous = data[size - 1];
                current = data[size];

                if (previous == 13 && current == 10)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
    class Program
    {
        static void Main(string[] args)
        {
            string path = ".";
            if (args.Count() > 0)
            {
                path = args[0];
            }
            var files = Directory.GetFiles(path, "*.doc");
            FileStream csvFile = new FileStream("output.csv", FileMode.Create);
            StreamWriter sw = new StreamWriter(csvFile, Encoding.Unicode);
            foreach (var file in files)
            {
                string content = File.ReadAllText(file);
                content = content.Replace("\n", "\r\n");
                Stream fs = new MemoryStream(Encoding.ASCII.GetBytes(content));
                
                var reader = new MailReader();
                MIMER.IEndCriteriaStrategy endofmessage = new BasicEndOfMessageStrategy();
                //MIMER.IEndCriteriaStrategy endofmessage = new EndOfLineStrategy();
                var message = reader.ReadMimeMessage(ref fs, endofmessage);
                //reader.Read(ref fs, endofmessage);
                var html = message.Body.FirstOrDefault<KeyValuePair<string, string>>().Value;


                var htmldoc = new HtmlAgilityPack.HtmlDocument();
                htmldoc.LoadHtml(html);

                //var nodes = htmldoc.DocumentNode.SelectNodes("//td");
                //foreach (var node in nodes)
                //{
                //    if (node.InnerText == "15021991585" || node.InnerText.Contains("丁曹凯"))
                //    {
                //        Console.WriteLine(node.InnerText + " " + node.XPath);
                //    }
                //}
                //var node = htmldoc.DocumentNode.SelectNodes("//a");
                var email = htmldoc.DocumentNode.SelectSingleNode("/html[1]/body[1]/table[2]/tr[1]/td[1]/html[1]/body[1]/table[1]/tr[1]/td[1]/table[1]/tr[1]/td[2]/table[1]/tr[2]/td[3]/table[1]/tr[1]/td[2]/a[1]");
                var name = htmldoc.DocumentNode.SelectSingleNode("/html[1]/body[1]/table[2]/tr[1]/td[1]/html[1]/body[1]/table[1]/tr[1]/td[1]/table[1]/tr[1]/td[2]");
                var phone = htmldoc.DocumentNode.SelectSingleNode("/html[1]/body[1]/table[2]/tr[1]/td[1]/html[1]/body[1]/table[1]/tr[1]/td[1]/table[1]/tr[1]/td[2]/table[1]/tr[2]/td[2]/table[1]/tr[1]/td[2]");
                //Console.WriteLine(email.InnerText + " " + name.InnerText + " " + phone.InnerText);
                int start = name.InnerText.IndexOf("&nbsp;") + 6;
                int end = name.InnerText.IndexOf("流程");
                string line = name.InnerText.Substring(start, end-start).Replace("&nbsp", "").Trim() + "," + phone.InnerText + "," + email.InnerText;

                sw.WriteLine(line);
            }
            sw.Close();
            csvFile.Close();
        }
    }
}

