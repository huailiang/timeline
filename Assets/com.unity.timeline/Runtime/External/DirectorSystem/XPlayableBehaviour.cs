using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    public class XPlayableBehaviour : PlayableBehaviour
    {
        private IntervalTree<RuntimeElement> m_IntervalTree = new IntervalTree<RuntimeElement>();
        private List<ITimelineEvaluateCallback> m_EvaluateCallbacks = new List<ITimelineEvaluateCallback>();


        public override void OnPlayableCreate(Playable playable)
        {
            base.OnPlayableCreate(playable);
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            base.PrepareFrame(playable, info);
        }

        public void Compile(ref PlayableGraph graph,
            XPlayableAsset asset,
            Playable timelinePlayable,
            GameObject go, bool createOutputs)
        {
            var tracks = asset.TrackAssets;
            if (tracks == null)
            {
                Debug.LogError("tracks is null");
                return;
            }

            int cnt = tracks.Length;
            for (int i = 0; i < cnt; i++)
            {
                var track = tracks[i];
                if (track != null && !track.mute)
                {
                    CreateTrackPlayable(graph, timelinePlayable, track, go, createOutputs);
                }
            }
        }

        private Playable CreateTrackPlayable(PlayableGraph graph,
            Playable timelinePlayable,
            XTrackAsset track,
            GameObject go, bool createOutputs)
        {
            if (track.name == "root")
                return timelinePlayable;
            XTrackAsset parentActor = track.parent as XTrackAsset;
            var parentPlayable = parentActor != null ?
                CreateTrackPlayable(graph, timelinePlayable, parentActor, go, createOutputs) : timelinePlayable;
            var actorPlayable = track.CreatePlayableGraph(graph, go, m_IntervalTree, timelinePlayable);
            bool connected = false;

            // Special case for animation tracks
            if (parentPlayable.IsValid() && actorPlayable.IsValid())
            {
                int port = parentPlayable.GetInputCount();
                parentPlayable.SetInputCount(port + 1);
                connected = graph.Connect(actorPlayable, 0, parentPlayable, port);
                parentPlayable.SetInputWeight(port, 1.0f);
            }

            if (createOutputs && connected)
            {
                CreateTrackOutput(graph, track, go, parentPlayable, parentPlayable.GetInputCount() - 1);
            }

            return actorPlayable;
        }

        void CreateTrackOutput(PlayableGraph graph, XTrackAsset track, GameObject go, Playable playable, int port)
        {
            if (track.isSubTrack) return;

            var bindings = track.outputs;
            foreach (var binding in bindings)
            {
                var playableOutput = binding.CreateOutput(graph);
                playableOutput.SetReferenceObject(binding.sourceObject);
                playableOutput.SetSourcePlayable(playable);
                playableOutput.SetSourceOutputPort(port);
                playableOutput.SetWeight(1.0f);

                // only apply this on our animation track
                if (track.trackType == TrackType.ANIMTION)
                    EvaluateWeightsForAnimationPlayableOutput((AnimationPlayableOutput)playableOutput);
                if (playableOutput.IsPlayableOutputOfType<AudioPlayableOutput>())
                    ((AudioPlayableOutput)playableOutput).SetEvaluateOnSeek(true);
            }
        }

        void EvaluateWeightsForAnimationPlayableOutput(AnimationPlayableOutput animOutput)
        {
            m_EvaluateCallbacks.Add(new AnimationOutputWeightProcessor(animOutput));
        }

    }

}