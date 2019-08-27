using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
    [ActiveInMode(TimelineModes.Default)]
    abstract class ItemAction<T> : MenuItemActionBase where T : class
    {
        public abstract bool Execute(WindowState state, T[] items);

        protected virtual MenuActionDisplayState GetDisplayState(WindowState state, T[] items)
        {
            return items.Length > 0 ? MenuActionDisplayState.Visible : MenuActionDisplayState.Disabled;
        }

        protected virtual string GetDisplayName(WindowState state, T[] items)
        {
            return GetDisplayName(this);
        }

        public bool CanExecute(WindowState state, T[] items)
        {
            return GetDisplayState(state, items) == MenuActionDisplayState.Visible;
        }

        protected virtual void AddSelfToMenu(GenericMenu menu, WindowState state, T[] items)
        {
            var displayState = GetDisplayState(state, items);

            if (displayState == MenuActionDisplayState.Visible && !IsActionActiveInMode(this, TimelineWindow.instance.currentMode.mode))
                displayState = MenuActionDisplayState.Disabled;

            if (displayState == MenuActionDisplayState.Hidden)
                return;

            var subMenuPath = GetSubMenuPath();
            var displayName = this.GetDisplayName(state, items);
            var menuItemName = subMenuPath + displayName;
            var separator = GetSeparator(this);

            if (separator != null && separator.before)
                menu.AddSeparator(subMenuPath);

            if (displayState == MenuActionDisplayState.Visible)
            {
                menu.AddItem(new GUIContent(menuItemName), false, f =>
                {
                    Execute(state, items);
                }, this);
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(menuItemName));
            }

            if (separator != null && separator.after)
                menu.AddSeparator(subMenuPath);
        }

        string GetSubMenuPath()
        {
            string subMenuPath = string.Empty;
            var categoryAttr = GetCategoryAttribute(this);

            if (categoryAttr != null)
            {
                subMenuPath = categoryAttr.Category;
                if (!subMenuPath.EndsWith("/"))
                    subMenuPath += "/";
            }

            return subMenuPath;
        }

        public static bool HandleShortcut(WindowState state, Event evt, T item)
        {
            T[] items = { item };

            foreach (ItemAction<T> action in actions)
            {
                var attr = action.GetType().GetCustomAttributes(typeof(ShortcutAttribute), true);

                foreach (ShortcutAttribute shortcut in attr)
                {
                    if (shortcut.MatchesEvent(evt))
                    {
                        if (s_ShowActionTriggeredByShortcut)
                            Debug.Log(action.GetType().Name);

                        if (!IsActionActiveInMode(action, TimelineWindow.instance.currentMode.mode))
                            return false;

                        var result = action.Execute(state, items);
                        state.Refresh();
                        state.Evaluate();
                        return result;
                    }
                }
            }

            return false;
        }

        static List<ItemAction<T>> s_ActionClasses;

        static List<ItemAction<T>> actions
        {
            get
            {
                if (s_ActionClasses == null)
                {
                    s_ActionClasses = GetActionsOfType(typeof(ItemAction<T>)).Select(x => (ItemAction<T>)x.GetConstructors()[0].Invoke(null)).ToList();
                }

                return s_ActionClasses;
            }
        }

        public static void AddToMenu(GenericMenu menu, WindowState state)
        {
            var items = SelectionManager.SelectedItemOfType<T>().ToArray();

            if (items.Length < 1)
                return;

            var actionsToAdd = actions.Where(i => !TypeUtility.IsHiddenInMenu(i.GetType())).ToList();
            if (actionsToAdd.Any())
                menu.AddSeparator(string.Empty);
            else
                return;

            actionsToAdd.ForEach(action =>
            {
                action.AddSelfToMenu(menu, state, items);
            });
        }

        public static bool Invoke<TAction>(WindowState state, T[] items)
            where TAction : ItemAction<T>
        {
            var itemsDerived = items.ToArray();

            if (!itemsDerived.Any())
                return false;

            var action = actions.FirstOrDefault(x => x.GetType() == typeof(TAction));

            if (action != null)
                return action.Execute(state, itemsDerived);

            return false;
        }

        public static bool Invoke<TAction>(WindowState state, T item)
            where TAction : ItemAction<T>
        {
            return Invoke<TAction>(state, new[] {item});
        }
    }
}
