using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Gambler.Bot.Common.Helpers
{
    public class CopyHelper
    {
        public static T CreateCopy<T>(T Instance) where T : new()
        {
            Type typ = typeof(T);
            return (T)CreateCopy(typ, Instance);
            /*T tmp = new T();
            foreach (PropertyInfo x in typ.GetProperties())
            {
                if (x.CanWrite)
                x.SetValue(tmp, x.GetValue(Instance));
            }
            return tmp;*/
        }

        public static object CreateCopy(Type typ, object Instance)
        {
            try
            {
                string serializedobj = JsonSerializer.Serialize(Instance);
                object result = JsonSerializer.Deserialize(serializedobj, typ);
                return result;                
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
