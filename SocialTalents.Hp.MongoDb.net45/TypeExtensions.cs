using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.MongoDB
{
    public static class TypeExtensions
    {
        // https://stackoverflow.com/a/47900592/11768
        public static bool TypeIs(this Type x, Type d)
        {
            if (null == d)
            {
                return false;
            }

            for (var c = x; null != c; c = c.BaseType)
            {
                var a = c.GetInterfaces();

                for (var i = a.Length; i-- >= 0;)
                {
                    var t = i < 0 ? c : a[i];

                    if (t == d || t.IsGenericType && t.GetGenericTypeDefinition() == d)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static IEnumerator<TChild> Cast<TParent, TChild>(this IEnumerator<TParent> iterator) where TChild : TParent
        {
            while (iterator.MoveNext())
            {
                yield return (TChild)iterator.Current;
            }
        }
    }
}
