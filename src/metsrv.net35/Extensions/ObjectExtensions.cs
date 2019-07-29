using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Met.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static byte[] ToByteArray<T>(this T structure) where T : struct
        {
            var size = Marshal.SizeOf(structure);
            var ptr = Marshal.AllocHGlobal(size);
            var result = new byte[size];

            Marshal.StructureToPtr(structure, ptr, true);
            Marshal.Copy(ptr, result, 0, size);
            Marshal.FreeHGlobal(ptr);

            return result;
        }

        public static T GetPrivateProperty<T>(this object obj, string name)
        {
            var property = obj.GetType().GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            return (T)property.GetValue(obj, null);
        }

        public static T GetPrivateField<T>(this object obj, string name)
        {
            var field = obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)field.GetValue(obj);
        }

        public static TResult Pokemon<T, TResult>(this T obj, Func<T, TResult> f, TResult def = default(TResult))
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

        public static void Pokemon<T, TResult>(this T obj, Func<T, TResult> f, Action<TResult> a)
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

        public static void Pokemon<T>(this T obj, Action<T> a)
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
