using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
    class DirectorStyles
    {
        const string k_Elipsis = "…";

        public static readonly GUIContent referenceTrackLabel = EditorGUIUtility.TrTextContent("R", "This track references an external asset");
        public static readonly GUIContent recordingLabel = EditorGUIUtility.TrTextContent("Recording...");
        public static readonly GUIContent sequenceSelectorIcon = EditorGUIUtility.IconContent("TimelineSelector");
        public static readonly GUIContent playContent = EditorGUIUtility.TrIconContent("Animation.Play", "Play the timeline (Space)");
        public static readonly GUIContent gotoBeginingContent = EditorGUIUtility.TrIconContent("Animation.FirstKey", "Go to the beginning of the timeline (Shift+<)");
        public static readonly GUIContent gotoEndContent = EditorGUIUtility.TrIconContent("Animation.LastKey", "Go to the end of the timeline (Shift+>)");
        public static readonly GUIContent nextFrameContent = EditorGUIUtility.TrIconContent("Animation.NextKey", "Go to the next frame");
        public static readonly GUIContent previousFrameContent = EditorGUIUtility.TrIconContent("Animation.PrevKey", "Go to the previous frame");
        public static readonly GUIContent noTimelineAssetSelected = EditorGUIUtility.TrTextContent("To start creating a timeline, select a GameObject");
        public static readonly GUIContent createTimelineOnSelection = EditorGUIUtility.TrTextContent("To begin a new timeline with {0}, create {1}");
        public static readonly GUIContent noTimelinesInScene = EditorGUIUtility.TrTextContent("No timeline found in the scene");
        public static readonly GUIContent createNewTimelineText = EditorGUIUtility.TrTextContent("Create a new Timeline and Director Component for Game Object");
        public static readonly GUIContent newContent = EditorGUIUtility.TrTextContent("Add", "Add new tracks.");
        public static readonly GUIContent previewContent = EditorGUIUtility.TrTextContent("Preview", "Enable/disable scene preview mode");
        public static readonly GUIContent mixOff = EditorGUIUtility.IconContent("TimelineEditModeMixOFF", "|Mix Mode (1)");
        public static readonly GUIContent mixOn = EditorGUIUtility.IconContent("TimelineEditModeMixON", "|Mix Mode (1)");
        public static readonly GUIContent rippleOff = EditorGUIUtility.IconContent("TimelineEditModeRippleOFF", "|Ripple Mode (2)");
        public static readonly GUIContent rippleOn = EditorGUIUtility.IconContent("TimelineEditModeRippleON", "|Ripple Mode (2)");
        public static readonly GUIContent replaceOff = EditorGUIUtility.IconContent("TimelineEditModeReplaceOFF", "|Replace Mode (3)");
        public static readonly GUIContent replaceOn = EditorGUIUtility.IconContent("TimelineEditModeReplaceON", "|Replace Mode (3)");
        public static readonly GUIContent showMarkersOn = EditorGUIUtility.TrTextContentWithIcon(string.Empty, "Show / Hide Timeline Markers", "TimelineMarkerAreaButtonEnabled");
        public static readonly GUIContent showMarkersOff = EditorGUIUtility.TrTextContentWithIcon(string.Empty, "Show / Hide Timeline Markers", "TimelineMarkerAreaButtonDisabled");
        public static readonly GUIContent showMarkersOnTimeline = EditorGUIUtility.TrTextContent("Show markers");
        public static readonly GUIContent timelineMarkerTrackHeader = EditorGUIUtility.TrTextContentWithIcon("Markers", string.Empty, "TimelineHeaderMarkerIcon");
        public static readonly GUIContent markerCollapseButton = EditorGUIUtility.TrTextContent(string.Empty, "Expand / Collapse Track Markers");

        public GUIContent playrangeContent;

        public static readonly float kBaseIndent = 15.0f;
        public static readonly float kDurationGuiThickness = 5.0f;

        // matches dark skin warning color.
        public static readonly Color kClipErrorColor = new Color(0.957f, 0.737f, 0.008f, 1f);

        // TODO: Make skinnable? If we do, we should probably also make the associated cursors skinnable...
        public static readonly Color kMixToolColor = Color.white;
        public static readonly Color kRippleToolColor = new Color(255f / 255f, 210f / 255f, 51f / 255f);
        public static readonly Color kReplaceToolColor = new Color(165f / 255f, 30f / 255f, 30f / 255f);

        public const string markerDefaultStyle = "MarkerItem";

        public GUIStyle handLeft = "MeTransitionHandleLeft";
        public GUIStyle handRight = "MeTransitionHandleRight";
        public GUIStyle groupBackground;
        public GUIStyle displayBackground;
        public GUIStyle fontClip;
        public GUIStyle fontClipLoop;
        public GUIStyle trackHeaderFont;
        public GUIStyle groupFont;
        public GUIStyle timeCursor;
        public GUIStyle endmarker;
        public GUIStyle tinyFont;
        public GUIStyle foldout;
        public GUIStyle mute;
        public GUIStyle locked;
        public GUIStyle autoKey;
        public GUIStyle playTimeRangeStart;
        public GUIStyle playTimeRangeEnd;
        public GUIStyle options;
        public GUIStyle selectedStyle;
        public GUIStyle trackSwatchStyle;
        public GUIStyle connector;
        public GUIStyle keyframe;
        public GUIStyle warning;
        public GUIStyle extrapolationHold;
        public GUIStyle extrapolationLoop;
        public GUIStyle extrapolationPingPong;
        public GUIStyle extrapolationContinue;
        public GUIStyle collapseMarkers;
        public GUIStyle markerMultiOverlay;
        public GUIStyle outlineBorder;
        public GUIStyle timelineClip;
        public GUIStyle timelineClipSelected;
        public GUIStyle bottomShadow;
        public GUIStyle trackOptions;
        public GUIStyle infiniteTrack;
        public GUIStyle blendMixIn;
        public GUIStyle blendMixOut;
        public GUIStyle blendEaseIn;
        public GUIStyle blendEaseOut;
        public GUIStyle clipOut;
        public GUIStyle clipIn;
        public GUIStyle curves;
        public GUIStyle lockedBG;
        public GUIStyle activation;
        public GUIStyle playrange;
        public GUIStyle lockButton;
        public GUIStyle avatarMaskOn;
        public GUIStyle avatarMaskOff;
        public GUIStyle editModesToolbar;
        public GUIStyle markerWarning;

        static internal DirectorStyles s_Instance;

        readonly float k_Indent = 10.0f;

        DirectorNamedColor m_DarkSkinColors;
        DirectorNamedColor m_LightSkinColors;
        DirectorNamedColor m_DefaultSkinColors;

        static readonly string s_DarkSkinPath = "Editors/TimelineWindow/Timeline_DarkSkin.txt";
        static readonly string s_LightSkinPath = "Editors/TimelineWindow/Timeline_LightSkin.txt";

        static readonly GUIContent s_TempContent = new GUIContent();

        public static bool IsInitialized
        {
            get { return s_Instance != null; }
        }

        public static DirectorStyles Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new DirectorStyles();
                    s_Instance.Initialize();
                }

                return s_Instance;
            }
        }

        public static void ReloadStylesIfNeeded()
        {
            if (Instance.ShouldLoadStyles())
            {
                Instance.LoadStyles();
                if (!Instance.ShouldLoadStyles())
                    Instance.Initialize();
            }
        }

        public DirectorNamedColor customSkin
        {
            get { return EditorGUIUtility.isProSkin ? m_DarkSkinColors : m_LightSkinColors; }
            internal set
            {
                if (EditorGUIUtility.isProSkin)
                    m_DarkSkinColors = value;
                else
                    m_LightSkinColors = value;
            }
        }

        DirectorNamedColor LoadColorSkin(string path)
        {
            var asset = EditorGUIUtility.LoadRequired(path) as TextAsset;

            if (asset != null && !string.IsNullOrEmpty(asset.text))
            {
                return DirectorNamedColor.CreateAndLoadFromText(asset.text);
            }

            return m_DefaultSkinColors;
        }

        static DirectorNamedColor CreateDefaultSkin()
        {
            var nc = ScriptableObject.CreateInstance<DirectorNamedColor>();
            nc.SetDefault();
            return nc;
        }

        public void ExportSkinToFile()
        {
            if (customSkin == m_DarkSkinColors)
                customSkin.ToText(s_DarkSkinPath);

            if (customSkin == m_LightSkinColors)
                customSkin.ToText(s_LightSkinPath);
        }

        public void ReloadSkin()
        {
            if (customSkin == m_DarkSkinColors)
            {
                m_DarkSkinColors = LoadColorSkin(s_DarkSkinPath);
            }
            else if (customSkin == m_LightSkinColors)
            {
                m_LightSkinColors = LoadColorSkin(s_LightSkinPath);
            }
        }

        public string cutomSkinContext
        {
            get
            {
                if (customSkin == m_DarkSkinColors)
                    return "Dark Skin";

                if (customSkin == m_LightSkinColors)
                    return "Light Skin";

                return "Default";
            }
        }

        public void Initialize()
        {
            m_DefaultSkinColors = CreateDefaultSkin();
            m_DarkSkinColors = LoadColorSkin(s_DarkSkinPath);
            m_LightSkinColors = LoadColorSkin(s_LightSkinPath);

            // add the built in colors (control track uses attribute)
            TrackResourceCache.ClearTrackColorCache();
            TrackResourceCache.SetTrackColor<AnimationTrack>(customSkin.colorAnimation);
            TrackResourceCache.SetTrackColor<PlayableTrack>(Color.white);
            TrackResourceCache.SetTrackColor<AudioTrack>(customSkin.colorAudio);
            TrackResourceCache.SetTrackColor<ActivationTrack>(customSkin.colorActivation);
            TrackResourceCache.SetTrackColor<GroupTrack>(customSkin.colorGroup);
            TrackResourceCache.SetTrackColor<ControlTrack>(customSkin.colorControl);

            // add default icons
            TrackResourceCache.ClearTrackIconCache();
            TrackResourceCache.SetTrackIcon<AnimationTrack>(EditorGUIUtility.IconContent("AnimationClip Icon"));
            TrackResourceCache.SetTrackIcon<AudioTrack>(EditorGUIUtility.IconContent("AudioSource Icon"));
            TrackResourceCache.SetTrackIcon<PlayableTrack>(EditorGUIUtility.IconContent("cs Script Icon"));
            TrackResourceCache.SetTrackIcon<ActivationTrack>(new GUIContent(activation.normal.background));
            TrackResourceCache.SetTrackIcon<SignalTrack>(EditorGUIUtility.IconContent("TimelineSignal"));
        }

        DirectorStyles()
        {
            LoadStyles();
        }

        bool ShouldLoadStyles()
        {
            return endmarker == null ||
                endmarker.name == GUISkin.error.name;
        }

        void LoadStyles()
        {
            endmarker = GetStyle("Icon.Endmarker");
            handLeft = GetStyle("MeTransitionHandleLeft");
            handRight = GetStyle("MeTransitionHandleRight");
            groupBackground = GetStyle("groupBackground");
            displayBackground = GetStyle("sequenceClip");
            fontClip = GetStyle("Font.Clip");
            trackHeaderFont = GetStyle("sequenceTrackHeaderFont");
            groupFont = GetStyle("sequenceGroupFont");
            timeCursor = GetStyle("Icon.TimeCursor");
            tinyFont = GetStyle("tinyFont");
            foldout = GetStyle("Icon.Foldout");
            mute = GetStyle("Icon.Mute");
            locked = GetStyle("Icon.Locked");
            autoKey = GetStyle("Icon.AutoKey");
            playTimeRangeStart = GetStyle("Icon.PlayAreaStart");
            playTimeRangeEnd = GetStyle("Icon.PlayAreaEnd");
            options = GetStyle("Icon.Options");
            selectedStyle = GetStyle("Color.Selected");
            trackSwatchStyle = GetStyle("Icon.TrackHeaderSwatch");
            connector = GetStyle("Icon.Connector");
            keyframe = GetStyle("Icon.Keyframe");
            warning = GetStyle("Icon.Warning");
            extrapolationHold = GetStyle("Icon.ExtrapolationHold");
            extrapolationLoop = GetStyle("Icon.ExtrapolationLoop");
            extrapolationPingPong = GetStyle("Icon.ExtrapolationPingPong");
            extrapolationContinue = GetStyle("Icon.ExtrapolationContinue");
            outlineBorder = GetStyle("Icon.OutlineBorder");
            timelineClip = GetStyle("Icon.Clip");
            timelineClipSelected = GetStyle("Icon.ClipSelected");
            bottomShadow = GetStyle("Icon.Shadow");
            trackOptions = GetStyle("Icon.TrackOptions");
            infiniteTrack = GetStyle("Icon.InfiniteTrack");
            blendMixIn = GetStyle("Icon.BlendMixIn");
            blendMixOut = GetStyle("Icon.BlendMixOut");
            blendEaseIn = GetStyle("Icon.BlendEaseIn");
            blendEaseOut = GetStyle("Icon.BlendEaseOut");
            clipOut = GetStyle("Icon.ClipOut");
            clipIn = GetStyle("Icon.ClipIn");
            curves = GetStyle("Icon.Curves");
            lockedBG = GetStyle("Icon.LockedBG");
            activation = GetStyle("Icon.Activation");
            playrange = GetStyle("Icon.Playrange");
            lockButton = GetStyle("IN LockButton");
            avatarMaskOn = GetStyle("Icon.AvatarMaskOn");
            avatarMaskOff = GetStyle("Icon.AvatarMaskOff");
            editModesToolbar = GetStyle("EditModeToolbar");
            collapseMarkers = "TrackCollapseMarkerButton";
            markerMultiOverlay = "MarkerMultiOverlay";

            playrangeContent = new GUIContent(playrange.normal.background);
            playrangeContent.tooltip = "Toggle play range markers.";

            fontClipLoop = new GUIStyle(fontClip) { fontStyle = FontStyle.Bold };

            // temporary until in package uss styles are working
            markerWarning = EditorStyles.FromUSS("MarkerMultiOverlay",
                "height: 13; width: 14; background-image: resource(\"Packages/com.unity.timeline/Editor/StyleSheets/Images/Timeline-Marker-Warning-Overlay.png\");"
            );
        }

        public GUIStyle GetStyle(string s)
        {
            return new GUIStyle(s);
        }

        public float indentWidth
        {
            get { return k_Indent; }
        }

        public static string Elipsify(string label, Rect rect, GUIStyle style)
        {
            var ret = label;

            if (label.Length == 0)
                return ret;

            s_TempContent.text = label;
            float neededWidth = style.CalcSize(s_TempContent).x;

            return Elipsify(label, rect.width, neededWidth);
        }

        public static string Elipsify(string label, float destinationWidth, float neededWidth)
        {
            var ret = label;

            if (label.Length == 0)
                return ret;

            if (destinationWidth < neededWidth)
            {
                float averageWidthOfOneChar = neededWidth / label.Length;
                int floor = Mathf.Max((int)Mathf.Floor(destinationWidth / averageWidthOfOneChar), 0);

                if (floor < k_Elipsis.Length)
                    ret = string.Empty;
                else if (floor == k_Elipsis.Length)
                    ret = k_Elipsis;
                else if (floor < label.Length)
                    ret = label.Substring(0, floor - k_Elipsis.Length) + k_Elipsis;
            }

            return ret;
        }
    }
}
