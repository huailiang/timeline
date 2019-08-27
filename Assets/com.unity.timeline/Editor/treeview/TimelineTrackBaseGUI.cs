using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
    abstract class TimelineTrackBaseGUI : TreeViewItem, IBounds
    {
        static class Styles
        {
            public static readonly GUIContent s_LockedAndMuted = EditorGUIUtility.TrTextContent("Locked / Muted");
            public static readonly GUIContent s_LockedAndPartiallyMuted = EditorGUIUtility.TrTextContent("Locked / Partially Muted");
            public static readonly GUIContent s_Locked = EditorGUIUtility.TrTextContent("Locked");
            public static readonly GUIContent s_Muted = EditorGUIUtility.TrTextContent("Muted");
            public static readonly GUIContent s_PartiallyMuted = EditorGUIUtility.TrTextContent("Partially Muted");
        }

        protected bool m_IsRoot = false;
        protected const float k_ButtonSize = 16.0f;

        readonly TimelineTreeViewGUI m_TreeViewGUI;
        readonly TrackDrawer m_Drawer;

        public Vector2 treeViewToWindowTransformation { get; set; }
        public bool isExpanded { get; set; }
        public bool isDropTarget { protected get; set; }
        public TrackAsset track { get; }
        TreeViewController treeView { get; }

        public TimelineWindow TimelineWindow
        {
            get
            {
                if (m_TreeViewGUI == null)
                    return null;

                return m_TreeViewGUI.TimelineWindow;
            }
        }

        public TrackDrawer drawer
        {
            get { return m_Drawer; }
        }

        public virtual float GetVerticalSpacingBetweenTracks()
        {
            return 3.0f;
        }

        public bool visibleRow { get; set; }        // is the header row visible
        public bool visibleExpanded { get; set; }   // is the expanded area (group) visible
        public bool drawInsertionMarkerBefore { get; set; }
        public bool drawInsertionMarkerAfter  { get; set; }

        public abstract Rect boundingRect { get; }
        public abstract bool expandable { get; }
        public abstract void Draw(Rect headerRect, Rect contentRect, WindowState state);
        public abstract void OnGraphRebuilt(); // callback when the corresponding graph is rebuilt. This can happen, but not have the GUI rebuilt.

        protected TimelineTrackBaseGUI(int id, int depth, TreeViewItem parent, string displayName, TrackAsset trackAsset, TreeViewController tv, TimelineTreeViewGUI tvgui)
            : base(id, depth, parent, displayName)
        {
            m_Drawer = TrackDrawer.CreateInstance(trackAsset);
            m_Drawer.sequencerState = tvgui.TimelineWindow.state;

            isExpanded = false;
            isDropTarget = false;
            track = trackAsset;
            treeView = tv;

            m_TreeViewGUI = tvgui;
        }

        public static TimelineTrackBaseGUI FindGUITrack(TrackAsset track)
        {
            var allTracks = TimelineWindow.instance.allTracks;
            return allTracks.Find(x => x.track == track);
        }

        public void DisplayTrackMenu()
        {
            SequencerContextMenu.ShowTrackContextMenu(drawer, track, Event.current.mousePosition);
        }

        public void DrawLockState(TrackAsset track, Rect trackRect)
        {
            if (track.lockedInHierarchy)
            {
                DrawLockTrackBG(trackRect);
                DrawTrackStateBox(trackRect, track);
            }
        }

        static void DrawLockTrackBG(Rect trackRect)
        {
            var styles = DirectorStyles.Instance;
            var texture = styles.lockedBG.normal.background;
            Timeline.Graphics.DrawTextureRepeated(trackRect, texture);
        }

        protected static void DrawTrackStateBox(Rect trackRect, TrackAsset track)
        {
            const float k_LockTextPadding = 40f;
            var styles = DirectorStyles.Instance;

            bool locked = track.locked && !track.parentLocked;
            bool muted = track.muted && !TimelineUtility.IsParentMuted(track);
            bool allSubTrackMuted = TimelineUtility.IsAllSubTrackMuted(track);

            GUIContent content = null;
            if (locked && muted)
            {
                content = Styles.s_LockedAndMuted;
                if (!allSubTrackMuted)
                    content = Styles.s_LockedAndPartiallyMuted;
            }
            else if (locked) content = Styles.s_Locked;
            else if (muted)
            {
                content = Styles.s_Muted;
                if (!allSubTrackMuted)
                    content = Styles.s_PartiallyMuted;
            }

            // the track could be locked, but we only show the 'locked portion' on the upper most track
            //  that is causing the lock
            if (content == null)
                return;

            var textRect = trackRect;
            textRect.width = styles.fontClip.CalcSize(content).x + k_LockTextPadding;
            textRect.x += (trackRect.width - textRect.width) / 2f;
            textRect.height -= 4f;
            textRect.y += 2f;

            using (new GUIColorOverride(styles.customSkin.colorLockTextBG))
            {
                GUI.Box(textRect, GUIContent.none, styles.displayBackground);
            }

            Timeline.Graphics.ShadowLabel(textRect, content, styles.fontClip, Color.white, Color.black);
        }

        public void DrawInsertionMarkers(Rect rowRectWithIndent)
        {
            const float insertionHeight = WindowConstants.trackInsertionMarkerHeight;
            if (Event.current.type == EventType.Repaint && (drawInsertionMarkerAfter || drawInsertionMarkerBefore))
            {
                if (drawInsertionMarkerBefore)
                {
                    var rect = new Rect(rowRectWithIndent.x, rowRectWithIndent.y - insertionHeight * 0.5f - 2.0f, rowRectWithIndent.width, insertionHeight);
                    EditorGUI.DrawRect(rect, Color.white);
                }

                if (drawInsertionMarkerAfter)
                {
                    var rect = new Rect(rowRectWithIndent.x, rowRectWithIndent.y + rowRectWithIndent.height - insertionHeight * 0.5f + 1.0f, rowRectWithIndent.width, insertionHeight);
                    EditorGUI.DrawRect(rect, Color.white);
                }
            }
        }

        public void ClearDrawFlags()
        {
            if (Event.current.type == EventType.Repaint)
            {
                isDropTarget = false;
                drawInsertionMarkerAfter = false;
                drawInsertionMarkerBefore = false;
            }
        }
    }
}
