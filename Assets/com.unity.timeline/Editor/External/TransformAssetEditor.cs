using System;
using UnityEditor;
using UnityEditor.Timeline;


namespace UnityEngine.Timeline
{

    [CustomTimelineEditor(typeof(TransformAsset))]
    class TransformAssetEditor : ClipEditor
    {

        Color32 backg = new Color32(1, 1, 1, 0);
        Color[] cSeq = { Color.red, Color.green, Color.blue };
        TransformTrack target;


        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
        {
            TransformAsset asset = clip.asset as TransformAsset;
            Rect rect = region.position;
            TrackAsset track = clip.parentTrack;
            if (target == null && track is TransformTrack)
            {
                target = track as TransformTrack;
            }
            if (target != null)
            {
                target.RebuildClip();
            }
            if (asset != null)
            {
                var quantizedRect = new Rect(Mathf.Ceil(rect.x), Mathf.Ceil(rect.y), Mathf.Ceil(rect.width), Mathf.Ceil(rect.height));
                AnimationCurve[] curves = asset.clip;
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