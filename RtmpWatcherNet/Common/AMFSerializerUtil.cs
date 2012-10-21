using System;
using System.IO;
using com.TheSilentGroup.Fluorine;

namespace RtmpWatcherNet.Common
{
    public static class AMFSerializerUtil<T>
    {
        public static byte[] SerializeAMF3(T item)
        {
            using (var amfData = new MemoryStream())
            {
                var ser = new AMFSerializer(amfData) { UseLegacyCollection = false };
                using (var fcm = new FluorineClassMappingApplicationContext())
                {
                    ser.WriteData(fcm, ObjectEncoding.AMF3 , item);
                    amfData.Position = 0;
                    return amfData.ToArray();
                }
            }
        }

        public static Object DeserializeAmf(MemoryStream stream)
        {
            using(var deserializer = new AMFDeserializer(stream))
            {
                return deserializer.ReadObject(new FluorineClassMappingApplicationContext());
            }
        }

        public static string ConvertToBase64(T item)
        {
            var amfData = SerializeAMF3(item);
            return Convert.ToBase64String(amfData);
        }
    }
}
