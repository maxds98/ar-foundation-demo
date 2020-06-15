using System;
using System.Collections.Generic;

public class ServiceLocator
{
    private static readonly Dictionary<Type, object> Services
        = new Dictionary<Type, object>();

    public static TI GetService<TI>()
    {
        return (TI)Services[typeof(TI)];
    }
    public static TI Register<TI, T>() where T : TI, new()
    {
        var service = new T();
        Services[typeof(TI)] = service;
        return service;
    }
    public static void Reset()
    {
        Services.Clear();
    }
}