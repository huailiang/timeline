using System;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{

    [Serializable]
    [CustomStyle("TransforSignalmEmitter")]
    public class AnchorSignalEmitter : Marker
    {
        [SerializeField] Vector3 m_Position = Vector3.zero;

        [SerializeField] Vector3 m_Rotation = Vector3.zero;


        public Vector3 position
        {
            get { return m_Position; }
            set { m_Position = value; }
        }

        public Vector3 rotation
        {
            get { return m_Rotation; }
            set { m_Rotation = value; }
        }

    }

}