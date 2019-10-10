using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;



[ExecuteInEditMode]
public class TimelineEntry : MonoBehaviour, IInterface
{

    PlayableDirector director;
    TimelineLoader loader;
    private bool backward;
    private GUIStyle style = new GUIStyle();
    private Rect rect = new Rect(20, 20, 150, 40);

    public NotifyDelegate notify { get; set; }
    

    private void Awake()
    {
        director = GetComponent<PlayableDirector>();
        DirectorSystem.Director = director;
        TimelineUtil.Interface = this;
        notify = OnNotify;
        backward = false;
        TimelineUtil.playMode = Application.isPlaying ?
            TimelinePlayMode.PREVIEWPLAYING :
            TimelinePlayMode.EDITOR;
    }

    void Start()
    {
        style.normal.textColor = Color.red;
        style.fontSize = 18;
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            director.Stop();
            director.time = 0d;
            director.Play();
        }
        if (Input.GetKeyUp(KeyCode.F1))
        {
            var output = ScriptPlayableOutput.Create(director.playableGraph, "");
            JumpSignalEmmiter sign = ScriptableObject.CreateInstance<JumpSignalEmmiter>();
            sign.jumpTime = 0;
            output.PushNotification(Playable.Null, sign);
        }
        if (Input.GetKeyUp(KeyCode.F3))
        {
            director.Pause();
            backward = true;
        }
        if (Input.GetKeyUp(KeyCode.F4))
        {
            backward = false;
            director.Play();
        }
        if (Input.GetKeyUp(KeyCode.F5))
        {
            if (loader == null) loader = new TimelineLoader();
            if (director)
            {
                string path = Application.dataPath + "/Resources/TIMELINE.bytes";
                loader.Load(path, director);
                if (director.playableGraph.IsValid())
                {
                    //director.RebuildGraph();
                    director.Play();
                }
                else
                {
                    Debug.LogError("director graph is invalid");
                }
            }
        }
        if (backward)
        {
            director.time = director.time - Time.deltaTime;
            director.Evaluate();
        }
    }

    private void OnGUI()
    {
        if (director)
        {
            DirectorSystem.Director = director;
            GUI.Label(rect, "frame: " + (director.time * 30).ToString("f0"), style);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        DrawCamera();
    }

    private void DrawCamera()
    {
        Camera camera = Camera.main;

        Gizmos.matrix = Matrix4x4.TRS(camera.transform.position,
        camera.transform.rotation,
        Vector3.one);

        Gizmos.DrawFrustum(Vector3.zero,
        camera.fieldOfView,
        camera.farClipPlane,
        camera.nearClipPlane,
        camera.aspect);
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
            if (director.playableAsset is XDirectorAsset)
            {
                XDirectorAsset xda = director.playableAsset as XDirectorAsset;
                xda.SetSpeed(signal.slowRate);
            }
            else
            {
                director.playableGraph.GetRootPlayable(0).SetSpeed(signal.slowRate);
            }
        }
        else if (notification is ActiveSignalEmmiter)
        {
            ActiveSignalEmmiter signal = notification as ActiveSignalEmmiter;
            TrackAsset track = TimelineUtil.GetRootTrack(signal);
            Transform tf = DirectorSystem.FetchAttachOfTrack(track);
            if (tf) tf.gameObject.SetActive(signal.Active);
        }
    }

}