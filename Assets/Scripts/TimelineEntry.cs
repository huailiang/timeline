using System.Linq;
using UnityEditor;
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
        CheckDirector();
        if (DirectorSystem.Director)
        {
            GUI.Label(rect, "frame: " + (DirectorSystem.Director.time * 30).ToString("f0"), style);
        }
    }

    #region gizmos
    private void OnDrawGizmos()
    {
        CheckDirector();
        DrawAnchors();
        Gizmos.color = Color.cyan;
        DrawCamera();
    }


    private void CheckDirector()
    {
        if (DirectorSystem.Director == null)
        {
            DirectorSystem.Director = GameObject.FindObjectOfType<PlayableDirector>();
        }
    }


    private void DrawAnchors()
    {
        if (DirectorSystem.Director.playableAsset != null)
        {
            var list = DirectorSystem.Director.playableAsset.outputs;
            int idx = 0;
            foreach (PlayableBinding pb in list)
            {
                if (pb.sourceObject is AnchorTrack)
                {
                    AnchorTrack tack = pb.sourceObject as AnchorTrack;
                    DrawAnchorTrack(tack, idx++);
                }
            }
        }
    }


    private void DrawAnchorTrack(AnchorTrack track, int idx)
    {
        var clips = track.GetClips();
        TimelineClip clip = clips.FirstOrDefault();
        if (clip != null && clip.asset != null)
        {
            AnchorAsset asset = clip.asset as AnchorAsset;
            DrawAnchorAsset(asset, clip.start, clip.end, idx);
        }
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


    const int sample_cnt = 20;
    Vector3[] sample_vtx = new Vector3[sample_cnt];
    Color[] gizColors = { Color.red, Color.green, Color.white };

    private void DrawAnchorAsset(AnchorAsset asset, double start, double end, int idx)
    {
        FetchKeys(asset, start, end);
        if (sample_vtx != null)
        {
            Handles.color = gizColors[idx % 3];
            for (int i = 0; i < sample_cnt - 1; i++)
            {
                Handles.DrawLine(sample_vtx[i], sample_vtx[i + 1]);
            }
        }
    }

    private void FetchKeys(AnchorAsset asset, double start, double end)
    {
        double dur = end - start;
        double delta = dur / sample_cnt;
        if (asset.IsValid())
        {
            for (int i = 0; i < sample_cnt; i++)
            {
                float time = (float)(start + delta * i);
                float x = asset.clip_pos[0].Evaluate(time);
                float y = asset.clip_pos[1].Evaluate(time);
                float z = asset.clip_pos[2].Evaluate(time);
                sample_vtx[i] = new Vector3(x, y, z);
            }
        }
    } 
    #endregion

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