using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

#if UNITY_EDITOR
using UnityEditor;
#endif


[TrackColor(0.66f, 0.134f, 0.644f)]
[TrackClipType(typeof(BoneFxAsset))]
[TrackBindingType(typeof(GameObject))]
public class BoneFxTrack : TrackAsset
{

    protected override void OnCreateClip(TimelineClip clip)
    {
        clip.duration = 2f;
        base.OnCreateClip(clip);
    }

    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var director = go.GetComponent<PlayableDirector>();
        var list = director.playableAsset.outputs;
        int i = 0;
        bool findObj = false;
        foreach (PlayableBinding pb in director.playableAsset.outputs)
        {
            if (pb.sourceObject is UnityEngine.Timeline.AnimationTrack)
            {
                findObj = true;
                var bind = pb.sourceObject;
                if(bind is GameObject)
                {
                    director.SetGenericBinding(this, (bind as GameObject));
                }
                else if( bind is Animator)
                {
                    director.SetGenericBinding(this, (bind as Animator).gameObject);
                }
                break;
            }
            i++;
        }
        if (!findObj)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayDialog("error", "not found fx root", "ok");
#endif
        }
         return base.CreateTrackMixer(graph, go, inputCount);
    }

}