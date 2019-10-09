using System.Collections.Generic;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{

    public class XDirectorAsset : PlayableAsset
    {

        private double _duration;

        private XTrackAsset[] _trackAssets;

        private ScriptPlayable<TimelinePlayable> _behaviour;

        public XTrackAsset[] TrackAssets
        {
            get { return _trackAssets; }
            set { _trackAssets = value; }
        }

        public override double duration
        {
            get { return _duration; }
        }
        

        public override IEnumerable<PlayableBinding> outputs
        {
            get
            {
                if (_trackAssets != null)
                {
                    for (int i = 0; i < _trackAssets.Length; i++)
                    {
                        var track = _trackAssets[i];
                        foreach (var output in track.outputs)
                            yield return output;
                    }
                }
            }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            bool createOutputs = graph.GetPlayableCount() == 0;
            _behaviour = TimelinePlayable.Create(graph, _trackAssets, owner,  false, createOutputs);
            _behaviour.SetPropagateSetTime(true);
            return _behaviour.IsValid() ? _behaviour : Playable.Null;
        }

        
        public void SetDuration(double duration)
        {
            _duration = duration;
        }

        public void SetSpeed(float speed)
        {
            if (_behaviour.IsValid())
            {
                _behaviour.SetSpeed(speed);
            }
        }

    }
}