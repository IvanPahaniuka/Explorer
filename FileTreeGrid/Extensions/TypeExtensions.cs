using System;
using System.Collections.Generic;
using System.Text;

namespace FileTreeGrids.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsCompatible(this Type thisType, Type type)
        {
            if (type == null)
                return false;

            return thisType == type || thisType.IsSubclassOf(type);
        }
        public static bool IsCompatible<T>(this Type thisType)
        {
            return thisType.IsCompatible(typeof(T));
        }
    }
}
