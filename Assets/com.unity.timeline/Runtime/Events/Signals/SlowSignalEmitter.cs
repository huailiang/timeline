using System;
using System.IO;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{

    [Serializable]
    [CustomStyle("SlowSignalEmitter")]
    [MarkerAttribute(TrackType.MARKER)]
    public class SlowSignalEmitter : Marker,
        IXMarker, 
        INotification, 
        INotificationOptionProvider,
        IDirectorIO
    {
        [SerializeField] bool m_Retroactive;
        [SerializeField] bool m_EmitOnce;
        [SerializeField] float m_SlowRate;

        public bool retroactive
        {
            get { return m_Retroactive; }
            set { m_Retroactive = value; }
        }

        public bool emitOnce
        {
            get { return m_EmitOnce; }
            set { m_EmitOnce = value; }
        }

        public float slowRate
        {
            get { return m_SlowRate; }
            set { m_SlowRate = value; }
        }

        public PropertyName id
        {
            get { return new PropertyName("SlowSignalEmitter"); }
        }

        public MarkType markType => MarkType.SLOW;

        NotificationFlags INotificationOptionProvider.flags
        {
            get
            {
                return (retroactive ? NotificationFlags.Retroactive : default(NotificationFlags)) |
                    (emitOnce ? NotificationFlags.TriggerOnce : default(NotificationFlags)) |
                    NotificationFlags.TriggerInEditMode;
            }
        }


        public void Load(BinaryReader reader, XTrackAsset track)
        {
            m_SlowRate = reader.ReadSingle();
            m_Retroactive = reader.ReadBoolean();
            m_EmitOnce = reader.ReadBoolean();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(m_SlowRate);
            writer.Write(m_Retroactive);
            writer.Write(m_EmitOnce);
        }
    }

}
