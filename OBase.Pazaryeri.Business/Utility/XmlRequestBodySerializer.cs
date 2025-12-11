using RestEase;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;

namespace OBase.Pazaryeri.Business.Utility
{
    public class XmlRequestBodySerializer : RequestBodySerializer
    {
        public override HttpContent SerializeBody<T>(T body, RequestBodySerializerInfo info)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
            if (body == null)
                return null;

            // Consider caching generated XmlSerializers
            var serializer = new XmlSerializer(typeof(T));

            //
            using (StringWriter stringWriter = new Utf8StringWriter())
            {
                serializer.Serialize(stringWriter, body);
                var content = new StringContent(stringWriter.ToString());
                // Set the default Content-Type header to application/xml
                content.Headers.ContentType.MediaType = "application/xml";
                return content;
            }
        }
    }
    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
