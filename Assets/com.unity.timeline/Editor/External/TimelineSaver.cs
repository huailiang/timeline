using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineSaver
{

    private static PlayableDirector director;
    private static List<TrackAsset> m_tracks = new List<TrackAsset>();
    private static Dictionary<string, Object> bindingDict = new Dictionary<string, Object>();

    public static void Save(PlayableDirector dir)
    {
        if (dir != null)
        {
            director = dir;
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
        string path = Application.dataPath + "/Res/" + asset.name + ".bytes";
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
    

    private static void SaveTrack(TrackAsset track, BinaryWriter bw)
    {
        bw.Write(track.name);
        var type = DirectorSystem.UtilTrackType(track);
        bw.Write((int)type);
        bw.Write(track.start);
        bw.Write(track.end);
       
        int parent = m_tracks.IndexOf(track.parent as TrackAsset);
        bw.Write(parent);
        bw.Write(track.mutedInHierarchy);
        Object bindObj = director.GetGenericBinding(bindingDict[track.name]);
        string bind = bindObj ? bindObj.name : "";
        bw.Write(bind);

        Debug.Log("track: " + " " + type + " " + parent + " " + bind);

        //track clips
        track.SortClips();
        var clips = track.GetClips();
        bw.Write(IsinfiniteClip(track) ? 1 : clips.Count());
        foreach (var it in clips)
        {
            SaveClip(it, bw);
        }
        SaveinfiniteClip(track, bw);

        //track markers
        var markers = track.GetMarkers();
        bw.Write(markers.Count());
        foreach (var it in markers)
        {
            SaveMarker(it, bw);
        }
    }

    private static bool IsinfiniteClip(TrackAsset track)
    {
        AnimationTrack atrack = track as AnimationTrack;
        return (atrack != null && atrack.infiniteClip != null);
    }

    private static void SaveinfiniteClip(TrackAsset track, BinaryWriter bw)
    {
        if (IsinfiniteClip(track))
        {
            AnimationTrack atrack = track as AnimationTrack;
            var clip = atrack.infiniteClip;
            bw.Write(0d);
            bw.Write(0d);
            bw.Write(DirectorSystem.Director.duration);
            bw.Write(1.0d);
            bw.Write(0d);
            bw.Write(0d);
            bw.Write(0d);
            bw.Write(0d);
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
        Debug.Log("marker: " + type + " " + parent);
        bw.Write(marker.time);
        bw.Write((int)type);
        bw.Write(parent);
        if (marker is IDirectorIO)
        {
            var io = marker as IDirectorIO;
            io.Write(bw);
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


    /// <summary>
    /// 这里主要是为了给导出的track 按照树的结果自上而下排序
    /// </summary>
    /// <param name="track"></param>
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