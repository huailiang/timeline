using System.Collections.Generic;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{

    public class XPlayableAsset : PlayableAsset
    {

        private double _duration;

        private XTrackAsset[] _trackAssets;

        private ScriptPlayable<XPlayableBehaviour> _behaviour;

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
            var beh = _behaviour.GetBehaviour();
            if (beh == null)
            {
                _behaviour = CreateBehaviour(graph);
                beh = _behaviour.GetBehaviour();
            }
            int count = graph.GetPlayableCount();
            beh.Compile(ref graph, this, _behaviour, owner, count == 1);
            if (_behaviour.IsValid())
            {
                return _behaviour;
            }
            return Playable.Null;
        }


        private ScriptPlayable<XPlayableBehaviour> CreateBehaviour(PlayableGraph graph)
        {
            var playable = ScriptPlayable<XPlayableBehaviour>.Create(graph);
            playable.SetTraversalMode(PlayableTraversalMode.Passthrough);
            playable.SetPropagateSetTime(true);
            return playable;
        }

        public void SetDuration(double duration)
        {
            _duration = duration;
        }

    }

}