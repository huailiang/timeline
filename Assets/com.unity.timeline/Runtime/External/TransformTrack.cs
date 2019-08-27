using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackClipType(typeof(TransformAsset))]
[TrackColor(0.76f, 0.44f, 0.24f)]
public class TransformTrack : TrackAsset
{

    List<TransforSignalEmitter> signals;
    AnimationCurve[] curves;

    protected override void OnAfterTrackDeserialize()
    {
        base.OnAfterTrackDeserialize();
        OutputTrackinfo();
    }


    private void OutputTrackinfo()
    {
        if (signals == null)
        {
            signals = new List<TransforSignalEmitter>();
        }
        else signals.Clear();
        var marks = GetMarkers().GetEnumerator();
        while (marks.MoveNext())
        {
            IMarker mark = marks.Current;
            if (mark is TransforSignalEmitter)
            {
                signals.Add(mark as TransforSignalEmitter);
            }
        }
        marks.Dispose();

        CreateClips();
    }


    private void CreateClips()
    {
        if (signals != null && signals.Count > 0)
        {
            if (curves == null)
            {
                curves = new AnimationCurve[3];
                for (int i = 0; i < 3; i++)
                {
                    curves[i] = new AnimationCurve();
                }
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    curves[i].keys = null;
                }
            }
            for (int i = 0; i < signals.Count; i++)
            {
                TransforSignalEmitter sign = signals[i];
                curves[0].AddKey((float)sign.time, sign.position.x);
                curves[1].AddKey((float)sign.time, sign.position.y);
                curves[2].AddKey((float)sign.time, sign.position.z);
            }
            var clips = GetClips();
            if (clips != null)
            {
                TimelineClip xclip = clips.FirstOrDefault();
                if (xclip == null)
                {
                    Debug.LogError("transform clip con't be null");
                }
                else
                {
                    TransformAsset asset = xclip.asset as TransformAsset;
                    asset.clip = curves;
                    BindObj(asset);
                }
            }
        }
    }

    private void BindObj(TransformAsset asset)
    {
        var track = parent as TrackAsset;
        asset.parent = track;
    }


    public TransformAsset GetAsset()
    {
        var clips = GetClips();
        if (clips.Count() > 0)
        {
            TimelineClip xclip = clips.FirstOrDefault();
            if (xclip != null)
            {
                return xclip.asset as TransformAsset;
            }
        }
        return null;
    }


    public void RebuildClip()
    {
        CreateClips();
    }

}