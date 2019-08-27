using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


public class ExternalHelp
{

    public static Transform FetchAttachOfTrack(PlayableDirector director, TrackAsset track)
    {
        if (track != null && director != null)
        {
            var binding = director.GetGenericBinding(track);
            if (binding is Animator)
            {
                return (binding as Animator).transform;
            }
            else if (binding is Animation)
            {
                return (binding as Animation).transform;
            }
            else if (binding is GameObject)
            {
                return (binding as GameObject).transform;
            }
        }
        return null;
    }


  

}