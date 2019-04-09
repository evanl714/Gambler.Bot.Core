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
            T tmp = new T();
            foreach (PropertyInfo x in typ.GetProperties())
            {
                x.SetValue(tmp, x.GetValue(Instance));
            }
            return tmp;
        }
    }
}
