using System;
using UnityEditor;
using UnityEditor.Timeline;


namespace UnityEngine.Timeline
{

    [CustomTrackDrawer(typeof(TransformTrack))]
    class TransformTrackDrawer : TrackDrawer
    {

        Color32 backg = new Color32(1, 1, 1, 0);
        Color[] cSeq = { Color.red, Color.green, Color.blue };

      

        public override bool DrawTrack(Rect rect, TrackAsset trackAsset, Vector2 visibleTime, WindowState state)
        {
            TransformTrack track = trackAsset as TransformTrack;
            TransformAsset asset = track.GetAsset();
            track.RebuildClip();
            if (asset != null)
            {
                var quantizedRect = new Rect(Mathf.Ceil(rect.x), Mathf.Ceil(rect.y), Mathf.Ceil(rect.width), Mathf.Ceil(rect.height));
                AnimationCurve[] curves = asset.clip;
                DrawCurve(rect, curves);
            }
            return base.DrawTrack(rect, trackAsset, visibleTime, state);
        }

        //protected override void DrawCustomClipBody(ClipDrawData drawData, Rect rect)
        //{
        //    base.DrawCustomClipBody(drawData, rect);

        //    var clip = drawData.clip;
        //    var asset = clip.asset as TransformAsset;

        //    if (target == null && track != null)
        //    {
        //        target = track as TransformTrack;
        //    }
        //    target.RebuildClip();
        //    var quantizedRect = new Rect(Mathf.Ceil(rect.x), Mathf.Ceil(rect.y), Mathf.Ceil(rect.width), Mathf.Ceil(rect.height));
        //    AnimationCurve[] curves = asset.clip;
        //    DrawCurve(rect, curves);
        //}



        private void DrawCurve(Rect rect, AnimationCurve[] curves)
        {
            for (int i = 0; i < curves.Length; i++)
            {
                EditorGUIUtility.DrawCurveSwatch(rect, curves[i], null, cSeq[i], backg);
            }

        }

    }

}