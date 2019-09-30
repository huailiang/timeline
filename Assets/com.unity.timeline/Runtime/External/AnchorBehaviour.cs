using UnityEngine.Playables;


namespace UnityEngine.Timeline
{
    public class AnchorBehaviour : XPlayableBehaviour
    {

        AnimationCurve[] clips_pos;
        AnimationCurve[] clips_rot;
        Transform m_Transf;


        protected override void OnInitial()
        {
            base.OnInitial();
            AnchorAsset aa = asset as AnchorAsset;
            clips_pos = aa.clip_pos;
            clips_rot = aa.clip_rot;
            if (bindObj)
            {
                m_Transf = bindObj.transform;
            }
            debugY = 0;
        }

        float debugY = 0;
        public override void PrepareFrame(Playable playable, FrameData info)
        {
            base.PrepareFrame(playable, info);
            var direcor = DirectorSystem.Director;
            if (clips_pos != null && direcor != null)
            {
                float time = (float)(direcor.time);

                if (m_Transf)
                {
                    float x = clips_pos[0].Evaluate(time);
                    float y = clips_pos[1].Evaluate(time);
                    float z = clips_pos[2].Evaluate(time);
                    Vector3 pos = new Vector3(x, y, z);
                    if (y > debugY)
                    {
                        debugY = y;
                        Debug.Log(string.Format("debug {0:.00} time:{1:.00}", debugY, time));
                    }
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

}