using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


namespace UnityEditor.Timeline.Signals
{
    [CustomEditor(typeof(TransforSignalEmitter))]
    public class TransforSignalEditor : Editor
    {
        TrackAsset track;
        TransforSignalEmitter signal;
        Transform bindTf;

        private void OnEnable()
        {
            PlayableDirector director = GameObject.FindObjectOfType<PlayableDirector>();
            signal = target as TransforSignalEmitter;
            track = signal.parent.parent as TrackAsset;
            var binding = director.GetGenericBinding(track);
            if (binding is Animator)
            {
                bindTf = (binding as Animator).transform;
            }
            else if (binding is Animation)
            {
                bindTf = (binding as Animation).transform;
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Space(4);
            Execute();
            GUILayout.Label("try drag transform option");
        }



        private void Execute()
        {
            if (bindTf)
            {
                bindTf.position = signal.position;
                bindTf.localEulerAngles = signal.rotation;
            }
        }

    }

}