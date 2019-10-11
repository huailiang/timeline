using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


namespace UnityEditor.Timeline.Signals
{

    [CustomEditor(typeof(AnchorSignalEmitter))]
    public class ArchorSignalEditor : Editor
    {
        TrackAsset parentTrack;
        AnchorTrack anchTrack;
        AnchorSignalEmitter signal;
        Transform bindTf;

        private void OnEnable()
        {
            PlayableDirector director = GameObject.FindObjectOfType<PlayableDirector>();
            signal = target as AnchorSignalEmitter;
            anchTrack = signal.parent as AnchorTrack;
            parentTrack = signal.parent.parent as TrackAsset;
            bindTf = DirectorSystem.FetchAttachOfTrack(parentTrack);

            if (bindTf)
            {
                if (signal.position == Vector3.zero)
                {
                    signal.position = bindTf.transform.localPosition;
                }
                if (signal.rotation == Vector3.zero)
                {
                    signal.rotation = bindTf.transform.localEulerAngles;
                }
                TimelineEditor.inspectedDirector.time = signal.time;
                RecordBindtf();
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Space(4);
            Execute();
            RePaintCurve();
        }

        private void RecordBindtf()
        {
            if (TimelineWindow.instance && bindTf)
            {
                var state = TimelineWindow.instance.state;
                bool isRecording = state.recording && state.IsArmedForRecord(anchTrack);
                if (isRecording)
                {
                    Selection.activeObject = bindTf;
                }
            }
        }


        private void Execute()
        {
            if (bindTf)
            {
                bindTf.position = signal.position;
                bindTf.localEulerAngles = signal.rotation;
            }
        }


        private void RePaintCurve()
        {
            AnchorTrack anTrack = signal.parent as AnchorTrack;
            if (anTrack)
            {
                anTrack.OutputTrackinfo();
            }
        }

    }

}