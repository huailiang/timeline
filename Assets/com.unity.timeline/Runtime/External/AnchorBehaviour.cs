using UnityEngine;
using UnityEngine.Playables;

public class AnchorBehaviour : PlayableBehaviour
{

    AnimationCurve[] clips_pos;
    AnimationCurve[] clips_rot;
    Transform m_Transf;
    PlayableDirector m_Director;

    public void Set(AnimationCurve[] pos, AnimationCurve[] rot, Transform tf, PlayableDirector dir)
    {
        clips_pos = pos;
        clips_rot = rot;
        m_Transf = tf;
        m_Director = dir;
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        base.PrepareFrame(playable, info);
        if (clips_pos != null && clips_pos.Length >= 3)
        {
            float time = (float)(m_Director.time);
            if (m_Transf)
            {
                float x = clips_pos[0].Evaluate(time);
                float y = clips_pos[1].Evaluate(time);
                float z = clips_pos[2].Evaluate(time);
                Vector3 pos = new Vector3(x, y, z);
                x = clips_rot[0].Evaluate(time);
                y = clips_rot[1].Evaluate(time);
                z = clips_rot[2].Evaluate(time);
                Quaternion rot = Quaternion.Euler(x, y, z);
                m_Transf.localPosition = pos;
                m_Transf.localRotation = rot;
            }
        }
    }

}
