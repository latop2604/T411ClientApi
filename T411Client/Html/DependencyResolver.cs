using System;
using System.Collections.Generic;

namespace T411.Html
{
    public static class DependencyResolver
    {
        private static readonly Dictionary<Type, Func<object>> Mapping = new Dictionary<Type, Func<object>>();

        public static T Resolve<T>() where T : class
        {
            Func<object> func;
            var type = typeof (T);
            if (Mapping.TryGetValue(type, out func))
            {
                var obj = func();
                return obj as T;
            }
            throw new InvalidOperationException("Unabled to find mapping for " + type.Name);
        }

        public static void Register<T>(Func<T> generator)
        {
            if(generator == null)
                throw new ArgumentNullException(nameof(generator));

            Mapping[typeof (T)] = () => generator();
        }
    }
}
