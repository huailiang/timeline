using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace UnityEditor.Timeline
{
    static class ContextMenus
    {
        public static ContextMenu markerHeaderMenu = new ContextMenu(
            ContextMenu.MenuItemForAction(new CopyAction()),
            ContextMenu.MenuItemForAction(new PasteAction()),
            ContextMenu.MenuItemForAction(new ToggleMuteMarkersOnTimeline()),
            ContextMenu.MenuItemForAction(new ToggleShowMarkersOnTimeline())
        );
    }

    class ContextMenu
    {
        List<MenuItemBase> m_Items;

        public ContextMenu(params MenuItemBase[] items)
        {
            m_Items = items.ToList();
        }

        public void ShowMenu(WindowState state)
        {
            var menu = new GenericMenu();
            AddToMenu(menu, state);
            menu.ShowAsContext();
        }

        public void AddToMenu(GenericMenu menu, WindowState state)
        {
            foreach (var entry in m_Items)
            {
                entry.AddToMenu(menu, state);
            }
        }

        public static MenuItemBase MenuItemForAction(TimelineAction action)
        {
            return new TimelineActionMenuItem(action);
        }
    }

    static class SequencerContextMenu
    {
        static class Styles
        {
            public static readonly string addItemFromAssetTemplate       = L10n.Tr("Add {0} From {1}");
            public static readonly string addSingleItemFromAssetTemplate = L10n.Tr("Add From {1}");
            public static readonly string addItemTemplate                = L10n.Tr("Add {0}");
            public static readonly string removeInvalidMarkersString     = L10n.Tr("Remove Invalid Markers");
            public static readonly string typeSelectorTemplate           = L10n.Tr("Select {0}");
        }

        public static void ShowNewTracksContextMenu(TrackAsset parentTrack, TimelineGroupGUI parentGroup, WindowState state)
        {
            var menu = new GenericMenu();
            AddTrackMenuCommands(menu, parentTrack, parentGroup, state);
            menu.ShowAsContext();
        }

        public static void ShowTrackContextMenu(TrackDrawer drawer, TrackAsset track, Vector2 mousePosition)
        {
            var menu = new GenericMenu();

            TimelineAction.AddToMenu(menu, TimelineWindow.instance.state, mousePosition);
            menu.AddSeparator("");

            TrackAction.AddToMenu(menu, TimelineWindow.instance.state);

            var groupTrack = track as GroupTrack;
            if (groupTrack == null)
            {
                if (drawer != null)
                {
                    drawer.OnBuildTrackContextMenu(menu, track, TimelineWindow.instance.state);
                }
            }
            else
            {
                // Add all the track types..
                menu.AddSeparator("");
                TimelineGroupGUI.AddMenuItems(menu, groupTrack);
            }

            menu.ShowAsContext();
        }

        public static void ShowItemContextMenu(Vector2 mousePosition)
        {
            var menu = new GenericMenu();
            var state = TimelineWindow.instance.state;

            TimelineAction.AddToMenu(menu, state, mousePosition);

            if (SelectionManager.SelectedClips().Any())
                ItemAction<TimelineClip>.AddToMenu(menu, state);
            if (SelectionManager.SelectedMarkers().Any())
                ItemAction<IMarker>.AddToMenu(menu, state);

            var clipGUI = PickerUtils.PickedLayerableOfType<TimelineClipGUI>();
            if (clipGUI != null && clipGUI.drawer != null)
            {
                var clips = SelectionManager.SelectedClips().ToArray();
                if (clips.Length > 0)
                {
                    clipGUI.drawer.OnBuildClipContextMenu(menu, clips, state);
                    AddMarkerMenuCommands(menu, clipGUI.clip.parentTrack, TimelineHelpers.GetCandidateTime(state, mousePosition));
                }
            }

            menu.ShowAsContext();
        }

        public static void AddClipMenuCommands(GenericMenu menu, TrackAsset trackAsset, double candidateTime)
        {
            var assetTypes = TypeUtility.GetPlayableAssetsHandledByTrack(trackAsset.GetType());
            var visibleAssetTypes = TypeUtility.GetVisiblePlayableAssetsHandledByTrack(trackAsset.GetType());

            if (assetTypes.Any() || visibleAssetTypes.Any())
                menu.AddSeparator(string.Empty);

            // skips the name if there is only a single type
            var commandNameTemplate = assetTypes.Count() == 1 ? Styles.addSingleItemFromAssetTemplate : Styles.addItemFromAssetTemplate;
            foreach (var assetType in assetTypes)
            {
                Action<Object> onObjectChanged = obj =>
                {
                    if (obj != null)
                        TimelineHelpers.CreateClipOnTrack(assetType, obj, trackAsset, candidateTime);
                };
                AddItemFromAssetCommands(menu, commandNameTemplate, assetType, onObjectChanged, !trackAsset.lockedInHierarchy && !TimelineWindow.instance.state.editSequence.isReadOnly);
            }

            foreach (var assetType in visibleAssetTypes)
            {
                var commandName = string.Format(Styles.addItemTemplate, TypeUtility.GetDisplayName(assetType));
                GenericMenu.MenuFunction command = () =>
                {
                    TimelineHelpers.CreateClipOnTrack(assetType, trackAsset, candidateTime);
                };
                AddCommandToMenu(menu, commandName, command, !trackAsset.lockedInHierarchy && !TimelineWindow.instance.state.editSequence.isReadOnly);
            }
        }

        static void AddMarkerMenuCommands(GenericMenu menu, IEnumerable<Type> markerTypes, Func<Type, IMarker> addMarkerCommand, bool enabled)
        {
            foreach (var markerType in markerTypes)
            {
                var commandName = string.Format(Styles.addItemTemplate, TypeUtility.GetDisplayName(markerType));
                AddCommandToMenu(menu, commandName, () => addMarkerCommand(markerType), enabled);

                if (!typeof(ScriptableObject).IsAssignableFrom(markerType))
                    continue;

                Action<Object> onObjectChanged = obj =>
                {
                    if (obj == null) return;
                    var marker = addMarkerCommand(markerType);

                    foreach (var field in ObjectReferenceField.FindObjectReferences(markerType))
                    {
                        if (field.IsAssignable(obj))
                        {
                            field.Assign(marker as ScriptableObject, obj, TimelineWindow.instance.state.editSequence.director);
                            break;
                        }
                    }
                };

                AddItemFromAssetCommands(menu, Styles.addItemFromAssetTemplate, markerType, onObjectChanged, enabled);
            }
        }

        public static void AddMarkerMenuCommands(GenericMenu menu, TrackAsset track, double candidateTime)
        {
            var enabled = !track.lockedInHierarchy && !TimelineWindow.instance.state.editSequence.isReadOnly;
            var addMarkerCommand = new Func<Type, IMarker>(type => AddMarkerCommand(track, type, candidateTime));

            AddMarkerMenuCommands(menu, track, addMarkerCommand, enabled);
        }

        public static void AddMarkerMenuCommands(GenericMenu menu, TrackAsset track, Func<Type, IMarker> command, bool enabled)
        {
            var markerTypes = TypeUtility.GetBuiltInMarkerTypes().Union(TypeUtility.GetUserMarkerTypes());
            if (track != null)
            {
                if (track is MarkerTrack)
                    markerTypes = markerTypes.Where(x => TypeUtility.DoesTrackSupportMarkerType(track, x));
                else
                    markerTypes = markerTypes.Where(x => TypeUtility.DoesTrackSupportTrackType(track, x));
            }

            menu.AddSeparator(string.Empty);
            AddMarkerMenuCommands(menu, markerTypes, command, enabled);
            RemoveInvalidMarkersMenuItem(menu, track);
        }

        public static IMarker AddMarkerCommand(TrackAsset target, Type markerType, double time)
        {
            SelectionManager.Clear();
            var marker = MarkerModifier.CreateMarkerAtTime(target, markerType, time);
            TimelineWindow.instance.state.Refresh();
            return marker;
        }

        public static void RemoveInvalidMarkersMenuItem(GenericMenu menu, TrackAsset target)
        {
            var hasInvalid = target != null && target.GetMarkerCount() != target.GetMarkersRaw().Count();
            if (!hasInvalid) return;

            menu.AddSeparator(string.Empty);
            AddCommandToMenu(menu, Styles.removeInvalidMarkersString, () => RemoveInvalidMarkersCommand(target));
        }

        static void AddTrackMenuCommands(GenericMenu newTrackMenu, TrackAsset parentTrack, TimelineGroupGUI parentGroup, WindowState state)
        {
            // Add Group or SubGroup
            var title = parentTrack == null ? L10n.Tr("Track Group") : L10n.Tr("Track Sub-Group");
            var disabled = (parentTrack != null && parentTrack.lockedInHierarchy) || state.editSequence.isReadOnly;
            GenericMenu.MenuFunction command = () =>
            {
                SelectionManager.Clear();
                TimelineHelpers.CreateTrack<GroupTrack>(parentTrack, title);
            };

            AddCommandToMenu(newTrackMenu, title, command, !disabled);
            newTrackMenu.AddSeparator("");

            var allTypes = TypeUtility.AllTrackTypes().Where(x => x != typeof(GroupTrack) && !TypeUtility.IsHiddenInMenu(x)).ToList();
            var builtIn = allTypes.Where(x => x.Assembly.Equals(typeof(TimelineAsset).Assembly)).OrderBy(i => i.FullName).ToList();
            var customTypes = allTypes.Except(builtIn).ToList();

            foreach (var t in builtIn)
                AddNewTrackMenuCommand(newTrackMenu, parentTrack, parentGroup, t, state);

            if (builtIn.Any() && customTypes.Any())
                newTrackMenu.AddSeparator("");

            foreach (var t in customTypes)
                AddNewTrackMenuCommand(newTrackMenu, parentTrack, parentGroup, t, state);
        }

        static void AddNewTrackMenuCommand(GenericMenu menu, TrackAsset parentTrack, TimelineGroupGUI parentGroup, Type type, WindowState state)
        {
            GenericMenu.MenuFunction2 lastMethod = trackType =>
            {
                SelectionManager.Clear();
                TimelineHelpers.CreateTrack((Type)trackType, parentTrack);
            };

            var category = TimelineHelpers.GetTrackCategoryName(type);
            if (!string.IsNullOrEmpty(category))
                category += "/";

            var name = category + TimelineHelpers.GetTrackMenuName(type);
            var disabled = (parentTrack != null && parentTrack.lockedInHierarchy) || state.editSequence.isReadOnly;
            AddCommandToMenu(menu, name, lastMethod, type, !disabled);
        }

        static void AddCommandToMenu(GenericMenu menu, string name, GenericMenu.MenuFunction func = null, bool enabled = true, bool on = false)
        {
            var content = EditorGUIUtility.TextContent(name);
            if (!enabled)
                menu.AddDisabledItem(content, on);
            else
                menu.AddItem(content, on, func);
        }

        static void AddCommandToMenu(GenericMenu menu, string name, GenericMenu.MenuFunction2 func = null, object userData = null, bool enabled = true, bool on = false)
        {
            var content = EditorGUIUtility.TextContent(name);
            if (!enabled)
                menu.AddDisabledItem(content, on);
            else
                menu.AddItem(content, on, func, userData);
        }

        internal static void RemoveInvalidMarkersCommand(TrackAsset target)
        {
            var invalids = target.GetMarkersRaw().Where(x => !(x is IMarker)).ToList();
            foreach (var m in invalids)
            {
                target.DeleteMarkerRaw(m);
            }
        }

        static void AddItemFromAssetCommands(GenericMenu menu, string commandNameTemplate, Type itemType, Action<Object> onObjectChanged, bool enabled)
        {
            foreach (var objectReference in TypeUtility.ObjectReferencesForType(itemType))
            {
                var isSceneReference = objectReference.isSceneReference;
                GenericMenu.MenuFunction2 menuCallback = userData =>
                {
                    var dataType = (Type)userData;
                    ObjectSelector.get.Show(null, dataType, null, isSceneReference, null, onObjectChanged, null);
                    ObjectSelector.get.titleContent = EditorGUIUtility.TrTextContent(string.Format(Styles.typeSelectorTemplate, TypeUtility.GetDisplayName(dataType)));
                };

                var menuItemContent = string.Format(commandNameTemplate, TypeUtility.GetDisplayName(itemType), TypeUtility.GetDisplayName(objectReference.type));
                AddCommandToMenu(menu, menuItemContent, menuCallback, objectReference.type, enabled);
            }
        }
    }
}
