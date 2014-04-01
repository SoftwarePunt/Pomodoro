using System;
using System.IO;
using System.Text;
using System.Xml;
using Tomato.Properties;

namespace Tomato
{
    /// <summary>
    /// Handles activity log file creation.
    /// </summary>
    public class Logging
    {
        public static string FilePath
        {
            get
            {
                return Environment.CurrentDirectory + "/log.xml";
            }
        }

        public static bool FileExists
        {
            get
            {
                return File.Exists(FilePath);
            }
        }

        public static void WriteActivityLog(string activity, DateTime time)
        {
            if (!FileExists)
            {
                CreateFileFromTemplate();
            }

            XmlDocument document = new XmlDocument();
            document.Load(FilePath);

            XmlElement activityElement = document.CreateElement("Activity");

            XmlElement descriptionElement = document.CreateElement("Description");
            descriptionElement.InnerText = activity;

            XmlElement timeElement = document.CreateElement("StartTime");
            timeElement.InnerText = time.ToString();

            activityElement.AppendChild(timeElement);
            activityElement.AppendChild(descriptionElement);

            XmlNode rootElement = document.GetElementsByTagName("log")[0];
            rootElement.AppendChild(activityElement);

            document.Save(FilePath);
        }

        public static void CreateFileFromTemplate()
        {
            File.WriteAllText(FilePath, Resources.logging_template_xml, Encoding.UTF8);
        }
    }
}
