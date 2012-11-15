using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RtmpWatcherNet.Parser;

namespace RtmpWatcherNet.Common
{
    public static class AMFSerializerUtil<T> where T : class, new()
    {
        public static AmfContainer DeserializeAmf(MemoryStream stream)
        {
            try
            {
                using (var deserializer = new AMFReader(stream))
                {
                    var method = deserializer.ReadData();
                    var requestId = deserializer.ReadData();
                    var nullVal = deserializer.ReadData();

                    Object obj = "na";

                    try
                    {
                        var container = new AmfContainer();
                        obj = deserializer.ReadData();

                        container.Name = Convert.ToString(method);
                        container.Values = new List<AmfContainer>{ new AmfContainer
                                                                       {
                                                                           Value = obj
                                                                       }};

                        return container;
                    }
                    catch(Exception ex)
                    {
                        
                    }

                    Console.WriteLine("{0}: {1}", method, obj);
                }
            }
            catch
            {
            }
            return null;
        }
    }
}
