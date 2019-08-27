using System;
using UnityEngine;

namespace UnityEngine.Timeline
{


    [AttributeUsage(AttributeTargets.Class)]
    public class SubTrackAttribute : Attribute
    {
        public bool onlyInSub;


        public SubTrackAttribute(bool onlySub)
        {
            onlyInSub = onlySub;
        }

    }
}
