using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


public class TimelineEntry : MonoBehaviour
{

    PlayableDirector director;
    TimelineImp imp;


    private void Awake()
    {
        director = GetComponent<PlayableDirector>();
        imp = new TimelineImp();
        TimelineUtil.Interface = imp;
        imp.notify = OnNotify;
    }
    

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.F1))
        {
            var output = ScriptPlayableOutput.Create(director.playableGraph, "");
            JumpSignalEmmiter sign = ScriptableObject.CreateInstance<JumpSignalEmmiter>();
            sign.jumpTime = 0;
            output.PushNotification(Playable.Null, sign);
        }
    }

    public void OnRecSignal1()
    {
        Debug.Log("recv signal 1" + name);
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

    public void OnNotify(Playable origin, INotification notification)
    {
        if (director == null) Awake();
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
        else if (notification is ActiveSignalEmmiter)
        {
            ActiveSignalEmmiter signal = notification as ActiveSignalEmmiter;
            TrackAsset track = TimelineUtil.GetRootTrack(signal);
            Transform tf = ExternalHelp.FetchAttachOfTrack(director, track);
            if (tf) tf.gameObject.SetActive(signal.Active);
        }
    }

}