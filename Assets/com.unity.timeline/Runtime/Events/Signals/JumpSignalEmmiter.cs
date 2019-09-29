using System;
using System.IO;
using UnityEngine.Playables;


namespace UnityEngine.Timeline
{

    [Serializable]
    [CustomStyle("JumpSignalEmmiter")]
    [MarkerAttribute(TrackType.MARKER)]
    public class JumpSignalEmmiter : Marker,
        IXMarker,
        INotification,
        INotificationOptionProvider,
        IDirectorIO
    {
        [SerializeField] bool m_EmitOnce;
        [SerializeField] float m_JumpTime;


        public bool emitOnce
        {
            get { return m_EmitOnce; }
            set { m_EmitOnce = value; }
        }

        public float jumpTime
        {
            get { return m_JumpTime; }
            set { m_JumpTime = value; }
        }

        public PropertyName id
        {
            get
            {
                return new PropertyName("JumpSignalEmmiter");
            }
        }

        public MarkType markType => MarkType.JUMP;

        NotificationFlags INotificationOptionProvider.flags
        {
            get
            {
                return (emitOnce ? NotificationFlags.TriggerOnce : default(NotificationFlags)) |
                    NotificationFlags.TriggerInEditMode;
            }
        }

        public void Load(BinaryReader reader)
        {
            jumpTime = reader.ReadSingle();
            m_EmitOnce = reader.ReadBoolean();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(jumpTime);
            writer.Write(m_EmitOnce);
        }

    }

}