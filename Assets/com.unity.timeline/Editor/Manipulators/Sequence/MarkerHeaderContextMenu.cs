using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
    class TimelineMarkerHeaderContextMenu : Manipulator
    {
        protected override bool ContextClick(Event evt, WindowState state)
        {
            if (!state.showMarkerHeader)
                return false;

            if (!(state.GetWindow().markerHeaderRect.Contains(evt.mousePosition)
                  || state.GetWindow().markerContentRect.Contains(evt.mousePosition)))
                return false;

            ShowMenu(evt.mousePosition, state);
            return true;
        }

        public static void ShowMenu(Vector2? mousePosition, WindowState state)
        {
            var menu = new GenericMenu();
            ContextMenus.markerHeaderMenu.AddToMenu(menu, state);
            var timeline = state.editSequence.asset;
            var time = TimelineHelpers.GetCandidateTime(state, mousePosition);
            var enabled = timeline.markerTrack == null || !timeline.markerTrack.lockedInHierarchy && !state.editSequence.isReadOnly;
            var addMarkerCommand = new Func<Type, IMarker>(type => AddMarkerCommand(type, time, state));

            SequencerContextMenu.AddMarkerMenuCommands(menu, timeline.markerTrack, addMarkerCommand, enabled);
            menu.ShowAsContext();
        }

        static IMarker AddMarkerCommand(Type markerType, double time, WindowState state)
        {
            var timeline = state.editSequence.asset;
            timeline.CreateMarkerTrack();
            var markerTrack = timeline.markerTrack;

            var marker = SequencerContextMenu.AddMarkerCommand(markerTrack, markerType, time);

            if (typeof(INotification).IsAssignableFrom(markerType))
            {
                // If we have no binding for the Notifications, set it to the director GO
                var director = state.editSequence.director;
                if (director != null && director.GetGenericBinding(markerTrack) == null)
                    director.SetGenericBinding(markerTrack, director.gameObject);
            }

            return marker;
        }
    }
}
