using System;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    /// <summary>
    /// A Timeline track that can play AudioClips.
    /// </summary>
    [Serializable]
    [TrackClipType(typeof(AudioPlayableAsset), false)]
    [TrackBindingType(typeof(AudioSource))]
    [TrackAttribute(false)]
    public class AudioTrack : XTrackAsset
    {
        [SerializeField]
        AudioMixerProperties m_TrackProperties = new AudioMixerProperties();

#if UNITY_EDITOR
        Playable m_LiveMixerPlayable = Playable.Null;

#endif

        /// <summary>
        /// Create an TimelineClip for playing an AudioClip on this track.
        /// </summary>
        /// <param name="clip">The audio clip to play</param>
        /// <returns>A TimelineClip with an AudioPlayableAsset asset.</returns>
        public TimelineClip CreateClip(AudioClip clip)
        {
            if (clip == null)
                return null;

            var newClip = CreateDefaultClip();

            var audioAsset = newClip.asset as AudioPlayableAsset;
            if (audioAsset != null)
                audioAsset.clip = clip;

            newClip.duration = clip.length;
            newClip.displayName = clip.name;

            return newClip;
        }

        internal override Playable OnCreateClipPlayableGraph(PlayableGraph graph, GameObject go, IntervalTree<RuntimeElement> tree)
        {
            var clipBlender = AudioMixerPlayable.Create(graph, clips.Length);

            // In the player, we do not add the AudioMixerProperties behaviour to the track if it contains no animation
            // however in the editor we want to always have it present to be able to preview changes live.
#if UNITY_EDITOR
            clipBlender.GetHandle().SetScriptInstance(m_TrackProperties.Clone());
            m_LiveMixerPlayable = clipBlender;
#else
            if (hasCurves)
                clipBlender.GetHandle().SetScriptInstance(m_TrackProperties.Clone());
#endif

            InitializeClips(graph, go, tree, clipBlender);

            return clipBlender;
        }

        void InitializeClips(PlayableGraph graph, GameObject go, IntervalTree<RuntimeElement> tree, AudioMixerPlayable clipBlender)
        {
            for (int i = 0; i < clips.Length; i++)
            {
                var c = clips[i];
                var asset = c.asset as PlayableAsset;
                if (asset == null)
                    continue;

                var buffer = 0.1f;
                var audioAsset = c.asset as AudioPlayableAsset;
                if (audioAsset != null)
                    buffer = audioAsset.bufferingTime;

                var source = asset.CreatePlayable(graph, go);
                if (!source.IsValid())
                    continue;

                if (source.IsPlayableOfType<AudioClipPlayable>())
                {
                    // Enforce initial values on all clips
                    var audioClipPlayable = (AudioClipPlayable)source;
                    var audioClipProperties = audioClipPlayable.GetHandle().GetObject<AudioClipProperties>();

                    audioClipPlayable.SetVolume(Mathf.Clamp01(m_TrackProperties.volume * audioClipProperties.volume));
                    audioClipPlayable.SetStereoPan(Mathf.Clamp(m_TrackProperties.stereoPan, -1.0f, 1.0f));
                    audioClipPlayable.SetSpatialBlend(Mathf.Clamp01(m_TrackProperties.spatialBlend));
                }

                tree.Add(new ScheduleRuntimeClip(c, source, clipBlender, buffer));
                graph.Connect(source, 0, clipBlender, i);
                source.SetSpeed(c.timeScale);
                source.SetDuration(c.extrapolatedDuration);
                clipBlender.SetInputWeight(source, 1.0f);
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<PlayableBinding> outputs
        {
            get { yield return AudioPlayableBinding.Create(name, this); }
        }

#if UNITY_EDITOR
        internal void LiveLink()
        {
            if (!m_LiveMixerPlayable.IsValid())
                return;

            var audioMixerProperties = m_LiveMixerPlayable.GetHandle().GetObject<AudioMixerProperties>();

            if (audioMixerProperties == null)
                return;

            audioMixerProperties.volume = m_TrackProperties.volume;
            audioMixerProperties.stereoPan = m_TrackProperties.stereoPan;
            audioMixerProperties.spatialBlend = m_TrackProperties.spatialBlend;
        }

#endif

        void OnValidate()
        {
            m_TrackProperties.volume = Mathf.Clamp01(m_TrackProperties.volume);
            m_TrackProperties.stereoPan = Mathf.Clamp(m_TrackProperties.stereoPan, -1.0f, 1.0f);
            m_TrackProperties.spatialBlend = Mathf.Clamp01(m_TrackProperties.spatialBlend);
        }
    }
}
