using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    // Does a post processing of the weights on an animation track to properly normalize
    // the mixer weights so that blending does not bring default poses and subtracks, layers and
    // layer graphs blend correctly
    class AnimationOutputWeightProcessor : ITimelineEvaluateCallback
    {
        struct WeightInfo
        {
            public Playable mixer;
            public Playable parentMixer;
            public int port;
            public bool modulate;
        }

        AnimationPlayableOutput m_Output;
        AnimationMotionXToDeltaPlayable m_MotionXPlayable;
        AnimationMixerPlayable m_PoseMixer;
        AnimationLayerMixerPlayable m_LayerMixer;
        readonly List<WeightInfo> m_Mixers = new List<WeightInfo>();

        public AnimationOutputWeightProcessor(AnimationPlayableOutput output)
        {
            m_Output = output;
            output.SetWeight(0);
            FindMixers();
        }

        static Playable FindFirstAnimationPlayable(Playable p)
        {
            var currentNode = p;
            while (currentNode.IsValid() && currentNode.GetInputCount() > 0
                   && !currentNode.IsPlayableOfType<AnimationLayerMixerPlayable>()
                   && !currentNode.IsPlayableOfType<AnimationMotionXToDeltaPlayable>()
                   && !currentNode.IsPlayableOfType<AnimationMixerPlayable>())
                currentNode = currentNode.GetInput(0);

            return currentNode;
        }

        void FindMixers()
        {
            m_Mixers.Clear();
            m_PoseMixer = AnimationMixerPlayable.Null;
            m_LayerMixer = AnimationLayerMixerPlayable.Null;
            m_MotionXPlayable = AnimationMotionXToDeltaPlayable.Null;

            var playable = m_Output.GetSourcePlayable();
            var outputPort = m_Output.GetSourceOutputPort();
            if (!playable.IsValid() || outputPort < 0 || outputPort >= playable.GetInputCount())
                return;

            var mixer = FindFirstAnimationPlayable(playable.GetInput(outputPort));

            Playable motionXPlayable = mixer;
            if (motionXPlayable.IsPlayableOfType<AnimationMotionXToDeltaPlayable>())
            {
                m_MotionXPlayable = (AnimationMotionXToDeltaPlayable)motionXPlayable;
                mixer = m_MotionXPlayable.GetInput(0);
            }

            if (mixer.IsValid() && mixer.IsPlayableOfType<AnimationMixerPlayable>())
            {
                m_PoseMixer = (AnimationMixerPlayable)mixer;
                Playable layerMixer = m_PoseMixer.GetInput(0);

                if (layerMixer.IsValid() && layerMixer.IsPlayableOfType<AnimationLayerMixerPlayable>())
                    m_LayerMixer = (AnimationLayerMixerPlayable)layerMixer;
            }
            else if (mixer.IsValid() && mixer.IsPlayableOfType<AnimationLayerMixerPlayable>())
            {
                m_LayerMixer = (AnimationLayerMixerPlayable)mixer;
            }


            if (!m_LayerMixer.IsValid())
                return;

            var count = m_LayerMixer.GetInputCount();
            for (var i = 0; i < count; i++)
            {
                FindMixers(m_LayerMixer, i, m_LayerMixer.GetInput(i));
            }
        }

        // Recursively accumulates mixers.
        void FindMixers(Playable parent, int port, Playable node)
        {
            if (!node.IsValid())
                return;

            var type = node.GetPlayableType();
            if (type == typeof(AnimationMixerPlayable) || type == typeof(AnimationLayerMixerPlayable))
            {
                // use post fix traversal so children come before parents
                int subCount = node.GetInputCount();
                for (int j = 0; j < subCount; j++)
                {
                    FindMixers(node, j, node.GetInput(j));
                }

                // if we encounter a layer mixer, we assume there is nesting occuring
                //  and we modulate the weight instead of overwriting it.
                var weightInfo = new WeightInfo
                {
                    parentMixer = parent,
                    mixer = node,
                    port = port,
                    modulate = (type == typeof(AnimationLayerMixerPlayable))
                };
                m_Mixers.Add(weightInfo);
            }
            else
            {
                var count = node.GetInputCount();
                for (var i = 0; i < count; i++)
                {
                    FindMixers(parent, port, node.GetInput(i));
                }
            }
        }

        public void Evaluate()
        {
            m_Output.SetWeight(1);
            for (int i = 0; i < m_Mixers.Count; i++)
            {
                var mixInfo = m_Mixers[i];
                float weight = mixInfo.modulate ? mixInfo.parentMixer.GetInputWeight(mixInfo.port) : 1.0f;
                mixInfo.parentMixer.SetInputWeight(mixInfo.port, weight * WeightUtility.NormalizeMixer(mixInfo.mixer));
            }

            float normalizedWeight = WeightUtility.NormalizeMixer(m_LayerMixer);

            var animator = m_Output.GetTarget();
            if (animator == null)
                return;

            // AnimationMotionXToDeltaPlayable must blend with default values when previewing tracks with absolute root motion.
            bool blendMotionX = !Application.isPlaying && m_MotionXPlayable.IsValid() && m_MotionXPlayable.IsAbsoluteMotion();

            if (blendMotionX)
            {
                m_PoseMixer.SetInputWeight(0, normalizedWeight);
                m_PoseMixer.SetInputWeight(1, 1.0f - normalizedWeight);
            }
            else
            {
                if (!m_PoseMixer.Equals(AnimationMixerPlayable.Null))
                {
                    m_PoseMixer.SetInputWeight(0, 1.0f);
                    m_PoseMixer.SetInputWeight(1, 0.0f);
                }

                m_Output.SetWeight(normalizedWeight);
            }
        }
    }
}
