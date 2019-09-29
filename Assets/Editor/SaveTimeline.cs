using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.IO;
using System.Linq;
using UnityEditor;
using System.Collections.Generic;

public class SaveTimeline
{

    private static PlayableDirector director;
    private static List<TrackAsset> m_tracks = new List<TrackAsset>();
    private static Dictionary<string, Object> bindingDict = new Dictionary<string, Object>();

    [MenuItem("XEditor/Save _F2", priority = 2)]
    public static void Save()
    {
        director = GameObject.FindObjectOfType<PlayableDirector>();
        if (director != null)
        {
            AnalyBinding(director.playableAsset);
            SaveAsset(director.playableAsset as TimelineAsset);
        }
        else
        {
            EditorUtility.DisplayDialog("tip",
                "There is not direcot in the scene", "ok");
        }
    }

    private static void SaveAsset(TimelineAsset asset)
    {
        string path = Application.dataPath + "/Res/" + asset.name + ".byte";
        Debug.Log(path);
        FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        BinaryWriter bw = new BinaryWriter(fs);
        SaveAsset(asset, bw);
        bw.Close();
        fs.Close();
        m_tracks.Clear();
        AssetDatabase.Refresh();
        Debug.Log("export timeline success");
    }


    private static void SaveAsset(TimelineAsset asset, BinaryWriter bw)
    {
        AnalyTrack(asset);

        bw.Write(asset.duration);
        bw.Write(m_tracks.Count());
        foreach (var it in m_tracks)
        {
            SaveTrack(it, bw);
        }
    }

    private static void AnalyTrack(TimelineAsset asset)
    {
        m_tracks.Clear();
        var tracks = asset.GetRootTracks();
        foreach (var track in tracks)
        {
            m_tracks.Add(track);
            AnalyTrack(track);
        }
    }


    private static void SaveTrack(TrackAsset track, BinaryWriter bw)
    {
        bw.Write(track.start);
        bw.Write(track.end);
        bw.Write(track.name);
        var type = DirectorSystem.UtilTrackType(track);
        bw.Write((int)type);
        int parent = m_tracks.IndexOf(track.parent as TrackAsset);
        bw.Write(parent);
        Object bindObj = director.GetGenericBinding(bindingDict[track.name]);
        string bind = bindObj ? bindObj.name : "";
        bw.Write(bind);

        Debug.Log("track: " + track.name + " " + type + " " + parent + " " + bind);

        //track clips
        track.SortClips();
        var clips = track.GetClips();
        bw.Write(clips.Count());
        foreach (var it in clips)
        {
            SaveClip(it, bw);
        }

        //track markers
        var markers = track.GetMarkers();
        foreach (var it in markers)
        {
            SaveMarker(it, bw);
        }
    }


    private static void SaveClip(TimelineClip clip, BinaryWriter bw)
    {
        bw.Write(clip.start);
        bw.Write(clip.clipIn);
        bw.Write(clip.duration);
        bw.Write(clip.timeScale);
        bw.Write(clip.blendInDuration);
        bw.Write(clip.blendOutDuration);
        bw.Write(clip.easeInDuration);
        bw.Write(clip.easeOutDuration);
        if (clip.asset is IDirectorIO)
        {
            var io = clip.asset as IDirectorIO;
            io.Write(bw);
        }
    }


    private static void SaveMarker(IMarker marker, BinaryWriter bw)
    {
        MarkType type = MarkType.NONE;
        if (marker is IXMarker)
        {
            type = (marker as IXMarker).markType;
        }
        int parent = m_tracks.IndexOf(marker.parent);
        // Debug.Log("marker: " + type + " " + parent);
        bw.Write(marker.time);
        bw.Write((int)type);
        bw.Write(parent);
        if (marker is IDirectorIO)
        {
            var io = marker as IDirectorIO;
            io.Write(bw);
        }
    }


    private static void AnalyTrack(TrackAsset track)
    {
        var childs = track.GetChildTracks();
        foreach (var it in childs)
        {
            m_tracks.Add(it);
            AnalyTrack(it);
        }
    }


    private static void AnalyBinding(PlayableAsset asset)
    {
        bindingDict = asset.outputs.
            ToDictionary(k => k.streamName, v => v.sourceObject);
    }

}