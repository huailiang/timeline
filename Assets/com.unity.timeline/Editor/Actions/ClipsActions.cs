using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using ClipAction = UnityEditor.Timeline.ItemAction<UnityEngine.Timeline.TimelineClip>;

namespace UnityEditor.Timeline
{
    [DisplayName("Edit in Animation Window")]
    [SeparatorMenuItem(SeparatorMenuItemPosition.After)]
    class EditClipInAnimationWindow : ClipAction
    {
        protected override MenuActionDisplayState GetDisplayState(WindowState state, TimelineClip[] clips)
        {
            if (clips.Length == 1 && clips[0].animationClip != null)
                return MenuActionDisplayState.Visible;
            return MenuActionDisplayState.Hidden;
        }

        public override bool Execute(WindowState state, TimelineClip[] clips)
        {
            var clip = clips[0];

            if (clip.curves != null || clip.animationClip != null)
            {
                var clipToEdit = clip.animationClip != null ? clip.animationClip : clip.curves;
                if (clipToEdit == null)
                    return false;

                var gameObject = state.GetSceneReference(clip.parentTrack);
                var timeController = TimelineAnimationUtilities.CreateTimeController(state, clip);
                TimelineAnimationUtilities.EditAnimationClipWithTimeController(
                    clipToEdit, timeController, clip.animationClip != null  ? gameObject : null);
                return true;
            }

            return false;
        }
    }

    [DisplayName("Edit Sub-Timeline")]
    [SeparatorMenuItem(SeparatorMenuItemPosition.After)]
    class EditSubTimeline : ClipAction
    {
        protected override MenuActionDisplayState GetDisplayState(WindowState state, TimelineClip[] clips)
        {
            return IsValid(state, clips) ? MenuActionDisplayState.Visible : MenuActionDisplayState.Hidden;
        }

        bool IsValid(WindowState state, TimelineClip[] clips)
        {
            if (clips.Length != 1 || state == null || state.editSequence.director == null) return false;
            var clip = clips[0];

            var directors = TimelineUtility.GetSubTimelines(clip, state.editSequence.director);
            return directors.Any(x => x != null);
        }

        public override bool Execute(WindowState state, TimelineClip[] clips)
        {
            if (!IsValid(state, clips)) return false;

            var clip = clips[0];

            var directors = TimelineUtility.GetSubTimelines(clip, state.editSequence.director);
            ExecuteInternal(state, directors, 0, clip);

            return true;
        }

        static void ExecuteInternal(WindowState state, IList<PlayableDirector> directors, int directorIndex, TimelineClip clip)
        {
            SelectionManager.Clear();
            state.GetWindow().SetCurrentTimeline(directors[directorIndex], clip);
        }

        protected override void AddSelfToMenu(GenericMenu menu, WindowState state, TimelineClip[] items)
        {
            if (GetDisplayState(state, items) == MenuActionDisplayState.Hidden)
                return;

            // GetDisplayState() guarantees that all that follows is valid
            var clip = items[0];
            var subDirectors = TimelineUtility.GetSubTimelines(clip, state.editSequence.director);
            var separator = GetSeparator(this);

            if (separator != null && separator.before)
                menu.AddSeparator("");

            if (subDirectors.Count == 1)
            {
                menu.AddItem(new GUIContent("Edit " + DisplayNameHelper.GetDisplayName(subDirectors[0])), false,
                    f =>
                    {
                        ExecuteInternal(state, subDirectors, 0, clip);
                    }, this);
            }
            else
            {
                const string prefix = "Edit Sub-Timelines/";
                for (int i = 0, n = subDirectors.Count; i < n; i++)
                {
                    var directorIndex = i;
                    menu.AddItem(
                        new GUIContent(prefix + DisplayNameHelper.GetDisplayName(subDirectors[directorIndex])), false,
                        f =>
                        {
                            ExecuteInternal(state, subDirectors, directorIndex, clip);
                        },
                        this);
                }
            }

            if (separator != null && separator.after)
                menu.AddSeparator("");
        }
    }

    [Category("Editing/")]
    [DisplayName("Trim Start")]
    [Shortcut(Shortcuts.Clip.trimStart)]
    class TrimStart : ItemAction<TimelineClip>
    {
        protected override MenuActionDisplayState GetDisplayState(WindowState state, TimelineClip[] clips)
        {
            return clips.All(x => state.editSequence.time <= x.start || state.editSequence.time >= x.start + x.duration) ?
                MenuActionDisplayState.Disabled : MenuActionDisplayState.Visible;
        }

        public override bool Execute(WindowState state, TimelineClip[] clips)
        {
            return ClipModifier.TrimStart(clips, state.editSequence.time);
        }
    }

    [Category("Editing/")]
    [DisplayName("Trim End")]
    [Shortcut(Shortcuts.Clip.trimEnd)]
    class TrimEnd : ItemAction<TimelineClip>
    {
        protected override MenuActionDisplayState GetDisplayState(WindowState state, TimelineClip[] clips)
        {
            return clips.All(x => state.editSequence.time <= x.start || state.editSequence.time >= x.start + x.duration) ?
                MenuActionDisplayState.Disabled : MenuActionDisplayState.Visible;
        }

        public override bool Execute(WindowState state, TimelineClip[] clips)
        {
            return ClipModifier.TrimEnd(clips, state.editSequence.time);
        }
    }

    [Category("Editing/")]
    [Shortcut(Shortcuts.Clip.split)]
    class Split : ClipAction
    {
        protected override MenuActionDisplayState GetDisplayState(WindowState state, TimelineClip[] clips)
        {
            return clips.All(x => state.editSequence.time <= x.start || state.editSequence.time >= x.start + x.duration) ?
                MenuActionDisplayState.Disabled : MenuActionDisplayState.Visible;
        }

        public override bool Execute(WindowState state, TimelineClip[] clips)
        {
            bool success = ClipModifier.Split(clips, state.editSequence.time, state.editSequence.director);
            if (success)
                state.Refresh();
            return success;
        }
    }

    [Category("Editing/")]
    [DisplayName("Complete Last Loop")]
    [SeparatorMenuItem(SeparatorMenuItemPosition.Before)]
    class CompleteLastLoop : ClipAction
    {
        protected override MenuActionDisplayState GetDisplayState(WindowState state, TimelineClip[] clips)
        {
            bool canDisplay = clips.Any(TimelineHelpers.HasUsableAssetDuration);
            return canDisplay ? MenuActionDisplayState.Visible : MenuActionDisplayState.Disabled;
        }

        public override bool Execute(WindowState state, TimelineClip[] clips)
        {
            return ClipModifier.CompleteLastLoop(clips);
        }
    }

    [Category("Editing/")]
    [DisplayName("Trim Last Loop")]
    class TrimLastLoop : ClipAction
    {
        protected override MenuActionDisplayState GetDisplayState(WindowState state, TimelineClip[] clips)
        {
            bool canDisplay = clips.Any(TimelineHelpers.HasUsableAssetDuration);
            return canDisplay ? MenuActionDisplayState.Visible : MenuActionDisplayState.Disabled;
        }

        public override bool Execute(WindowState state, TimelineClip[] clips)
        {
            return ClipModifier.TrimLastLoop(clips);
        }
    }

    [Category("Editing/")]
    [DisplayName("Match Duration")]
    class MatchDuration : ClipAction
    {
        protected override MenuActionDisplayState GetDisplayState(WindowState state, TimelineClip[] clips)
        {
            return clips.Length > 1 ? MenuActionDisplayState.Visible : MenuActionDisplayState.Disabled;
        }

        public override bool Execute(WindowState state, TimelineClip[] clips)
        {
            return ClipModifier.MatchDuration(clips);
        }
    }

    [Category("Editing/")]
    [DisplayName("Double Speed")]
    [SeparatorMenuItem(SeparatorMenuItemPosition.Before)]
    class DoubleSpeed : ClipAction
    {
        protected override MenuActionDisplayState GetDisplayState(WindowState state, TimelineClip[] clips)
        {
            bool canDisplay = clips.All(x => x.SupportsSpeedMultiplier());

            return canDisplay ? MenuActionDisplayState.Visible : MenuActionDisplayState.Disabled;
        }

        public override bool Execute(WindowState state, TimelineClip[] clips)
        {
            return ClipModifier.DoubleSpeed(clips);
        }
    }

    [Category("Editing/")]
    [DisplayName("Half Speed")]
    class HalfSpeed : ClipAction
    {
        protected override MenuActionDisplayState GetDisplayState(WindowState state, TimelineClip[] clips)
        {
            bool canDisplay = clips.All(x => x.SupportsSpeedMultiplier());

            return canDisplay ? MenuActionDisplayState.Visible : MenuActionDisplayState.Disabled;
        }

        public override bool Execute(WindowState state, TimelineClip[] clips)
        {
            return ClipModifier.HalfSpeed(clips);
        }
    }

    [Category("Editing/")]
    [DisplayName("Reset Duration")]
    [SeparatorMenuItem(SeparatorMenuItemPosition.Before)]
    class ResetDuration : ClipAction
    {
        protected override MenuActionDisplayState GetDisplayState(WindowState state, TimelineClip[] clips)
        {
            bool canDisplay = clips.Any(TimelineHelpers.HasUsableAssetDuration);
            return canDisplay ? MenuActionDisplayState.Visible : MenuActionDisplayState.Disabled;
        }

        public override bool Execute(WindowState state, TimelineClip[] clips)
        {
            return ClipModifier.ResetEditing(clips);
        }
    }

    [Category("Editing/")]
    [DisplayName("Reset Speed")]
    class ResetSpeed : ClipAction
    {
        protected override MenuActionDisplayState GetDisplayState(WindowState state, TimelineClip[] clips)
        {
            bool canDisplay = clips.All(x => x.SupportsSpeedMultiplier());

            return canDisplay ? MenuActionDisplayState.Visible : MenuActionDisplayState.Disabled;
        }

        public override bool Execute(WindowState state, TimelineClip[] clips)
        {
            return ClipModifier.ResetSpeed(clips);
        }
    }

    [Category("Editing/")]
    [DisplayName("Reset All")]
    class ResetAll : ClipAction
    {
        protected override MenuActionDisplayState GetDisplayState(WindowState state, TimelineClip[] clips)
        {
            bool canDisplay = clips.Any(TimelineHelpers.HasUsableAssetDuration) ||
                clips.All(x => x.SupportsSpeedMultiplier());

            return canDisplay ? MenuActionDisplayState.Visible : MenuActionDisplayState.Disabled;
        }

        public override bool Execute(WindowState state, TimelineClip[] clips)
        {
            var speedResult = ClipModifier.ResetSpeed(clips);
            var editResult = ClipModifier.ResetEditing(clips);
            return speedResult || editResult;
        }
    }

    class Tile : ClipAction
    {
        protected override MenuActionDisplayState GetDisplayState(WindowState state, TimelineClip[] clips)
        {
            return clips.Length > 1 ? MenuActionDisplayState.Visible : MenuActionDisplayState.Disabled;
        }

        public override bool Execute(WindowState state, TimelineClip[] clips)
        {
            return ClipModifier.Tile(clips);
        }
    }

    [DisplayName("Find Source Asset")]
    [ActiveInMode(TimelineModes.Default | TimelineModes.ReadOnly)]
    class FindSourceAsset : ClipAction
    {
        protected override MenuActionDisplayState GetDisplayState(WindowState state,
            TimelineClip[] clips)
        {
            if (clips.Length > 1)
                return MenuActionDisplayState.Disabled;

            if (GetUnderlyingAsset(state, clips[0]) == null)
                return MenuActionDisplayState.Disabled;

            return MenuActionDisplayState.Visible;
        }

        public override bool Execute(WindowState state, TimelineClip[] clips)
        {
            EditorGUIUtility.PingObject(GetUnderlyingAsset(state, clips[0]));
            return true;
        }

        private static UnityEngine.Object GetExternalPlayableAsset(TimelineClip clip)
        {
            if (clip.asset == null)
                return null;

            if ((clip.asset.hideFlags & HideFlags.HideInHierarchy) != 0)
                return null;

            return clip.asset;
        }

        private static UnityEngine.Object GetUnderlyingAsset(WindowState state, TimelineClip clip)
        {
            var asset = clip.asset as ScriptableObject;
            if (asset == null)
                return null;

            var fields = ObjectReferenceField.FindObjectReferences(asset.GetType());
            if (fields.Length == 0)
                return GetExternalPlayableAsset(clip);

            // Find the first non-null field
            foreach (var field in fields)
            {
                // skip scene refs in asset mode
                if (state.editSequence.director == null && field.isSceneReference)
                    continue;
                var obj = field.Find(asset, state.editSequence.director);
                if (obj != null)
                    return obj;
            }

            return GetExternalPlayableAsset(clip);
        }
    }

    [HideInMenu]
    class CopyClipsToClipboard : ClipAction
    {
        public override bool Execute(WindowState state, TimelineClip[] clips)
        {
            TimelineEditor.clipboard.CopyItems(clips.ToItems());
            return true;
        }
    }
}
