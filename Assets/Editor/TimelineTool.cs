using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineTool
{

    [MenuItem("XEditor/Save _F2", priority = 2)]
    public static void Save()
    {
        var director = GameObject.FindObjectOfType<PlayableDirector>();
        TimelineSaver.Save(director);
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


    public static void ExportRecord()
    {
        var sel = Selection.activeObject;
        if (sel != null)
        {
            if (sel is TimelineAsset)
            {
                TimelineAsset asset = sel as TimelineAsset;
                ExportRecord(asset);
            }
        }
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

}
