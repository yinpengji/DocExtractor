using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Xml;

using NI.Email.Mime.Message;
using NI.Email.Mime.Field;
using NI.Email.Mime.Util;

namespace Docextractor
{
    public class MimeConverter
    {


        public MimeConverter() { }

        public string ConvertToXml(MimeMessage msg)
        {
            StringWriter strWr = new StringWriter();
            XmlTextWriter xmlWr = new XmlTextWriter(strWr);
            xmlWr.Formatting = Formatting.Indented;
            GenerateXml(xmlWr, msg, "", "1");
            return strWr.ToString();
        }

        protected void GenerateXml(XmlWriter xmlWr, Entity e, String prefix, String id)
        {

            if (e is MimeMessage)
            {
                xmlWr.WriteStartElement("message");
            }
            else
            {
                xmlWr.WriteStartElement("body-part");
            }

            xmlWr.WriteStartElement("header");
            foreach (MimeField fld in e.Header.Fields)
            {
                xmlWr.WriteElementString("field", fld.Raw);
            }
            xmlWr.WriteEndElement();

            if (e.Body is Multipart)
            {
                xmlWr.WriteStartElement("multipart");

                Multipart multipart = (Multipart)e.Body;
                IList parts = multipart.BodyParts;

                xmlWr.WriteElementString("preamble", multipart.Preamble);

                for (int i = 0; i < parts.Count; i++)
                {
                    GenerateXml(xmlWr, (Entity)parts[i], prefix, id + "_" + (i + 1).ToString());
                }
                xmlWr.WriteElementString("epilogue", multipart.Epilogue);

                xmlWr.WriteEndElement();
            }
            else if (e.Body is MimeMessage)
            {
                GenerateXml(xmlWr, (MimeMessage)e.Body, prefix, id + "_1");
            }
            else
            {
                IBody b = e.Body;
                String name = prefix + "_decoded_" + id
                    + (b is ITextBody ? ".txt" : ".bin");
                String tag = b is ITextBody ? "text-body" : "binary-body";

                xmlWr.WriteStartElement(tag);
                xmlWr.WriteAttributeString("name", name);
                xmlWr.WriteEndElement();
            }

            xmlWr.WriteEndElement();

        }


    }
}
