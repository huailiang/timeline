using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
    abstract class MenuItemBase
    {
        public abstract void AddToMenu(GenericMenu menu, WindowState state);
    }

    class TimelineActionMenuItem : MenuItemBase
    {
        TimelineAction action { get; }

        public TimelineActionMenuItem(TimelineAction action)
        {
            this.action = action;
        }

        public override void AddToMenu(GenericMenu menu, WindowState state)
        {
            var mousePosition = Event.current.mousePosition;
            action.SetMousePosition(mousePosition);
            var displayState = action.GetDisplayState(state);
            if (displayState == MenuActionDisplayState.Visible && !MenuItemActionBase.IsActionActiveInMode(action, TimelineWindow.instance.currentMode.mode))
                displayState = MenuActionDisplayState.Disabled;
            action.ClearMousePosition();

            var content = new GUIContent(MenuItemActionBase.GetDisplayName(action));

            switch (displayState)
            {
                case MenuActionDisplayState.Visible:
                    menu.AddItem(content, action.IsChecked(state), () =>
                    {
                        action.SetMousePosition(mousePosition);
                        action.Execute(state);
                        action.ClearMousePosition();
                    });
                    break;
                case MenuActionDisplayState.Disabled:
                    menu.AddDisabledItem(content);
                    break;
            }
        }
    }
}
