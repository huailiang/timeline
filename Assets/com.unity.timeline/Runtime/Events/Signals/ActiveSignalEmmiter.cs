using System;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{

    [Serializable]
    [CustomStyle("ActiveSignalmEmitter")]
    [Marker(TrackType.ANIMTION | TrackType.CONTROL)]
    public partial class ActiveSignalEmmiter : Marker,IXMarker, INotification, INotificationOptionProvider
    {
        [SerializeField] protected bool m_Retroactive;
        [SerializeField] protected bool m_EmitOnce;
        [SerializeField] bool m_active = true;


        public bool Active
        {
            get { return m_active; }
            set { m_active = value; }
        }

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


        public PropertyName id
        {
            get { return new PropertyName("QteLongPressSignal"); }
        }

        public MarkType markType => MarkType.ACTIVE;

        NotificationFlags INotificationOptionProvider.flags
        {
            get
            {
                return (retroactive ? NotificationFlags.Retroactive : default(NotificationFlags)) |
                    (emitOnce ? NotificationFlags.TriggerOnce : default(NotificationFlags)) |
                    NotificationFlags.TriggerInEditMode;
            }
        }
    }

}