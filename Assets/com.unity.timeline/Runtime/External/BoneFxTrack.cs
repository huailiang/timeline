using UnityEngine.Timeline;

#if UNITY_EDITOR
#endif


[TrackColor(0.66f, 0.134f, 0.644f)]
[TrackClipType(typeof(BoneFxAsset))]
[TrackAttribute(true)]
public class BoneFxTrack : TrackAsset
{

    protected override void OnCreateClip(TimelineClip clip)
    {
        clip.duration = 2f;
        base.OnCreateClip(clip);
        BoneFxAsset asset = clip.asset as BoneFxAsset;
        asset.track = this.parent as TrackAsset;
    }


}