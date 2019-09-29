using System.Collections.Generic;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{

    public class XPlayableAsset : PlayableAsset
    {

        private double _duration;

        private XTrackAsset[] _trackAssets;


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
            throw new System.NotImplementedException();
        }

        public void SetDuration(double duration)
        {
            _duration = duration;
        }

    }

}