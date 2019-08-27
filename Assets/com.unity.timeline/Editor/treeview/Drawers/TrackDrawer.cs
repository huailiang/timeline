using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
namespace UnityEditor.Timeline
{
    class TrackDrawer : GUIDrawer
    {
        public class TrackMenuContext
        {
            public enum ClipTimeCreation
            {
                TimeCursor,
                Mouse
            }

            public ClipTimeCreation clipTimeCreation = ClipTimeCreation.TimeCursor;
            public Vector2? mousePosition = null;
        }

        public TrackMenuContext trackMenuContext = new TrackMenuContext();

        internal WindowState sequencerState { get; set; }


        public static TrackDrawer CreateInstance(TrackAsset trackAsset)
        {
            if (trackAsset == null)
                return Activator.CreateInstance<TrackDrawer>();

            TrackDrawer drawer;

            try
            {
                drawer = (TrackDrawer)Activator.CreateInstance(TimelineHelpers.GetCustomDrawer(trackAsset.GetType()));
            }
            catch (Exception)
            {
                drawer = Activator.CreateInstance<TrackDrawer>();
            }

            drawer.track = trackAsset;
            return drawer;
        }

        protected TrackAsset track { get; private set; }

        public virtual bool DrawTrackHeaderButton(Rect rect, TrackAsset track, WindowState state)
        {
            return false;
        }

        public virtual void OnBuildTrackContextMenu(GenericMenu menu, TrackAsset trackAsset, WindowState state)
        {
            var mousePosition = trackMenuContext.mousePosition;
            var candidateTime = TimelineHelpers.GetCandidateTime(state, mousePosition, trackAsset);

            SequencerContextMenu.AddClipMenuCommands(menu, trackAsset, candidateTime);
            SequencerContextMenu.AddMarkerMenuCommands(menu, trackAsset, candidateTime);
        }

        // Override this method for context menus on the clips on the same track
        public virtual void OnBuildClipContextMenu(GenericMenu menu, TimelineClip[] clips, WindowState state) {}

        public virtual bool DrawTrack(Rect trackRect, TrackAsset trackAsset, Vector2 visibleTime, WindowState state)
        {
            return false;
        }

        public virtual void DrawRecordingBackground(Rect trackRect, TrackAsset trackAsset, Vector2 visibleTime, WindowState state)
        {
            EditorGUI.DrawRect(trackRect, DirectorStyles.Instance.customSkin.colorTrackBackgroundRecording);
        }
    }
}
