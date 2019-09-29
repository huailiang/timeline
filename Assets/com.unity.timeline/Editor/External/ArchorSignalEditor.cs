using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


namespace UnityEditor.Timeline.Signals
{
    [CustomEditor(typeof(AnchorSignalEmitter))]
    public class ArchorSignalEditor : Editor
    {
        TrackAsset track;
        AnchorSignalEmitter signal;
        Transform bindTf;

        private void OnEnable()
        {
            PlayableDirector director = GameObject.FindObjectOfType<PlayableDirector>();
            signal = target as AnchorSignalEmitter;
            track = signal.parent.parent as TrackAsset;
            bindTf = ExternalHelp.FetchAttachOfTrack(director, track);
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