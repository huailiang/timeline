using UnityEngine.Playables;
using UnityEngine;

namespace UnityEngine.Timeline
{
    public class TimelineUtil
    {

        private static PlayableDirector _director;
        private static IInterface m_Interface;
        

        public static PlayableDirector Director
        {
            get
            {
                if (_director == null)
                {
                    _director = GameObject.FindObjectOfType<PlayableDirector>();
                }
                return _director;
            }
        }

        public static IInterface Interface
        {
            get { return m_Interface; }
            set { m_Interface = value; }
        }


        public static TrackAsset GetRootTrack(Marker imaker)
        {
            if (imaker)
            {
                var director = Director;
                TrackAsset track = imaker.parent;
                while (track)
                {
                    if (track.parent == null ||
                        track.parent == director.playableAsset)
                    {
                        return track;
                    }
                    else
                    {
                        track = track.parent as TrackAsset;
                    }
                }
            }
            return null;
        }


        public static Transform FetchAttachOfTrack(TrackAsset track)
        {
            if (track != null)
            {
                while (true)
                {
                    //some times this will be crash
                    var parent = track.parent as TrackAsset;
                    if (parent == null) break;
                    else track = parent;
                }
                var binding = Director.GetGenericBinding(track);
                if (binding is Animator)
                {
                    return (binding as Animator).transform;
                }
                else if (binding is Animation)
                {
                    return (binding as Animation).transform;
                }
                else if (binding is GameObject)
                {
                    return (binding as GameObject).transform;
                }
            }
            return null;
        }

        public static T Load<T>(string location, Transform parent, Vector3 pos, Quaternion rot) where T : Object
        {
            if (m_Interface != null)
                return m_Interface.Load<T>(location, parent, pos, rot);
            else
            {
                Debug.LogWarning("timeline interface is null");
                return default(T);
            }
        }

    }

}