namespace UnityEngine.Timeline
{

    [TrackColor(0.66f, 0.134f, 0.644f)]
    [TrackClipType(typeof(BoneFxAsset))]
    [TrackAttribute(true)]
    public class BoneFxTrack : TrackAsset
    {

        protected override void OnCreateClip(TimelineClip clip)
        {
            clip.duration = 2f;
            base.OnCreateClip(clip);
            ProcessClip(clip);
        }


        protected override void OnAfterTrackDeserialize()
        {
            base.OnAfterTrackDeserialize();
            foreach (var clip in GetClips())
            {
                ProcessClip(clip);
            }
        }


        private void ProcessClip(TimelineClip clip)
        {
            BoneFxAsset asset = clip.asset as BoneFxAsset;
            var bind = DirectorSystem.FetchAttachOfTrack(this);
            asset.SetBind(bind);
            clip.displayName = "BoneFX";
        }


    }

}