using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Linq;

[CustomEditor(typeof(PlayableDirector))]
public class DirectorInspector : Editor
{

    private Color[] gizColors = { Color.red, Color.green, Color.white };

    [MenuItem("Tool/SelectDirector _F1")]
    public static void FocusDirector()
    {
        GameObject go = GameObject.Find("TIMELINE");
        if (go != null)
        {
            Selection.activeGameObject = go;
        }
    }


    private void OnSceneGUI()
    {
        if (DirectorSystem.Director == null)
        {
            DirectorSystem.Director = target as PlayableDirector;
        }
        DrawAnchors();
    }

    
    private void DrawAnchors()
    {
        AnchorAsset asset = target as AnchorAsset;

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

    const int sample_cnt = 20;
    Vector3[] sample_vtx = new Vector3[sample_cnt];

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

}