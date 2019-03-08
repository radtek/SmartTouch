using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using System.Globalization;
namespace SmartTouchSqli18n
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new Sqli18nService();
            var sqlMessages = service.GetMessages();
            var languages = ConfigurationManager.AppSettings["languages"].Split(',');
            foreach(var language in languages)
            {
                Console.WriteLine("Started processing PO file for language code:" + language);
                var file = Directory.GetFiles(Path.Combine(ConfigurationManager.AppSettings["locale_path"],language), "*.po", SearchOption.AllDirectories).FirstOrDefault();
                IEnumerable<i18nMessage> poMessages = service.GetMessages(file);
                var newMessages = sqlMessages.Except(poMessages, s => s.MessageId, po => po.MessageId);
                using (System.IO.StreamWriter fileStream = new System.IO.StreamWriter(file, true))
                {
                    fileStream.WriteLine("\n");

                    newMessages.ToList().ForEach(m =>
                    {
                        fileStream.WriteLine("\n");
                        fileStream.WriteLine("\n");
                        fileStream.WriteLine("#: Database Translation Message");
                        Add(fileStream, m);
                    });
                }
                var dir = Directory.GetParent(file);

                //Load Old PO Messages
                var oldPoMessages = service.GetMessages(Path.Combine(ConfigurationManager.AppSettings["locale_path"], language, "old", "messages.po"));
                var mergedMessages = new List<i18nMessage>();


                poMessages.ToList().ForEach(n=>
                {
                    var po = oldPoMessages.Where(m => m.MessageId == n.MessageId).FirstOrDefault();
                    if (po != null)
                        mergedMessages.Add(po);
                    else
                        mergedMessages.Add(n);
                });
                var filename = Path.GetFileName(file);

                using (System.IO.StreamWriter streamWritter = new System.IO.StreamWriter(file.Replace(filename,"messages_new.po"), true))
                {
                    streamWritter.WriteLine("\n");
                    var i = 0;
                    poMessages.ToList().ForEach(m =>
                    {
                        var po = oldPoMessages.Where(o => o.MessageId == m.MessageId).FirstOrDefault();
                        if (po == null)
                            po = m;
                        
                        Add(streamWritter, po);
                        i++;
                    });
                }

            }
            Console.ReadLine();
        }
        static void Add(StreamWriter writer, i18nMessage message)
        {
            writer.WriteLine("msgid \"" + message.MessageId + "\"");
            if(string.IsNullOrEmpty(message.Translation) ? false: message.Translation.Contains("msgstr"))
                writer.WriteLine((string.IsNullOrEmpty(message.Translation) ? string.Empty : message.Translation));
            else
                writer.WriteLine("msgstr " + "\"" + (string.IsNullOrEmpty(message.Translation) ? string.Empty : message.Translation) + "\"");
            Console.WriteLine(message.MessageId + ":" + message.Translation);
        }
    }

    public class i18nMessage
    {
        public string MessageId { get; set; }
        public string Translation { get; set; }
    }
}
