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
            object tmp = Activator.CreateInstance(typ);
            foreach (PropertyInfo x in typ.GetProperties())
            {
                if (x.CanWrite)
                    x.SetValue(tmp, x.GetValue(Instance));
            }
            return tmp;
        }
    }
}
