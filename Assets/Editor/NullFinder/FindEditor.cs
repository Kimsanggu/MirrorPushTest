using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NullFinder))]
public class FindEditor : Editor
{
    NullFinder find;
    public override void OnInspectorGUI()
    {
        find = target as NullFinder;

        GUILayout.BeginHorizontal();

        GUILayout.Label("ToFindClassName");

        find.toFindclassName = EditorGUILayout.TextField(find.toFindclassName);

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        GUILayout.Label("ToFindMethodName");

        find.toFindMethodName= EditorGUILayout.TextField(find.toFindMethodName);

        GUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical();

        if (GUILayout.Button("Find"))
        {
            find.Find();
        }
        if (find != null)
        {
            if (find.missingList != null)
            {
                int length = find.missingList.Count;
                for (int index = 0; index < length; index++)
                {
                    find.missingList[index] = (GameObject)EditorGUILayout.ObjectField(find.missingList[index], typeof(Object), false);
                }
            }
        }

        EditorGUILayout.EndVertical();
    }
}
