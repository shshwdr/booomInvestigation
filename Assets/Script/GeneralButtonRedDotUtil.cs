using System.Reflection;
using UnityEngine;

public static class GeneralButtonRedDotUtil
{
    public static GameObject ResolveRedDot(GameObject host)
    {
        if (host == null)
        {
            return null;
        }

        var components = host.GetComponents<Component>();
        foreach (var component in components)
        {
            if (component == null)
            {
                continue;
            }

            var type = component.GetType();
            if (type.Name != "GeneralButton")
            {
                continue;
            }

            var field = type.GetField("redDot", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                return null;
            }

            return field.GetValue(component) as GameObject;
        }

        return null;
    }
}
