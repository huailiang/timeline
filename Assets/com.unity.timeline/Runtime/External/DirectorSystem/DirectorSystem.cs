using UnityEngine;
using System;
using System.IO;
using UnityEngine.Timeline;


public enum XTrackType
{
    MARKER = 0,
    ANIMATION,
    ACTIVE,
    CONTROL,
    AUDIO,
    ANCHOR,
    BONEFX,
    NONE = 100
}


public interface IDirector
{
    void Load(BinaryReader reader);


    void OnSave(BinaryWriter writer);

}

public class DirectorSystem
{


    public static XTrackType UtilTrackType(TrackAsset track)
    {
        if (track is AudioTrack)
        {
            return XTrackType.AUDIO;
        }
        if (track is AnimationTrack)
        {
            return XTrackType.ANIMATION;
        }
        if (track is ControlTrack)
        {
            return XTrackType.CONTROL;
        }
        if (track is ActivationTrack)
        {
            return XTrackType.ACTIVE;
        }
        if (track is AnchorTrack)
        {
            return XTrackType.ANCHOR;
        }
        if (track is BoneFxTrack)
        {
            return XTrackType.BONEFX;
        }
        return XTrackType.NONE;
    }


    public static Type UtilTrackType(XTrackType type)
    {
        Type tp = null;
        switch (type)
        {
            case XTrackType.ACTIVE:
                tp = typeof(GameObject);
                break;
            case XTrackType.ANIMATION:
                tp = typeof(Animator);
                break;
            case XTrackType.AUDIO:
                tp = typeof(AudioListener);
                break;
        }
        return tp;
    }





}
