using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
    /// <summary>
    /// Animator to Editor Curve Binding cache. Used to prevent frequent calls to GetAnimatorBindings which can be costly
    /// </summary>
    class AnimatorBindingCache
    {
        struct AnimatorEntry
        {
            public int animatorID;
            public bool applyRootMotion;
            public bool humanoid;
        }

        class AnimatorEntryComparer : IEqualityComparer<AnimatorEntry>
        {
            public bool Equals(AnimatorEntry x, AnimatorEntry y) { return x.animatorID == y.animatorID && x.applyRootMotion == y.applyRootMotion && x.humanoid == y.humanoid; }
            public int GetHashCode(AnimatorEntry obj) { return HashUtility.CombineHash(obj.animatorID, obj.applyRootMotion.GetHashCode(), obj.humanoid.GetHashCode()); }
            public static readonly AnimatorEntryComparer Instance = new AnimatorEntryComparer();
        }

        readonly Dictionary<AnimatorEntry, EditorCurveBinding[]> m_AnimatorCache = new Dictionary<AnimatorEntry, EditorCurveBinding[]>(AnimatorEntryComparer.Instance);
        private static readonly EditorCurveBinding[] kEmptyArray = new EditorCurveBinding[0];

        public EditorCurveBinding[] GetAnimatorBindings(GameObject gameObject)
        {
            if (gameObject == null)
                return kEmptyArray;

            Animator animator = gameObject.GetComponent<Animator>();
            if (animator == null)
                return kEmptyArray;

            AnimatorEntry entry = new AnimatorEntry()
            {
                animatorID = animator.GetInstanceID(),

                applyRootMotion = animator.applyRootMotion,
                humanoid = animator.isHuman
            };

            EditorCurveBinding[] result = null;
            if (m_AnimatorCache.TryGetValue(entry, out result))
                return result;

            result = AnimationUtility.GetAdditionalAnimatorBindings(animator.gameObject);
            m_AnimatorCache[entry] = result;
            return result;
        }

        public void Clear()
        {
            m_AnimatorCache.Clear();
        }
    }
}
