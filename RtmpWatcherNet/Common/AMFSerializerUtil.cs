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
            try
            {
                using (var deserializer = new AMFDeserializer(stream))
                {
                    var metohd = deserializer.ReadData(null);
                    var requestId = deserializer.ReadData(null);
                    var nullVal = deserializer.ReadData(null);
                    var obj = deserializer.ReadData(null);

                    Console.WriteLine("Method {0}, Obj {1}", metohd, obj);
//                    return deserializer.ReadObject(new FluorineClassMappingApplicationContext());
                }
            }
            catch
            {
            }
            return null;
        }

        public static string ConvertToBase64(T item)
        {
            var amfData = SerializeAMF3(item);
            return Convert.ToBase64String(amfData);
        }
    }
}
