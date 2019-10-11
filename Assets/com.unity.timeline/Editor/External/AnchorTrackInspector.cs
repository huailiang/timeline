using UnityEngine;
using UnityEngine.Timeline;
using System.Linq;

namespace UnityEditor.Timeline
{
    [CustomEditor(typeof(AnchorTrack))]
    class AnchorTrackInspector : TrackAssetInspector
    {

        AnchorTrack track;

        public override void OnEnable()
        {
            base.OnEnable();
            track = target as AnchorTrack;
            EnsureClip();
        }


        private void EnsureClip()
        {
            var clips = track.GetClips();
            if (clips == null || clips.Count() <= 0)
            {
                var clip = track.CreateClipOfType(typeof(AnchorAsset));
                clip.displayName = " ";
                clip.start = 0.0f;
                var director = TimelineEditor.inspectedDirector;
                clip.duration = director.duration;
                clip.timeScale = 1.0;
                clip.parentTrack = track;
                TimelineHelpers.CreateClipOnTrack(clip.asset, track, 0);
            }
        }



        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Label("AnchorTrack");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Rebuild", GUILayout.Width(120)))
            {
                track.OutputTrackinfo();
            }
            GUILayout.Space(40);
            if (GUILayout.Button("Add Signal", GUILayout.Width(120)))
            {
                track.CreateAnchor();
            }
            GUILayout.EndHorizontal();

            var clip = track.GetClips().FirstOrDefault();
            if (clip != null)
            {
                clip.start = 0;
                clip.duration = TimelineEditor.inspectedDirector.duration;
            }
        }

    }

}
