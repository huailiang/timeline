using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    public class XTrackAsset : PlayableAsset
    {
        static event Action<TimelineClip, GameObject, Playable> OnClipPlayableCreate;
        private double m_Start;
        private double m_End;
        private TrackType m_TrackType;
        private string m_Name;
        private bool m_Mute;

        public Playable playable;
        public PlayableOutput playableOutput;
        private int m_ParentIndex;
        protected internal List<TimelineClip> m_Clips = new List<TimelineClip>();

        MarkerList m_Markers = new MarkerList(0);

        public sealed override double duration
        {
            get { return m_End - m_Start; }
        }

        public PlayableAsset parent;

        public TrackType trackType
        {
            get { return m_TrackType; }
        }

        public bool isSubTrack
        {
            get { return m_ParentIndex >= 0; }
        }

        public bool mute
        {
            get { return m_Mute; }
        }

        public override IEnumerable<PlayableBinding> outputs
        {
            get
            {
                Type type = DirectorSystem.UtilTrackType(m_TrackType);
                yield return ScriptPlayableBinding.Create(m_Name, this, type);
            }
        }


        protected virtual void OnCreateClip(TimelineClip clip) { }
        public virtual Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return Playable.Create(graph, inputCount);
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return Playable.Null;
        }

        protected virtual Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
        {
            var asset = clip.asset as IPlayableAsset;
            if (asset != null)
            {
                var handle = asset.CreatePlayable(graph, gameObject);
                if (handle.IsValid())
                {
                    handle.SetAnimatedProperties(clip.curves);
                    handle.SetSpeed(clip.timeScale);
                    if (OnClipPlayableCreate != null)
                        OnClipPlayableCreate(clip, gameObject, handle);
                }
                return handle;
            }
            return Playable.Null;
        }

        internal Playable CreatePlayableGraph(PlayableGraph graph, GameObject go, IntervalTree<RuntimeElement> tree, Playable timelinePlayable)
        {
            var mixerPlayable = Playable.Null;
            if (m_Clips.Count > 0)
            {
                mixerPlayable = OnCreateClipPlayableGraph(graph, go, tree);
            }

            var notificationsPlayable = CreateNotificationsPlayable(graph, mixerPlayable, go, timelinePlayable);
            if (!notificationsPlayable.IsValid() && !mixerPlayable.IsValid())
            {
                Debug.LogErrorFormat("Track {0} of type {1} has no notifications and returns an invalid mixer Playable", name,
                    GetType().FullName);

                return Playable.Create(graph);
            }

            var playableGraph = m_Markers.Count > 0 ? notificationsPlayable : mixerPlayable;
            ConfigureTrackAnimation(tree, go, playableGraph);

            return playableGraph;
        }

        void ConfigureTrackAnimation(IntervalTree<RuntimeElement> tree, GameObject go, Playable blend)
        {
            if (trackType == TrackType.ANIMTION)
            {
                //blend.SetAnimatedProperties(m_Curves);
                tree.Add(new InfiniteRuntimeClip(blend));
            }
        }

        Playable CreateNotificationsPlayable(PlayableGraph graph, Playable mixerPlayable, GameObject go, Playable timelinePlayable)
        {
            var notificationPlayable = NotificationUtilities.CreateNotificationsPlayable(graph, m_Markers.GetMarkers(), go);
            if (notificationPlayable.IsValid())
            {
                notificationPlayable.GetBehaviour().timeSource = timelinePlayable;
                if (mixerPlayable.IsValid())
                {
                    notificationPlayable.SetInputCount(1);
                    graph.Connect(mixerPlayable, 0, notificationPlayable, 0);
                    notificationPlayable.SetInputWeight(mixerPlayable, 1);
                }
            }

            return notificationPlayable;
        }

        internal virtual Playable OnCreateClipPlayableGraph(PlayableGraph graph, GameObject go,
                IntervalTree<RuntimeElement> tree)
        {
            int clipCount = m_Clips.Count;
            Playable blend = CreateTrackMixer(graph, go, clipCount);
            for (var c = 0; c < clipCount; c++)
            {
                Playable source = CreatePlayable(graph, go, m_Clips[c]);
                if (source.IsValid())
                {
                    source.SetDuration(m_Clips[c].duration);
                    var clip = new RuntimeClip(m_Clips[c], source, blend);
                    tree.Add(clip);
                    graph.Connect(source, 0, blend, c);
                    blend.SetInputWeight(c, 0.0f);
                }
            }
            return blend;
        }


        public void Load(BinaryReader reader)
        {
            m_Start = reader.ReadDouble();
            m_End = reader.ReadDouble();
            m_Name = reader.ReadString();
            m_TrackType = (TrackType)reader.ReadInt32();
            m_ParentIndex = reader.ReadInt32();
            m_Mute = reader.ReadBoolean();
        }

        public void OnPostLoad(XPlayableAsset playable)
        {
            if (playable && m_ParentIndex >= 0)
            {
                parent = playable.TrackAssets[m_ParentIndex];
            }
        }


        public void AddClip(TimelineClip newClip)
        {
            m_Clips.Add(newClip);
        }

        public void AddMarker(ScriptableObject e)
        {
            m_Markers.Add(e);
        }



        internal TimelineClip CreateClip(PlayableAsset playableAsset)
        {
            if (playableAsset != null)
            {
                TimelineCreateUtilities.SaveAssetIntoObject(playableAsset, this);
                var clip = CreateClipFromAsset(playableAsset);
                AddClip(clip);
                return clip;
            }
            return null;
        }

        private TimelineClip CreateClipFromAsset(ScriptableObject playableAsset)
        {
            var newClip = CreateNewClipContainerInternal();
            newClip.displayName = playableAsset.name;
            newClip.asset = playableAsset;

            IPlayableAsset iPlayableAsset = playableAsset as IPlayableAsset;
            if (iPlayableAsset != null)
            {
                var candidateDuration = iPlayableAsset.duration;

                if (!double.IsInfinity(candidateDuration) && candidateDuration > 0)
                    newClip.duration = Math.Min(Math.Max(candidateDuration, TimelineClip.kMinDuration), TimelineClip.kMaxTimeValue);
            }

            try
            {
                OnCreateClip(newClip);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message, playableAsset);
                return null;
            }

            return newClip;
        }

        internal TimelineClip CreateNewClipContainerInternal()
        {
            var clipContainer = new TimelineClip(/*this*/null);
            clipContainer.asset = null;

            // position clip at end of sequence
            var newClipStart = 0.0;
            for (var a = 0; a < m_Clips.Count - 1; a++)
            {
                var clipDuration = m_Clips[a].duration;
                if (double.IsInfinity(clipDuration))
                    clipDuration = TimelineClip.kDefaultClipDurationInSeconds;
                newClipStart = Math.Max(newClipStart, m_Clips[a].start + clipDuration);
            }

            clipContainer.mixInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            clipContainer.mixOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
            return clipContainer;
        }

    }

}