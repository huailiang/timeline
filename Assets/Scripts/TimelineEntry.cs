using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;



public class TimelineEntry : MonoBehaviour, INotificationReceiver
{

    PlayableDirector director;


    private void Start()
    {
        director = GetComponent<PlayableDirector>();
        ParseTrack();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.F1))
        {
            var output = ScriptPlayableOutput.Create(director.playableGraph, "");
            output.AddNotificationReceiver(this);
            JumpSignalEmmiter sign = ScriptableObject.CreateInstance<JumpSignalEmmiter>();
            sign.jumpTime = 0;
            output.PushNotification(Playable.Null, sign);
        }
    }

    public void OnRecSignal1()
    {
        Debug.Log("recv signal 1" + name);
    }

    public void OnRecSignal2()
    {
        Debug.Log("recv signal 2" + name);
        director.time = 3.41d;
    }

    public void OnRecbSignal3()
    {
        Debug.Log("recv signal 3" + name);
    }

    public void JumpTo(float time)
    {
        Debug.Log("jump to:" + time);
        director.time = time;
    }

    public void Slowly(float slowRate)
    {
        Debug.Log("slow rate:" + slowRate);
    }

    public void OnNotify(Playable origin, INotification notification, object context)
    {
        if (notification is JumpSignalEmmiter)
        {
            JumpSignalEmmiter signal = notification as JumpSignalEmmiter;
            director.time = signal.jumpTime;
        }
        else if (notification is SlowSignalEmitter)
        {
            SlowSignalEmitter signal = notification as SlowSignalEmitter;
            director.playableGraph.GetRootPlayable(0).SetSpeed(signal.slowRate);
        }
    }

    public void ParseTrack()
    {
        foreach (PlayableBinding pb in director.playableAsset.outputs)
        {
            if (pb.sourceObject is AnimationTrack)
            {
                AnimationTrack track = pb.sourceObject as AnimationTrack;
                int cnt = track.GetMarkerCount();
                var tfTracks = new List<AnchorSignalEmitter>();
                var marks = track.GetMarkers().GetEnumerator();
                while (marks.MoveNext())
                {
                    IMarker mark = marks.Current;
                    if (mark is AnchorSignalEmitter)
                    {
                        tfTracks.Add(mark as AnchorSignalEmitter);
                    }
                }
                marks.Dispose();

                for (int i = 0; i < tfTracks.Count; i++)
                {
                    AnchorSignalEmitter sign = tfTracks[i];
                    Debug.Log(sign.time + " " + sign.position);
                }
            }
        }
    }

}