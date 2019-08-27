using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;


[CustomEditor(typeof(PlayableDirector))]
public class TimelineEditor :  Editor
{
    [MenuItem("XEditor/Timeline/SelectDirector _F1", priority = 3)]
    public static void FocusDirector()
    {
        GameObject go = GameObject.Find("TIMELINE");
        if (go != null)
        {
            Selection.activeGameObject = go;
        }
    }
}
