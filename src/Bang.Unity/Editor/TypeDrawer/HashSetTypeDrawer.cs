using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;


namespace Bang.Unity.Editor {

public class HashSetTypeDrawer : ITypeDrawer
{
    public bool CanHandlesType(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>);

    public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target)
    {
        var elementType = memberType.GetGenericArguments()[0];
        var itemsToRemove = new ArrayList();
        var itemsToAdd = new ArrayList();
        var isEmpty = !((IEnumerable)value).GetEnumerator().MoveNext();

        EditorGUILayout.BeginHorizontal();
        {
            if (isEmpty)
                EditorGUILayout.LabelField(memberName, "empty");
            else
                EditorGUILayout.LabelField(memberName);

            if (EntityDrawer.MiniButton($"new {elementType.ToCompilableString().TypeName()}"))
                if (EntityDrawer.CreateDefault(elementType, out var defaultValue))
                    itemsToAdd.Add(defaultValue);
        }
        EditorGUILayout.EndHorizontal();

        if (!isEmpty)
        {
            EditorGUILayout.Space();
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = indent + 1;
            foreach (var item in (IEnumerable)value)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EntityDrawer.DrawObjectMember(elementType, string.Empty, item,
                        target, (_, newValue) =>
                        {
                            itemsToRemove.Add(item);
                            itemsToAdd.Add(newValue);
                        });

                    if (EntityDrawer.MiniButton("-"))
                        itemsToRemove.Add(item);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel = indent;
        }

        var removeMethod = memberType.GetMethod("Remove")!;
        foreach (var item in itemsToRemove)
            removeMethod.Invoke(value, new[] { item });

        var addMethod = memberType.GetMethod("Add")!;
        foreach (var item in itemsToAdd)
            addMethod.Invoke(value, new[] { item });

        return value;
    }

}

}
