using UnityEngine.Timeline;
using UnityEngine;
using UnityEngine.Playables;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TimelineImp : IInterface
{

    public NotifyDelegate notify { get; set; }


    public T Load<T>(string location) where T : Object
    {
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<T>(location);
#else
         return Resources.Load<T>(location);
#endif
    }


    public T Load<T>(string location, Transform parent, Vector3 pos, Quaternion rot) where T : Object
    {
        T obj = Load<T>(location);
        Transform tf = null;
        if (obj is Transform)
        {
            tf = obj as Transform;
        }
        else if (obj is GameObject)
        {
            tf = (obj as GameObject).transform;
        }
        if (tf)
        {
            tf.parent = parent;
            tf.localPosition = pos;
            tf.rotation = rot;
        }
        return obj;
    }
    
}
