using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NullFinder))]
public class FindEditor : Editor
{
    NullFinder find;
    public override void OnInspectorGUI()
    {
        find = target as NullFinder;

        EditorGUILayout.LabelField("ClassName");
        GUILayout.TextField("",find.toFindclassName);

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

        EditorGUI.LabelField(new Rect(Vector2.zero, new Vector2(100f, 50f)), "List");
    }
}
