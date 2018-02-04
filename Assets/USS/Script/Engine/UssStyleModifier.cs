using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UssStyleModifier : MonoBehaviour
{
    private class ModifierData
    {
        public object modifier;
        public MethodInfo method;
        public Type acceptedComponent;
        public bool isArrayParameter;
    }

    public static bool loaded = false;
    public static bool hasError = false;
    public static bool applied = false;

    private static List<UssStyleDefinition> styles;
    private static Dictionary<string, ModifierData> modifiers;

    private static DateTime applyTime;

    static UssStyleModifier()
    {
        modifiers = new Dictionary<string, ModifierData>();

        // Load default modifiers
        LoadModifier<UssColorModifier>();
        LoadModifier<UssTextModifier>();
        LoadModifier<UssOutlineModifier>();
        LoadModifier<UssShadowModifier>();
        LoadModifier<UssPaddingModifier>();
        LoadModifier<UssOverflowModifier>();
        LoadModifier<UssTransformModifier>();
    }
    public static void LoadModifier<T>()
        where T : new()
    {
        var obj = new T();

        foreach (var method in typeof(T).GetMethods())
        {
            var attrs = method.GetCustomAttributes(typeof(UssModifierKeyAttribute), true);
            if (attrs.Length == 0)
                continue;

            var key = ((UssModifierKeyAttribute)attrs[0]).key;
            if (modifiers.ContainsKey(key))
                throw new InvalidOperationException("Already has modifier with key: " + key);
            if (method.GetParameters().Length != 2)
                throw new InvalidOperationException("Invalid modifier format. Params.Length must be length of 2.");

            modifiers.Add(key, new ModifierData()
            {
                modifier = obj,
                method = method,
                acceptedComponent = method.GetParameters().First().ParameterType,
                isArrayParameter = method.GetParameters().Last().ParameterType.IsArray
            });
        }
    }
    public static void LoadUss(string uss)
    {
        loaded = true;

        try
        {
            UssValues.Reset();
            var result = UssParser.Parse(uss);
            styles = new List<UssStyleDefinition>(result.styles);
            foreach (var pair in result.values)
                UssValues.SetValue(pair.Key, pair.Value);
            
            Apply(UssRoot.FindRootInScene().gameObject);

            hasError = false;
            applied = true;

#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
#endif
        }
        catch(Exception e)
        {
            hasError = true;
            Debug.LogException(e);
        }
    }

    public static void Apply(GameObject g)
    {
        Apply(g, styles.ToArray());
    }
    public static void Apply(GameObject g, UssStyleDefinition[] styles)
    {
        if (g == null)
            Debug.LogWarning("Apply: gameObject is null");
        if (styles == null)
            throw new InvalidOperationException(".ucss file not loaded yet.");

        int appliedCount = 0;
        applyTime = DateTime.Now;

        foreach (var style in styles)
        {
            if (CheckConditions(g, style.conditions) == false)
                continue;

            AddInspectorItem(g, style);

            var properties = new List<UssStyleProperty>(style.properties);
            foreach (var bundle in style.bundles)
            {
                var bundleValue = UssValues.GetValue(bundle) as UssBundleValue;
                if (bundleValue == null)
                    throw new InvalidOperationException(bundle + " is not a bundle.");

                properties.AddRange(bundleValue.value);
            }

            foreach (var p in properties)
            {
                foreach (var m in modifiers)
                {
                    if (p.key != m.Key) continue;

                    var comp = g.GetComponent(m.Value.acceptedComponent);
                    if (comp == null) continue;

                    if (m.Value.isArrayParameter)
                    {
                        m.Value.method.Invoke(m.Value.modifier, new object[]{
                            comp, p.values
                        });
                    }
                    else
                    {
                        m.Value.method.Invoke(m.Value.modifier, new object[]{
                            comp, p.values[0]
                        });
                    }
                }
            }

            appliedCount ++;
        }

        if (appliedCount == 0)
            DestroyImmediate(g.GetComponent<UssInspector>());

        for (int i = 0; i < g.transform.childCount; i++)
            Apply(g.transform.GetChild(i).gameObject, styles);
    }
    public static UnityEngine.Object[] GetReferences(GameObject root, string key)
    {
        var result = new List<UnityEngine.Object>();
        var styleRefs = new List<UssStyleDefinition>();

        foreach (var style in styles)
        {
            if (style.properties.SelectMany(x => x.values).Any(x => (x is UssRefValue) && ((UssRefValue)x).key == key))
                styleRefs.Add(style);
        }

        GetReferencesInternal(root, styleRefs, result);

        return result.ToArray();
    }
    private static void GetReferencesInternal(GameObject g, List<UssStyleDefinition> styleRefs, List<UnityEngine.Object> result)
    {
        foreach (var style in styleRefs)
        {
            if (CheckConditions(g, style.conditions))
            {
                result.Add(g);
                break;
            }
        }

        for (int i = 0; i < g.transform.childCount; i++)
            GetReferencesInternal(g.transform.GetChild(i).gameObject, styleRefs, result);
    }

    public static GameObject[] FindObjects(GameObject g, UssStyleCondition[] conditions)
    {
        var result = new List<GameObject>();
        if (conditions.Length > 0)
            FindObjectsInternal(g, conditions, result);
        return result.ToArray();
    }
    private static void FindObjectsInternal(GameObject g, UssStyleCondition[] conditions, List<GameObject> result)
    {
        if (CheckConditions(g, conditions))
            result.Add(g);

        for (int i = 0; i < g.transform.childCount; i++)
            FindObjectsInternal(g.transform.GetChild(i).gameObject, conditions, result);
    }

    private static int CheckConditionsUpwards(GameObject g, UssStyleCondition[] conditions, int offset, bool mustMatch = false)
    {
        var target = conditions[conditions.Length - 1 - offset];
        var check = CheckCondition(g, target);

        offset += check ? 1 : 0;

        if (mustMatch && check == false)
            return offset;

        if (g.transform.parent == null) 
            return offset;
        else if (offset == conditions.Length)
            return offset;
        else
        {
            return CheckConditionsUpwards(
                g.transform.parent.gameObject, conditions, offset,
                check ? target.type == UssStyleConditionType.DirectDescendant : false);
        }
    }
    private static bool CheckCondition(GameObject g, UssStyleCondition c)
    {
        if (c.target == UssSelectorType.All)
            return true;
        else if (c.target == UssSelectorType.Name)
        {
            if (g.name != c.name)
                return false;
        }
        else if (c.target == UssSelectorType.Component)
        {
            if (g.GetComponent(c.name) == null)
                return false;
        }
        else if (c.target == UssSelectorType.Class)
        {
            var klass = g.GetComponent<UssClass>();
            if (klass == null)
                return false;
            if (klass.classes.Split(' ').Contains(c.name) == false)
                return false;
        }

        return true;
    }
    private static bool CheckConditions(GameObject g, UssStyleCondition[] conditions)
    {
        var check = CheckCondition(g, conditions[conditions.Length - 1]);
        if (check == false)
            return false;
        if (conditions.Length == 1)
            return check;

        return CheckConditionsUpwards(g.transform.parent.gameObject, conditions, 1,
            conditions[conditions.Length - 1].type == UssStyleConditionType.DirectDescendant) == conditions.Length; 
    }

    private static void AddInspectorItem(GameObject g, UssStyleDefinition style)
    {
        var insp = g.GetComponent<UssInspector>();
        if (insp == null)
            insp = g.AddComponent<UssInspector>();

        if (insp.updatedAt != applyTime)
            insp.Clear();

        insp.applied.Add(style);
        insp.updatedAt = applyTime;
    }
}
