using System.IO;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class LoadTimeline
{

    private XPlayableAsset asset;

    public void Load(string path, PlayableDirector director)
    {
        FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read);
        BinaryReader br = new BinaryReader(stream);
        asset = Load(br);
        director.playableAsset = asset;
        br.Close();
        stream.Close();
    }


    private XPlayableAsset Load(BinaryReader reader)
    {
        var asset = ScriptableObject.CreateInstance<XPlayableAsset>();
        var dur = reader.ReadDouble();
        asset.SetDuration(dur);
        int cnt = reader.ReadInt32();
        asset.TrackAssets = new XTrackAsset[cnt];
        for (int i = 0; i < cnt; i++)
        {
            LoadTrack(reader, asset.TrackAssets[i]);
        }
        return asset;
    }



    private void LoadTrack(BinaryReader reader, XTrackAsset track)
    {
        track = ScriptableObject.CreateInstance<XTrackAsset>();
        track.Load(reader);
        int parent = reader.ReadInt32();
        if (parent >= 0)
        {
            XTrackAsset parentTrack = asset.TrackAssets[parent];
            track.parent = parentTrack;
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
        TimelineClip clip = track.CreateAndAddNewClipOfType(typeof(TimelineClip));
        clip.start = reader.ReadDouble();
        clip.clipIn = reader.ReadDouble();
        clip.duration = reader.ReadDouble();
        clip.timeScale = reader.ReadDouble();
        clip.blendInDuration = reader.ReadDouble();
        clip.blendOutDuration = reader.ReadDouble();
        clip.easeInDuration = reader.ReadDouble();
        clip.easeOutDuration = reader.ReadDouble();
        track.AddClip(clip);
    }


    private void LoadMarker(BinaryReader reader, XTrackAsset track)
    {
        double time = reader.ReadDouble();
        int type = reader.ReadInt32();
        int parent = reader.ReadInt32();
        Marker marker = CreateMarker((MarkType)type);
        if (marker)
        {
            track.AddMarker(marker);
        }
    }


    private Marker CreateMarker(MarkType type)
    {
        Marker marker = null;
        switch (type)
        {
            case MarkType.ACTIVE:
                marker = ScriptableObject.CreateInstance<ActiveSignalEmmiter>();
                break;
            case MarkType.ANCHOR:
                marker = ScriptableObject.CreateInstance<AnchorSignalEmitter>();
                break;
            case MarkType.JUMP:
                marker = ScriptableObject.CreateInstance<JumpSignalEmmiter>();
                break;
            case MarkType.SLOW:
                marker = ScriptableObject.CreateInstance<SlowSignalEmitter>();
                break;
        }
        return marker;
    }

}