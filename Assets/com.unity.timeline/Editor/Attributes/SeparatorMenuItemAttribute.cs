using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
    [Flags]
    enum SeparatorMenuItemPosition
    {
        None = 0,
        Before = 1,
        After = 2
    }

    [AttributeUsage(AttributeTargets.All)]
    class SeparatorMenuItemAttribute : Attribute
    {
        public SeparatorMenuItemPosition position;

        public bool before
        {
            get { return (position & SeparatorMenuItemPosition.Before) == SeparatorMenuItemPosition.Before; }
        }

        public bool after
        {
            get { return (position & SeparatorMenuItemPosition.After) == SeparatorMenuItemPosition.After; }
        }

        public SeparatorMenuItemAttribute(SeparatorMenuItemPosition position)
        {
            this.position = position;
        }

        public SeparatorMenuItemAttribute()
        {
            position = SeparatorMenuItemPosition.None;
        }
    }
}
