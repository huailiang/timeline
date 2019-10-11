using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineTool
{
    [MenuItem("Tool/SelectDirector _F1")]
    public static void FocusDirector()
    {
        GameObject go = GameObject.Find("TIMELINE");
        if (go != null)
        {
            Selection.activeGameObject = go;
        }
    }

    
    [MenuItem("Tool/Save _F2")]
    public static void Save()
    {
        var director = GameObject.FindObjectOfType<PlayableDirector>();
        TimelineSaver.Save(director);
        ExportDirectorRecord();
    }

    [MenuItem("Tool/ClearBinding _F3")]
    public static void ClearBinding()
    {
        var director = GameObject.FindObjectOfType<PlayableDirector>();
        SerializedObject serializedObject = new SerializedObject(director);
        SerializedProperty sp = serializedObject.FindProperty("m_SceneBindings");
        if (sp != null)
        {
            sp.arraySize = 0;
            serializedObject.ApplyModifiedProperties();
        }
    }

    private static void Export<T>(IEnumerable<T> assets, Action<T> action)
         where T : UnityEngine.Object
    {
        float cnt = (float)assets.Count();
        int i = 0;
        foreach (var it in assets)
        {
            EditorUtility.DisplayProgressBar("export", "exporting " + it.name, i++ / cnt);
            if (action != null)
            {
                action(it);
            }
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    

    /// <summary>
    /// 支持timeline录制的clip导出
    /// （运行时不再加载.playable文件，因此需要导出其内部的录制内容）
    /// </summary>
    [MenuItem("Assets/Timeline/Export_Record")]
    public static void ExportRecords()
    {
        var objs = Selection.objects;
        var assets = objs.
            Where(x => x is TimelineAsset).
            Select(x => x as TimelineAsset);
        Export(assets, ExportRecord);
        EditorUtility.DisplayDialog("note", "export anim job done", "ok");
    }

    

    private static void ExportRecord(TimelineAsset asset)
    {
        var tracks = asset.GetRootTracks();
        var clips = tracks.
            Where(x => x is AnimationTrack).
            Select(x => x as AnimationTrack).
            Select(x => x.infiniteClip);

        string path = AssetDatabase.GetAssetPath(asset);
        path = path.Substring(0, path.LastIndexOf('/'));
        foreach (var clip in clips)
        {
            if (clip != null)
            {
                Debug.Log(clip.name);
                AnimationClip newClip = new AnimationClip();
                EditorUtility.CopySerializedIfDifferent(clip, newClip);
                AssetDatabase.CreateAsset(newClip, path + "/" + asset.name + "_" + clip.name + ".anim");
            }
        }
    }
    
    public static void ExportDirectorRecord()
    {
        PlayableDirector playable = GameObject.FindObjectOfType<PlayableDirector>();
        if (playable != null)
        {
            TimelineAsset asset = playable.playableAsset as TimelineAsset;
            if (asset != null)
            {
                playable.time = 0;
                AssetDatabase.SaveAssets();
                var tracks = asset.GetRootTracks().Where(x => x is AnimationTrack);
                Dictionary<string, AnimationClip> dic = new Dictionary<string, AnimationClip>();
                foreach (var it in tracks)
                {
                    var tf = DirectorSystem.FetchAttachOfTrack(it);
                    if (tf && tf.gameObject.GetComponent<Animator>())
                    {
                        AnimationTrack atrack = it as AnimationTrack;
                        if (atrack.infiniteClip != null)
                        {
                            Debug.Log(tf.name + " " + tf.localPosition + " " + tf.localEulerAngles);
                            dic.Add(tf.name, atrack.infiniteClip);
                        }
                    }
                }
                string prefix = "Assets/Resources/Animation/";

                foreach (var it in dic)
                {
                    AnimationClip newClip = new AnimationClip();
                    EditorUtility.CopySerializedIfDifferent(it.Value, newClip);
                    string path = prefix + it.Key + ".anim";
                    Debug.Log(path);
                    AssetDatabase.CreateAsset(newClip, path);
                }
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError("timeline asset is null");
            }
        }
        else
        {
            Debug.Log("not found director in the scene");
        }
    }

}
