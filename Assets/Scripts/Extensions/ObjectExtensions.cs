using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Extensions
{
    public static class ObjectExtensions
    {
        public static string GetTypeName(this object obj)
        {
            return obj.GetType().Name;
        }
    }
}
