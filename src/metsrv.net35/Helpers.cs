using System;

namespace Met.Core
{
    public static class Helpers
    {
        public static TResult Try<T, TResult>(T obj, Func<T, TResult> f, TResult def = default(TResult))
        {
            try
            {
                return f(obj);
            }
#if DEBUG
            catch(Exception e)
#else
            catch
#endif
            {
                // gotta catch 'em all
                return def;
            }
        }

        public static void Try<T, TResult>(T obj, Func<T, TResult> f, Action<TResult> a)
        {
            try
            {
                a(f(obj));
            }
#if DEBUG
            catch(Exception e)
#else
            catch
#endif
            {
                // gotta catch 'em all
            }
        }

        public static void Try<T>(T obj, Action<T> a)
        {
            try
            {
                a(obj);
            }
#if DEBUG
            catch(Exception e)
#else
            catch
#endif
            {
                // gotta catch 'em all
            }
        }
    }
}
