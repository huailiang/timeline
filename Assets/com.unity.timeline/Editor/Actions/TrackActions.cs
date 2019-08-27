using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
    [ActiveInMode(TimelineModes.Default)]
    abstract class TrackAction : MenuItemActionBase
    {
        public abstract bool Execute(WindowState state, TrackAsset[] tracks);

        protected virtual MenuActionDisplayState GetDisplayState(WindowState state, TrackAsset[] tracks)
        {
            return tracks.Length > 0 ? MenuActionDisplayState.Visible : MenuActionDisplayState.Disabled;
        }

        protected virtual bool IsChecked(WindowState state, TrackAsset[] tracks)
        {
            return false;
        }

        protected virtual string GetDisplayName(WindowState state, TrackAsset[] tracks)
        {
            return GetDisplayName(this);
        }

        public static void Invoke<T>(WindowState state, TrackAsset[] tracks) where T : TrackAction
        {
            actions.First(x => x.GetType() == typeof(T)).Execute(state, tracks);
        }

        static List<TrackAction> s_ActionClasses;

        static List<TrackAction> actions
        {
            get
            {
                if (s_ActionClasses == null)
                    s_ActionClasses =
                        GetActionsOfType(typeof(TrackAction))
                            .Select(x => (TrackAction)x.GetConstructors()[0].Invoke(null))
                            .ToList();

                return s_ActionClasses;
            }
        }

        protected Vector2? mousePosition { get; private set; }

        public static void AddToMenu(GenericMenu menu, WindowState state)
        {
            var tracks = SelectionManager.SelectedTracks().ToArray();

            if (tracks.Length == 0)
                return;

            actions.ForEach(action =>
            {
                var subMenuPath = string.Empty;
                var categoryAttr = GetCategoryAttribute(action);

                if (categoryAttr != null)
                {
                    subMenuPath = categoryAttr.Category;
                    if (!subMenuPath.EndsWith("/"))
                        subMenuPath += "/";
                }

                string displayName = action.GetDisplayName(state, tracks);
                string menuItemName = subMenuPath + displayName;
                var separator = GetSeparator(action);
                var canBeAddedToMenu = !TypeUtility.IsHiddenInMenu(action.GetType());

                if (canBeAddedToMenu)
                {
                    Vector2? currentMousePosition = null;
                    if (Event.current != null)
                        currentMousePosition = Event.current.mousePosition;

                    action.mousePosition = currentMousePosition;
                    var displayState = action.GetDisplayState(state, tracks);
                    if (displayState == MenuActionDisplayState.Visible && !IsActionActiveInMode(action, TimelineWindow.instance.currentMode.mode))
                    {
                        displayState = MenuActionDisplayState.Disabled;
                    }
                    action.mousePosition = null;

                    if (displayState == MenuActionDisplayState.Visible)
                    {
                        menu.AddItem(new GUIContent(menuItemName), action.IsChecked(state, tracks), f =>
                        {
                            action.Execute(state, tracks);
                        }, action);
                    }

                    if (displayState == MenuActionDisplayState.Disabled)
                        menu.AddDisabledItem(new GUIContent(menuItemName), action.IsChecked(state, tracks));

                    if (displayState != MenuActionDisplayState.Hidden && separator != null && separator.after)
                        menu.AddSeparator(subMenuPath);
                }
            });
        }

        public static bool HandleShortcut(WindowState state, Event evt, TrackAsset[] tracks)
        {
            foreach (var action in actions)
            {
                var attr = action.GetType().GetCustomAttributes(typeof(ShortcutAttribute), true);

                foreach (ShortcutAttribute shortcut in attr)
                {
                    if (shortcut.MatchesEvent(evt))
                    {
                        if (s_ShowActionTriggeredByShortcut)
                            Debug.Log(action.GetType().Name);

                        if (!IsActionActiveInMode(action, TimelineWindow.instance.currentMode.mode))
                            return false;

                        return action.Execute(state, tracks);
                    }
                }
            }

            return false;
        }
    }

    [DisplayName("Edit in Animation Window")]
    [SeparatorMenuItem(SeparatorMenuItemPosition.After)]
    class EditTrackInAnimationWindow : TrackAction
    {
        public static bool Do(WindowState state, TrackAsset track)
        {
            AnimationClip clipToEdit = null;

            AnimationTrack animationTrack = track as AnimationTrack;
            if (animationTrack != null)
            {
                if (!animationTrack.CanConvertToClipMode())
                    return false;

                clipToEdit = animationTrack.infiniteClip;
            }
            else if (track.hasCurves)
            {
                clipToEdit = track.curves;
            }

            if (clipToEdit == null)
                return false;

            var gameObject = state.GetSceneReference(track);
            var timeController = TimelineAnimationUtilities.CreateTimeController(state, CreateTimeControlClipData(track));
            TimelineAnimationUtilities.EditAnimationClipWithTimeController(clipToEdit, timeController, gameObject);

            return true;
        }

        protected override MenuActionDisplayState GetDisplayState(WindowState state, TrackAsset[] tracks)
        {
            if (tracks.Length == 0)
                return MenuActionDisplayState.Hidden;

            if (tracks[0] is AnimationTrack)
            {
                var animTrack = tracks[0] as AnimationTrack;
                if (animTrack.CanConvertToClipMode())
                    return MenuActionDisplayState.Visible;
            }
            else if (tracks[0].hasCurves)
            {
                return MenuActionDisplayState.Visible;
            }

            return MenuActionDisplayState.Hidden;
        }

        public override bool Execute(WindowState state, TrackAsset[] tracks)
        {
            return Do(state, tracks[0]);
        }

        static TimelineWindowTimeControl.ClipData CreateTimeControlClipData(TrackAsset track)
        {
            var data = new TimelineWindowTimeControl.ClipData();
            data.track = track;
            data.start = track.start;
            data.duration = track.duration;
            return data;
        }
    }

    [DisplayName("Lock")]
    [Shortcut(Shortcuts.Timeline.toggleLock)]
    class LockTrack : TrackAction
    {
        protected override bool IsChecked(WindowState state, TrackAsset[] tracks)
        {
            return tracks.All(x => x.locked);
        }

        protected override MenuActionDisplayState GetDisplayState(WindowState state, TrackAsset[] tracks)
        {
            bool hasUnlockableTracks = tracks.Any(x => x.parentLocked);
            if (hasUnlockableTracks)
                return MenuActionDisplayState.Disabled;
            return MenuActionDisplayState.Visible;
        }

        public override bool Execute(WindowState state, TrackAsset[] tracks)
        {
            if (!tracks.Any()) return false;

            var hasUnlockedTracks = tracks.Any(x => !x.locked);
            SetLockState(tracks, hasUnlockedTracks, state);
            return true;
        }

        public static void SetLockState(TrackAsset[] tracks, bool shouldLock, WindowState state = null)
        {
            if (tracks.Length == 0)
                return;

            foreach (var track in tracks)
            {
                if (track.parentLocked)
                    continue;

                TimelineUndo.PushUndo(track, "Lock Tracks");
                track.locked = shouldLock;
            }

            if (state != null)
            {
                // find the tracks we've locked. unselect anything locked and remove recording.
                foreach (var track in tracks)
                {
                    if (track.parentLocked || !track.locked)
                        continue;

                    var flattenedChildTracks = track.GetFlattenedChildTracks();
                    foreach (var i in track.clips)
                        SelectionManager.Remove(i);
                    state.UnarmForRecord(track);
                    foreach (var child in flattenedChildTracks)
                    {
                        SelectionManager.Remove(child);
                        state.UnarmForRecord(child);
                        foreach (var clip in child.GetClips())
                            SelectionManager.Remove(clip);
                    }
                }

                // no need to rebuild, just repaint (including inspectors)
                InspectorWindow.RepaintAllInspectors();
                state.editorWindow.Repaint();
            }
        }
    }

    [UsedImplicitly]
    [DisplayName("Show Markers")]
    [ActiveInMode(TimelineModes.Default | TimelineModes.ReadOnly)]
    class ShowHideMarkers : TrackAction
    {
        protected override bool IsChecked(WindowState state, TrackAsset[] tracks)
        {
            return tracks.All(x => x.GetShowMarkers());
        }

        protected override MenuActionDisplayState GetDisplayState(WindowState state, TrackAsset[] tracks)
        {
            if (tracks.Any(x => x is GroupTrack) || tracks.Any(t => t.GetMarkerCount() == 0))
                return MenuActionDisplayState.Hidden;

            if (tracks.Any(t => t.lockedInHierarchy))
                return MenuActionDisplayState.Disabled;

            return MenuActionDisplayState.Visible;
        }

        public override bool Execute(WindowState state, TrackAsset[] tracks)
        {
            if (!tracks.Any()) return false;

            var hasUnlockedTracks = tracks.Any(x => !x.GetShowMarkers());
            ShowHide(state, tracks, hasUnlockedTracks);
            return true;
        }

        static void ShowHide(WindowState state, TrackAsset[] tracks, bool shouldLock)
        {
            if (tracks.Length == 0)
                return;

            var window = state.GetWindow();
            foreach (var track in tracks)
            {
                window.SetShowTrackMarkers(track, shouldLock);
            }
        }
    }

    [DisplayName("Mute selected track only")]
    class MuteSelectedTrack : TrackAction
    {
        protected override MenuActionDisplayState GetDisplayState(WindowState state, TrackAsset[] tracks)
        {
            if (tracks.Any(track => TimelineUtility.IsParentMuted(track) || track is GroupTrack ||
                !track.subTracksObjects.Any()))
                return MenuActionDisplayState.Hidden;
            return MenuActionDisplayState.Visible;
        }

        public override bool Execute(WindowState state, TrackAsset[] tracks)
        {
            if (!tracks.Any()) return false;

            var hasUnmutedTracks = tracks.Any(x => !x.muted);
            Mute(state, tracks.Where(p => !(p is GroupTrack)).ToArray(), hasUnmutedTracks);
            return true;
        }

        protected override string GetDisplayName(WindowState state, TrackAsset[] tracks)
        {
            return tracks.All(t => t.muted) ? "Unmute selected track only" : base.GetDisplayName(state, tracks);
        }

        public static void Mute(WindowState state, TrackAsset[] tracks, bool shouldMute)
        {
            if (tracks.Length == 0)
                return;

            foreach (var track in tracks.Where(t => !TimelineUtility.IsParentMuted(t)))
            {
                TimelineUndo.PushUndo(track, "Mute Tracks");
                track.muted = shouldMute;
            }
            state.Refresh();
        }
    }

    [DisplayName("Mute")]
    [Shortcut(Shortcuts.Timeline.toggleMute)]
    class MuteTrack : TrackAction
    {
        protected override MenuActionDisplayState GetDisplayState(WindowState state, TrackAsset[] tracks)
        {
            if (tracks.Any(track => TimelineUtility.IsParentMuted(track)))
                return MenuActionDisplayState.Disabled;
            return MenuActionDisplayState.Visible;
        }

        protected override string GetDisplayName(WindowState state, TrackAsset[] tracks)
        {
            return tracks.Any(x => !x.muted) ? base.GetDisplayName(state, tracks) : "Unmute" + GetShortcutText(GetShortcutAttributeForAction(this));
        }

        public override bool Execute(WindowState state, TrackAsset[] tracks)
        {
            if (!tracks.Any() || tracks.Any(track => TimelineUtility.IsParentMuted(track)))
                return false;

            var hasUnmutedTracks = tracks.Any(x => !x.muted);
            Mute(state, tracks, hasUnmutedTracks);
            return true;
        }

        public static void Mute(WindowState state, TrackAsset[] tracks, bool shouldMute)
        {
            if (tracks.Length == 0)
                return;

            foreach (var track in tracks)
            {
                if (track as GroupTrack == null)
                    Mute(state, track.GetChildTracks().ToArray(), shouldMute);
                TimelineUndo.PushUndo(track, "Mute Tracks");
                track.muted = shouldMute;
            }

            state.Refresh();
        }
    }

    [DisplayName("Add Track Sub-Group")]
    class AddTrackSubGroup : TrackAction
    {
        protected override MenuActionDisplayState GetDisplayState(WindowState state, TrackAsset[] tracks)
        {
            var areOnlyGroups = tracks.Where(x => x is GroupTrack).ToArray().Length == tracks.Length;
            if (!areOnlyGroups)
                return MenuActionDisplayState.Hidden;

            if (tracks.Any(x => x.lockedInHierarchy))
                return MenuActionDisplayState.Disabled;

            return MenuActionDisplayState.Visible;
        }

        public override bool Execute(WindowState state, TrackAsset[] tracks)
        {
            foreach (var track in tracks)
            {
                TimelineHelpers.CreateTrack<GroupTrack>(track, "Track Sub-Group");
            }

            state.Refresh();

            return true;
        }
    }

    [HideInMenu]
    class DeleteTracks : TrackAction
    {
        public static void Do(TimelineAsset timeline, TrackAsset track)
        {
            SelectionManager.Remove(track);
            TrackModifier.DeleteTrack(timeline, track);
        }

        public override bool Execute(WindowState state, TrackAsset[] tracks)
        {
            // disable preview mode so deleted tracks revert to default state
            // Case 956129: Disable preview mode _before_ deleting the tracks, since clip data is still needed
            state.previewMode = false;

            TimelineAnimationUtilities.UnlinkAnimationWindowFromTracks(tracks);

            foreach (var track in tracks)
                Do(state.editSequence.asset, track);

            state.Refresh();

            return true;
        }
    }

    [HideInMenu]
    class CopyTracksToClipboard : TrackAction
    {
        public static bool Do(WindowState state, TrackAsset[] tracks)
        {
            var action = new CopyTracksToClipboard();

            return action.Execute(state, tracks);
        }

        public override bool Execute(WindowState state, TrackAsset[] tracks)
        {
            TimelineEditor.clipboard.CopyTracks(tracks);

            return true;
        }
    }

    [HideInMenu]
    class DuplicateTracks : TrackAction
    {
        public override bool Execute(WindowState state, TrackAsset[] tracks)
        {
            if (tracks.Any())
            {
                SelectionManager.RemoveTimelineSelection();
            }

            foreach (var track in TrackExtensions.FilterTracks(tracks))
            {
                var newTrack = track.Duplicate(state.editSequence.director);
                SelectionManager.Add(newTrack);
                foreach (var childTrack in newTrack.GetFlattenedChildTracks())
                {
                    SelectionManager.Add(childTrack);
                }
            }

            state.Refresh();

            return true;
        }
    }
}
