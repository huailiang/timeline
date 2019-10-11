using UnityEngine;
using UnityEngine.Timeline;


namespace UnityEditor.Timeline
{

    [CustomTimelineEditor(typeof(AnchorAsset))]
    class AnchorAssetEditor : ClipEditor
    {

        Color32 backg = new Color32(1, 1, 1, 0);
        Color[] cSeq = { Color.red, Color.green, Color.blue };
        AnchorTrack track;


        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
        {
            AnchorAsset asset = clip.asset as AnchorAsset;
            Rect rect = region.position;
            TrackAsset track = clip.parentTrack;
            this.track = track as AnchorTrack;
            if (this.track != null)
            {
                this.track.RebuildClip();
                var quantizedRect = new Rect(Mathf.Ceil(rect.x), Mathf.Ceil(rect.y), Mathf.Ceil(rect.width), Mathf.Ceil(rect.height));
                AnimationCurve[] curves = asset.clip_pos;
                DrawCurve(rect, curves);
            }
            clip.start = 0;
            clip.duration = TimelineEditor.inspectedDirector.duration;
        }


        private void DrawCurve(Rect rect, AnimationCurve[] curves)
        {
            if (curves != null)
            {
                for (int i = 0; i < curves.Length; i++)
                {
                    EditorGUIUtility.DrawCurveSwatch(rect, curves[i], null, cSeq[i], backg);
                }
            }

        }

    }

}