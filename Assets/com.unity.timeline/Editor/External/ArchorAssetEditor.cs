using System;
using UnityEditor;
using UnityEditor.Timeline;


namespace UnityEngine.Timeline
{

    [CustomTimelineEditor(typeof(ArchorAsset))]
    class ArchorAssetEditor : ClipEditor
    {

        Color32 backg = new Color32(1, 1, 1, 0);
        Color[] cSeq = { Color.red, Color.green, Color.blue };
        ArchorTrack target;


        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
        {
            ArchorAsset asset = clip.asset as ArchorAsset;
            Rect rect = region.position;
            TrackAsset track = clip.parentTrack;
            if (target == null && track is ArchorTrack)
            {
                target = track as ArchorTrack;
            }
            if (target != null)
            {
                target.RebuildClip();
            }
            if (asset != null)
            {
                var quantizedRect = new Rect(Mathf.Ceil(rect.x), Mathf.Ceil(rect.y), Mathf.Ceil(rect.width), Mathf.Ceil(rect.height));
                AnimationCurve[] curves = asset.clip_pos;
                DrawCurve(rect, curves);
            }
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