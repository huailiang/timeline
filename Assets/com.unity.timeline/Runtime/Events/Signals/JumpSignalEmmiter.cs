using System;
using UnityEngine.Playables;


namespace UnityEngine.Timeline
{

    [Serializable]
    [CustomStyle("JumpSignalEmmiter")]
    [MarkerAttribute(TrackType.MARKER)]
    public class JumpSignalEmmiter : Marker, IXMarker, INotification, INotificationOptionProvider
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
    }


}