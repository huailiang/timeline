using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.IO;
using System.Linq;
using UnityEditor;

public class SaveTimeline
{

    [MenuItem("XEditor/Save _F2", priority = 2)]
    public static void Save()
    {
        var dir = GameObject.FindObjectOfType<PlayableDirector>();
        if (dir != null)
        {
            SaveAsset(dir.playableAsset as TimelineAsset);
        }
        else
        {
            EditorUtility.DisplayDialog("tip",
                "There is not direcot in the scene",
                "ok");
        }
    }

    private static void SaveAsset(TimelineAsset asset)
    {
        string path = Application.dataPath + "/Res/" + asset.name + ".byte";
        Debug.Log(path);
        FileStream fs = new FileStream("", FileMode.CreateNew, FileAccess.ReadWrite);
        BinaryWriter bw = new BinaryWriter(fs);
        SaveAsset(asset, bw);
        bw.Close();
        fs.Close();
    }


    private static void SaveAsset(TimelineAsset asset, BinaryWriter bw)
    {
        bw.Write(asset.duration);
        var tracks = asset.GetOutputTracks();
        bw.Write(tracks.Count());
        foreach (var it in tracks)
        {
            SaveTrack(it, bw);
        }
    }


    private static void SaveTrack(TrackAsset track, BinaryWriter bw)
    {
        bw.Write(track.start);
        bw.Write(track.end);

        var clips = track.GetClips();
        bw.Write(clips.Count());
        foreach (var it in clips)
        {
            SaveClip(it, bw);
        }
        var markers = track.GetMarkers();
        foreach (var it in markers)
        {
            SaveMarker(it, bw);
        }
    }


    private static void SaveClip(TimelineClip clip, BinaryWriter bw)
    {
        bw.Write(clip.start);
        bw.Write(clip.end);
        bw.Write(clip.clipIn);
        bw.Write(clip.duration);
        bw.Write(clip.timeScale);
        bw.Write(clip.timeScale);
        bw.Write(clip.blendInDuration);
        bw.Write(clip.blendOutDuration);
        bw.Write(clip.easeInDuration);
        bw.Write(clip.easeOutDuration);
        bw.Write(clip.easeOutTime);
    }


    private static void SaveMarker(IMarker marker, BinaryWriter bw)
    {
        bw.Write(marker.time);
    }

}