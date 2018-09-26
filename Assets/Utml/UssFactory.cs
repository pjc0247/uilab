using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using HtmlAgilityPack;

public class UssFactory : MonoBehaviour
{
    private static Dictionary<string, MethodInfo> ctors;

    static UssFactory()
    {
        ctors = new Dictionary<string, MethodInfo>();

        var asm = Assembly.GetExecutingAssembly();
        foreach (var klass in asm.GetTypes()
            .Where(x => x.IsSubclassOf(typeof(UtmlElement))))
        {
            var nameAttr = 
                (UtmlElementName)klass.GetCustomAttributes(true)
                .Where(x => x is UtmlElementName)
                .FirstOrDefault();
            if (nameAttr == null) continue; 

            var ctor = klass.GetMethod("Construct");
            if (ctor == null)
                throw new InvalidOperationException(klass.Name + " doesn't have a method `Construct`.");

            ctors[nameAttr.name] = ctor;
        }
    }

    public static GameObject CreateObject(HtmlNode node, Transform parent)
    {
        var go = new GameObject(node.Name, typeof(RectTransform));
        go.transform.SetParent(parent);

        if (ctors.ContainsKey(node.Name))
        {
            var ctor = ctors[node.Name];
            var comp = go.AddComponent(ctor.DeclaringType);
            var cd = new UtmlConstructionData(node);

            ctor.Invoke(comp, new object[] { cd });

            var id = cd.GetStringAttribute("id", null);
            if (id != null)
                go.name = id;

            var klass = cd.GetStringAttribute("class", null);
            if (klass != null)
                go.AddComponent<UssClass>().classes = klass;
        }

        return go;
    }
    
}
