using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
    static class AnimationOffsetMenu
    {
        public static GUIContent MatchPreviousMenuItem = EditorGUIUtility.TrTextContent("Match Offsets To Previous Clip");
        public static GUIContent MatchNextMenuItem = EditorGUIUtility.TrTextContent("Match Offsets To Next Clip");
        public static string MatchFieldsPrefix = "Match Offsets Fields/";
        public static GUIContent ResetOffsetMenuItem = EditorGUIUtility.TrTextContent("Reset Offsets");

        static bool EnforcePreviewMode(WindowState state)
        {
            state.previewMode = true; // try and set the preview mode
            if (!state.previewMode)
            {
                Debug.LogError("Match clips cannot be completed because preview mode cannot be enabed");
                return false;
            }
            return true;
        }

        internal static void MatchClipsToPrevious(WindowState state, TimelineClip[] clips)
        {
            if (!EnforcePreviewMode(state))
                return;

            clips = clips.OrderBy(x => x.start).ToArray();
            foreach (var clip in clips)
            {
                var sceneObject = TimelineUtility.GetSceneGameObject(state.editSequence.director, clip.parentTrack);
                if (sceneObject != null)
                {
                    TimelineUndo.PushUndo(clip.asset, "Match Clip");
                    TimelineAnimationUtilities.MatchPrevious(clip, sceneObject.transform, state.editSequence.director);
                }
            }

            InspectorWindow.RepaintAllInspectors();
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
        }

        internal static void MatchClipsToNext(WindowState state, TimelineClip[] clips)
        {
            if (!EnforcePreviewMode(state))
                return;

            clips = clips.OrderByDescending(x => x.start).ToArray();
            foreach (var clip in clips)
            {
                var sceneObject = TimelineUtility.GetSceneGameObject(state.editSequence.director, clip.parentTrack);
                if (sceneObject != null)
                {
                    TimelineUndo.PushUndo(clip.asset, "Match Clip");
                    TimelineAnimationUtilities.MatchNext(clip, sceneObject.transform, state.editSequence.director);
                }
            }

            InspectorWindow.RepaintAllInspectors();
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
        }

        static void ResetClipOffsets(WindowState state, TimelineClip[] clips)
        {
            foreach (var clip in clips)
            {
                if (clip.asset is AnimationPlayableAsset)
                {
                    var playableAsset = (AnimationPlayableAsset)clip.asset;
                    playableAsset.ResetOffsets();
                }
            }
            state.rebuildGraph = true;

            InspectorWindow.RepaintAllInspectors();
            TimelineEditor.Refresh(RefreshReason.SceneNeedsUpdate);
        }

        // Automatically picked up and called by clip menu
        public static void OnClipMenu(WindowState state, TimelineClip[] clips, GenericMenu menu)
        {
            if (state.editSequence.director == null)
                return;

            // find all animation clips that are not the first in the list
            var validClips = clips.Where(c => c.parentTrack != null && state.editSequence.director.GetGenericBinding(c.parentTrack) != null).ToList();
            var validPrevClip = validClips.Where(c => (c.asset as AnimationPlayableAsset) != null && c.parentTrack.clips.Any(x => x.start < c.start)).ToArray();
            var validNextClip = validClips.Where(c => (c.asset as AnimationPlayableAsset) != null && c.parentTrack.clips.Any(x => x.start > c.start)).ToArray();
            if (!validPrevClip.Any() && !validNextClip.Any())
                return;

            menu.AddSeparator("");

            if (validPrevClip.Any())
                menu.AddItem(MatchPreviousMenuItem, false, x => MatchClipsToPrevious(state, (TimelineClip[])x), validPrevClip);
            if (validNextClip.Any())
                menu.AddItem(MatchNextMenuItem, false, x => MatchClipsToNext(state, (TimelineClip[])x), validNextClip);

            menu.AddItem(ResetOffsetMenuItem, false, () => ResetClipOffsets(state, clips));
        }
    }
}
