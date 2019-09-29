using System.IO;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineLoader
{

    private XPlayableAsset asset;

    public void Load(string path, PlayableDirector director)
    {
        FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read);
        BinaryReader br = new BinaryReader(stream);
        asset = Load(br, director);
        OnPostLoad();
        director.playableAsset = asset;
        br.Close();
        stream.Close();
    }


    private void OnPostLoad()
    {
        var tracks = asset.TrackAssets;
        for (int i = 0; i < tracks.Length; i++)
        {
            tracks[i].OnPostLoad(asset);
        }
    }


    private XPlayableAsset Load(BinaryReader reader, PlayableDirector director)
    {
        var asset = ScriptableObject.CreateInstance<XPlayableAsset>();
        var dur = reader.ReadDouble();
        asset.SetDuration(dur);
        int cnt = reader.ReadInt32();
        asset.TrackAssets = new XTrackAsset[cnt];
        for (int i = 0; i < cnt; i++)
        {
            LoadTrack(reader, ref asset.TrackAssets[i], director);
        }
        return asset;
    }


    private void LoadTrack(BinaryReader reader, ref XTrackAsset track, PlayableDirector director)
    {
        track = ScriptableObject.CreateInstance<XTrackAsset>();
        track.Load(reader);
        Debug.Log("track " + track.trackType + " " + track.duration.ToString("f2"));
        string bind = reader.ReadString();
        if (!string.IsNullOrEmpty(bind))
        {
            GameObject bindGo = GameObject.Find(bind);
            director.SetGenericBinding(track, bindGo);
        }

        //clips
        int cnt = reader.ReadInt32();
        for (int i = 0; i < cnt; i++)
        {
            LoadClip(reader, track);
        }

        //markers
        cnt = reader.ReadInt32();
        for (int i = 0; i < cnt; i++)
        {
            LoadMarker(reader, track);
        }
    }


    private void LoadClip(BinaryReader reader, XTrackAsset track)
    {
        PlayableAsset asset = DirectorSystem.CreateClipAsset(track.trackType);
        TimelineClip clip = track.CreateClip(asset);
        clip.start = reader.ReadDouble();
        clip.clipIn = reader.ReadDouble();
        clip.duration = reader.ReadDouble();
        clip.timeScale = reader.ReadDouble();
        clip.blendInDuration = reader.ReadDouble();
        clip.blendOutDuration = reader.ReadDouble();
        clip.easeInDuration = reader.ReadDouble();
        clip.easeOutDuration = reader.ReadDouble();
        if (asset is IDirectorIO)
        {
            var io = asset as IDirectorIO;
            io.Load(reader);
        }
    }


    private void LoadMarker(BinaryReader reader, XTrackAsset track)
    {
        double time = reader.ReadDouble();
        int type = reader.ReadInt32();
        int parent = reader.ReadInt32();
        Marker marker = DirectorSystem.CreateMarker((MarkType)type);
        if (marker is IDirectorIO)
        {
            var io = marker as IDirectorIO;
            io.Load(reader);
        }
        if (marker)
        {
            track.AddMarker(marker);
        }
    }

}