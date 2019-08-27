using UnityEngine;
using UnityEngine.Playables;

public class TransformBehaviour : PlayableBehaviour
{

    AnimationCurve[] clips;
    GameObject m_Go;

    public void Set(AnimationCurve[] clip, GameObject go)
    {
        clips = clip;
        m_Go = go;
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        base.PrepareFrame(playable, info);
        if (clips != null)
        {
            float time = (float)(playable.GetTime());
            float x = clips[0].Evaluate(time);
            float y = clips[1].Evaluate(time);
            float z = clips[2].Evaluate(time);

            Vector3 pos = new Vector3(x, y, z);

            if (m_Go)
            {
                m_Go.transform.position = pos;
            }
        }
    }

}
