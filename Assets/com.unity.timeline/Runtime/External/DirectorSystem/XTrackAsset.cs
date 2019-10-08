using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{

   
    public class XTrackAsset : TrackAsset
    {
        public Playable playable;
        public PlayableOutput playableOutput;
        public GameObject bindObj;
        public int parentIndex;
        protected TrackType m_TrackType;

        public TrackType trackType
        {
            get { return m_TrackType; }
        }

        public override IEnumerable<PlayableBinding> outputs
        {
            get
            {
                Type type = DirectorSystem.UtilTrackType(m_TrackType);
                yield return ScriptPlayableBinding.Create(name, this, type);
            }
        }


        public void Load(BinaryReader reader)
        {
            name = reader.ReadString();
            m_TrackType = (TrackType)reader.ReadInt32();
            m_Start = reader.ReadDouble();
            m_End = reader.ReadDouble();
            parentIndex = reader.ReadInt32();
            muted = reader.ReadBoolean();
        }

        public override string ToString()
        {
            return string.Format("track {0} duration:{1:.00} start:{2:.00} end:{3:.00}", m_TrackType, duration, m_Start, m_End);
        }

        public void OnPostLoad(XDirectorAsset playable)
        {
            if (playable)
            {
                if (parentIndex >= 0)
                {
                    XTrackAsset pTrack = playable.TrackAssets[parentIndex];
                    this.parent = pTrack;
                    int cnt = m_Clips.Count;
                    for (int i = 0; i < cnt; i++)
                    {
                        var playableAsset = m_Clips[i].asset as ClipPlayaleAsset;
                        if (playableAsset)
                        {
                            playableAsset.SetBind(pTrack.bindObj); ;
                        }
                    }
                }
                else
                {
                    parent = playable;
                }
            }
        }

        internal TimelineClip CreateClip(PlayableAsset playableAsset)
        {
            if (playableAsset == null)
            {
                throw new System.InvalidOperationException("your's clip created is null");
            }
            playableAsset.hideFlags |= HideFlags.DontSave;
            TimelineClip clip = CreateClipFromAsset(playableAsset);
            clip.asset = playableAsset;
            AddClip(clip);
            return clip;
        }

    }

}