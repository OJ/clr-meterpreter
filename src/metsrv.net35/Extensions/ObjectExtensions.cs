using System.Reflection;

namespace Met.Core.Extensions
{
    public static class ObjectExtensions
    {
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
    }
}
