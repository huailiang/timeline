using UnityEngine;
using UnityEngine.Playables;

public class TransformBehaviour : PlayableBehaviour
{

    AnimationCurve[] clips_pos;
    AnimationCurve[] clips_rot;
    GameObject m_Go;

    public void Set(AnimationCurve[] pos, AnimationCurve[] rot, GameObject go)
    {
        clips_pos = pos;
        clips_rot = rot;
        m_Go = go;
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        base.PrepareFrame(playable, info);
        if (clips_pos != null && clips_pos.Length >= 3)
        {
            float time = (float)(playable.GetTime());

            if (m_Go)
            {
                float x = clips_pos[0].Evaluate(time);
                float y = clips_pos[1].Evaluate(time);
                float z = clips_pos[2].Evaluate(time);
                Vector3 pos = new Vector3(x, y, z);

                x = clips_rot[0].Evaluate(time);
                y = clips_rot[1].Evaluate(time);
                z = clips_rot[2].Evaluate(time);
                Quaternion rot = Quaternion.Euler(x, y, z);
                m_Go.transform.localPosition = pos;
                m_Go.transform.localRotation = rot;
            }
        }
    }

}
