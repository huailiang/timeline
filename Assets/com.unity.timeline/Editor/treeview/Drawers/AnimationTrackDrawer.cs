using System;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
    [CustomTrackDrawer(typeof(AnimationTrack)), UsedImplicitly]
    class AnimationTrackDrawer : TrackDrawer
    {
        internal static class Styles
        {
            public static readonly GUIContent AnimationButtonOnTooltip = EditorGUIUtility.TrTextContent("", "Avatar Mask enabled\nClick to disable");
            public static readonly GUIContent AnimationButtonOffTooltip = EditorGUIUtility.TrTextContent("", "Avatar Mask disabled\nClick to enable");
            public static readonly GUIContent ConvertToInfiniteClipMenuItem = EditorGUIUtility.TrTextContent("Convert to Infinite Clip");
            public static readonly GUIContent ConvertToClipTrackMenuItem = EditorGUIUtility.TrTextContent("Convert To Clip Track");
            public static readonly GUIContent AddOverrideTrackMenuItem = EditorGUIUtility.TrTextContent("Add Override Track");
            public static readonly GUIContent AddTransformTrackMenuItem = EditorGUIUtility.TrTextContent("Add Sub Transform Track");
            public static readonly string TrackOffsetMenuPrefix = L10n.Tr("Track Offsets/");
        }

        public override void OnBuildTrackContextMenu(GenericMenu menu, TrackAsset track, WindowState state)
        {
            var animTrack = track as AnimationTrack;
            if (animTrack == null)
            {
                base.OnBuildTrackContextMenu(menu, track, state);
                return;
            }

            if (animTrack.CanConvertFromClipMode() || animTrack.CanConvertToClipMode())
            {
                var canConvertToInfinite = animTrack.CanConvertFromClipMode();
                var canConvertToClip = animTrack.CanConvertToClipMode();

                if (canConvertToInfinite)
                {
                    if (track.lockedInHierarchy || TimelineWindow.instance.state.editSequence.isReadOnly)
                    {
                        menu.AddDisabledItem(Styles.ConvertToInfiniteClipMenuItem, false);
                    }
                    else
                    {
                        menu.AddItem(Styles.ConvertToInfiniteClipMenuItem, false, parentTrack =>
                        {
                            animTrack.ConvertFromClipMode(state.editSequence.asset);
                            state.Refresh();
                        }, track);
                    }
                }

                if (canConvertToClip)
                {
                    if (track.lockedInHierarchy || TimelineWindow.instance.state.editSequence.isReadOnly)
                    {
                        menu.AddDisabledItem(Styles.ConvertToClipTrackMenuItem, false);
                    }
                    else
                    {
                        menu.AddItem(Styles.ConvertToClipTrackMenuItem, false, parentTrack =>
                        {
                            animTrack.ConvertToClipMode();
                            state.Refresh();
                        }, track);
                    }
                }
            }

            if (!track.isSubTrack)
            {
                var items = Enum.GetValues(typeof(TrackOffset));
                foreach (var i in items)
                {
                    var item = (TrackOffset)i;
                    if (state.editSequence.isReadOnly)
                    {
                        menu.AddDisabledItem(new GUIContent(Styles.TrackOffsetMenuPrefix + TypeUtility.GetMenuItemName(item)));
                    }
                    else
                    {
                        menu.AddItem(
                            new GUIContent(Styles.TrackOffsetMenuPrefix + TypeUtility.GetMenuItemName(item)),
                            animTrack.trackOffset == item,
                            () =>
                            {
                                animTrack.trackOffset = item;
                                state.UnarmForRecord(animTrack);
                                state.rebuildGraph = true;
                            }
                        );
                    }
                }
            }

            base.OnBuildTrackContextMenu(menu, track, state);

            if (!track.isSubTrack)
            {
                menu.AddSeparator(string.Empty);
                if (track.lockedInHierarchy || TimelineWindow.instance.state.editSequence.isReadOnly)
                {
                    menu.AddDisabledItem(Styles.AddOverrideTrackMenuItem, false);
                }
                else
                {
                    menu.AddItem(Styles.AddOverrideTrackMenuItem, false, parentTrack =>
                    {
                        AddSubTrack(state, typeof(AnimationTrack), "Override " + track.GetChildTracks().Count().ToString(), track);
                    }, track);
                    menu.AddItem(Styles.AddTransformTrackMenuItem, false, parentTrack =>
                    {
                        AddSubTrack(state, typeof(TransformTrack), "TransformTrack " + track.GetChildTracks().Count().ToString(), track);
                    }, track);
                }
            }
        }

        static void AddSubTrack(WindowState state, Type trackOfType, string trackName, TrackAsset track)
        {
            TimelineHelpers.CreateTrack(trackOfType, track, trackName);
        }

        public override void OnBuildClipContextMenu(GenericMenu menu, TimelineClip[] clips, WindowState state)
        {
            AnimationOffsetMenu.OnClipMenu(state, clips, menu);
        }

        public override bool DrawTrackHeaderButton(Rect rect, TrackAsset track, WindowState state)
        {
            var animTrack = track as AnimationTrack;
            bool hasAvatarMask = animTrack != null && animTrack.avatarMask != null;
            if (hasAvatarMask)
            {
                var style = animTrack.applyAvatarMask
                    ? DirectorStyles.Instance.avatarMaskOn
                    : DirectorStyles.Instance.avatarMaskOff;
                var tooltip = animTrack.applyAvatarMask
                    ? Styles.AnimationButtonOnTooltip
                    : Styles.AnimationButtonOffTooltip;
                if (GUI.Button(rect, tooltip, style))
                {
                    animTrack.applyAvatarMask = !animTrack.applyAvatarMask;
                    if (state != null)
                        state.rebuildGraph = true;
                }
            }
            return hasAvatarMask;
        }

        public override void DrawRecordingBackground(Rect trackRect, TrackAsset trackAsset, Vector2 visibleTime, WindowState state)
        {
            base.DrawRecordingBackground(trackRect, trackAsset, visibleTime, state);
            DrawBorderOfAddedRecordingClip(trackRect, trackAsset, visibleTime, (WindowState)state);
        }

        static void DrawBorderOfAddedRecordingClip(Rect trackRect, TrackAsset trackAsset, Vector2 visibleTime, WindowState state)
        {
            if (!state.IsArmedForRecord(trackAsset))
                return;

            AnimationTrack animTrack = trackAsset as AnimationTrack;
            if (animTrack == null || !animTrack.inClipMode)
                return;

            // make sure there is no clip but we can add one
            TimelineClip clip = null;
            if (trackAsset.FindRecordingClipAtTime(state.editSequence.time, out clip) || clip != null)
                return;

            float yMax = trackRect.yMax;
            float yMin = trackRect.yMin;

            double startGap = 0;
            double endGap = 0;

            trackAsset.GetGapAtTime(state.editSequence.time, out startGap, out endGap);
            if (double.IsInfinity(endGap))
                endGap = visibleTime.y;

            if (startGap > visibleTime.y || endGap < visibleTime.x)
                return;


            startGap = Math.Max(startGap, visibleTime.x);
            endGap = Math.Min(endGap, visibleTime.y);

            float xMin = state.TimeToPixel(startGap);
            float xMax = state.TimeToPixel(endGap);

            Rect r = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
            ClipDrawer.DrawBorder(r, ClipBorder.kRecording, ClipBlends.kNone);
        }
    }
}
