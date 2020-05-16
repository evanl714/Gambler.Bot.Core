using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DoormatCore.Helpers
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
                string serializedobj = Newtonsoft.Json.JsonConvert.SerializeObject(Instance);
                object result = Newtonsoft.Json.JsonConvert.DeserializeObject(serializedobj, typ);
                return result;
                object tmp = null;
                if (typ.IsArray)
                    tmp = Activator.CreateInstance(typ, (Instance as Array).Length);
                else
                    tmp = Activator.CreateInstance(typ);
                foreach (PropertyInfo x in typ.GetProperties())
                {
                    if (x.CanWrite)
                    {
                        object value = x.GetValue(Instance);

                        if (x.PropertyType.IsClass && !x.PropertyType.Namespace.Contains("System") && value != null)
                        {
                            x.SetValue(tmp, CreateCopy(x.PropertyType, value));
                        }
                        else
                        {
                            x.SetValue(tmp, value);
                        }
                    }
                }
                return tmp;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
