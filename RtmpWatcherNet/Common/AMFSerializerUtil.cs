using System;
using System.IO;
using RtmpWatcherNet.Parser;

namespace RtmpWatcherNet.Common
{
    public static class AMFSerializerUtil<T>
    {
        public static Object DeserializeAmf(MemoryStream stream)
        {
            try
            {
                using (var deserializer = new AMFReader(stream))
                {
                    var metohd = deserializer.ReadData();
                    var requestId = deserializer.ReadData();
                    var nullVal = deserializer.ReadData();

                    Object obj = "na";

                    try
                    {
                        obj = deserializer.ReadData();
                    }
                    catch(Exception ex)
                    {
                        
                    }

                    Console.WriteLine("{0}: {1}", metohd, obj);
//                    return deserializer.ReadObject(new FluorineClassMappingApplicationContext());
                }
            }
            catch
            {
            }
            return null;
        }
    }
}
