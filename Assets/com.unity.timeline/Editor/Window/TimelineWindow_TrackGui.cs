using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace UnityEditor.Timeline
{
    partial class TimelineWindow
    {
        public TimelineTreeViewGUI treeView { get; private set; }

        void TracksGUI(Rect clientRect, WindowState state, TimelineModeGUIState trackState)
        {
            if (Event.current.type == EventType.Repaint && treeView != null)
            {
                state.spacePartitioner.Clear();
            }

            if (state.IsEditingASubTimeline() && !state.IsEditingAnEmptyTimeline())
            {
                var headerRect = clientRect;
                headerRect.width = state.sequencerHeaderWidth;
                Graphics.DrawBackgroundRect(state, headerRect);

                var clipRect = clientRect;
                clipRect.xMin = headerRect.xMax;
                Graphics.DrawBackgroundRect(state, clipRect, subSequenceMode: true);
            }
            else
            {
                Graphics.DrawBackgroundRect(state, clientRect);
            }

            if (!state.IsEditingAnEmptyTimeline())
                m_TimeArea.DrawMajorTicks(sequenceContentRect, state.referenceSequence.frameRate);

            GUILayout.BeginVertical();
            {
                GUILayout.Space(5.0f);
                GUILayout.BeginHorizontal();

                if (this.state.editSequence.asset == null)
                    DrawNoSequenceGUI(state);
                else
                    DrawTracksGUI(clientRect, trackState);

                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            Graphics.DrawShadow(clientRect);
        }

        void DrawNoSequenceGUI(WindowState windowState)
        {
            bool showCreateButton = false;
            var currentlySelectedGo = UnityEditor.Selection.activeObject != null ? UnityEditor.Selection.activeObject as GameObject : null;
            var textContent = DirectorStyles.noTimelineAssetSelected;
            var existingDirector = currentlySelectedGo != null ? currentlySelectedGo.GetComponent<PlayableDirector>() : null;
            var existingAsset = existingDirector != null ? existingDirector.playableAsset : null;

            if (currentlySelectedGo != null && !TimelineUtility.IsPrefabOrAsset(currentlySelectedGo) && existingAsset == null)
            {
                showCreateButton = true;
                textContent = new GUIContent(String.Format(DirectorStyles.createTimelineOnSelection.text, currentlySelectedGo.name, "a Director component and a Timeline asset"));
            }
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            GUILayout.Label(textContent);

            if (showCreateButton)
            {
                GUILayout.BeginHorizontal();
                var textSize = GUI.skin.label.CalcSize(textContent);
                GUILayout.Space((textSize.x / 2.0f) - (WindowConstants.createButtonWidth / 2.0f));
                if (GUILayout.Button("Create", GUILayout.Width(WindowConstants.createButtonWidth)))
                {
                    var message = DirectorStyles.createNewTimelineText.text + " '" + currentlySelectedGo.name + "'";
                    string newSequencePath = EditorUtility.SaveFilePanelInProject(DirectorStyles.createNewTimelineText.text, currentlySelectedGo.name + "Timeline", "playable", message, ProjectWindowUtil.GetActiveFolderPath());
                    if (!string.IsNullOrEmpty(newSequencePath))
                    {
                        var newAsset = CreateInstance<TimelineAsset>();
                        AssetDatabase.CreateAsset(newAsset, newSequencePath);

                        Undo.IncrementCurrentGroup();

                        if (existingDirector == null)
                        {
                            existingDirector = Undo.AddComponent<PlayableDirector>(currentlySelectedGo);
                        }

                        existingDirector.playableAsset = newAsset;
                        SetCurrentTimeline(existingDirector);
                        var newTrack = TimelineHelpers.CreateTrack<AnimationTrack>();

                        windowState.previewMode = false;
                        TimelineUtility.SetSceneGameObject(windowState.editSequence.director, newTrack, currentlySelectedGo);
                    }

                    // If we reach this point, the state of the pannel has changed; skip the rest of this GUI phase
                    // Fixes: case 955831 - [OSX] NullReferenceException when creating a timeline on a selected object
                    GUIUtility.ExitGUI();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
        }

        void DrawTracksGUI(Rect clientRect, TimelineModeGUIState trackState)
        {
            GUILayout.BeginVertical(GUILayout.Height(clientRect.height));
            if (treeView != null)
                treeView.OnGUI(clientRect);
            GUILayout.EndVertical();
        }

        void RefreshInlineCurves()
        {
            foreach (var trackGUI in allTracks.OfType<TimelineTrackGUI>())
            {
                if (trackGUI.inlineCurveEditor != null)
                    trackGUI.inlineCurveEditor.Refresh();
            }
        }
    }
}
