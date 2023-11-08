using System.Reflection;

namespace nFirewall.Domain.Shared;

public static class Common
{
    public static IEnumerable<Type> GetImplementedInterfaceOf<T>(Assembly assembly)
    {
        return GetImplementedInterfaceOf(typeof(T), assembly).ToList();
    }

    public static IEnumerable<Type> GetImplementedInterfaceOf(Type type, Assembly assembly)
    {
        return assembly
            .GetExportedTypes()
            .Where(t => type.IsAssignableFrom(t) && !t.IsInterface)
            .GroupBy(a => a)
            .Select(a => a.Key);
    }
}