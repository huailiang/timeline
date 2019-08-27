using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UnityEditor.Timeline
{
    enum MenuActionDisplayState
    {
        Visible,
        Disabled,
        Hidden
    }

    class MenuItemActionBase
    {
        protected static bool s_ShowActionTriggeredByShortcut = false;

        protected static IEnumerable<Type> GetActionsOfType(Type actionType)
        {
            var query = from type in EditorAssemblies.loadedTypes
                where !type.IsGenericType && !type.IsNested && !type.IsAbstract && type.IsSubclassOf(actionType)
                select type;

            return query;
        }

        public static string GetDisplayName(MenuItemActionBase action)
        {
            var shortcutAttribute = GetShortcutAttributeForAction(action);
            var displayNameAttributes = action.GetType().GetCustomAttributes(typeof(DisplayNameAttribute), true).FirstOrDefault() as DisplayNameAttribute;

            var builder = new StringBuilder();

            if (displayNameAttributes != null)
                builder.Append(L10n.Tr(displayNameAttributes.DisplayName));
            else
                builder.Append(L10n.Tr(action.GetType().Name));

            if (shortcutAttribute != null)
            {
                var shortcut = shortcutAttribute.GetMenuShortcut();
                if (shortcut != string.Empty)
                {
                    builder.Append(" ");
                    builder.Append(shortcut);
                }
            }

            return builder.ToString();
        }

        public static string GetShortcutText(ShortcutAttribute attribute)
        {
            if (attribute == null) return string.Empty;
            var shortcut = attribute.GetMenuShortcut();
            if (!string.IsNullOrEmpty(shortcut))
            {
                return " " + shortcut;
            }
            return string.Empty;
        }

        public static ShortcutAttribute GetShortcutAttributeForAction(MenuItemActionBase action)
        {
            var shortcutAttributes = action.GetType()
                .GetCustomAttributes(typeof(ShortcutAttribute), true)
                .Cast<ShortcutAttribute>();

            foreach (var shortcutAttribute in shortcutAttributes)
            {
                var shortcutOverride = shortcutAttribute as ShortcutPlatformOverrideAttribute;
                if (shortcutOverride != null)
                {
                    if (shortcutOverride.MatchesCurrentPlatform())
                        return shortcutOverride;
                }
                else
                {
                    return shortcutAttribute;
                }
            }

            return null;
        }

        protected static CategoryAttribute GetCategoryAttribute(MenuItemActionBase action)
        {
            var attr = action.GetType().GetCustomAttributes(typeof(CategoryAttribute), true);

            if (attr.Length > 0)
                return attr[0] as CategoryAttribute;

            return null;
        }

        protected static SeparatorMenuItemAttribute GetSeparator(MenuItemActionBase action)
        {
            var attr = action.GetType().GetCustomAttributes(typeof(SeparatorMenuItemAttribute), true);

            if (attr.Length > 0)
                return (attr[0] as SeparatorMenuItemAttribute);

            return null;
        }

        public static ActiveInModeAttribute GetActiveInModeAttribute(MenuItemActionBase action)
        {
            var attr = action.GetType().GetCustomAttributes(typeof(ActiveInModeAttribute), true);

            if (attr.Length > 0)
                return (attr[0] as ActiveInModeAttribute);

            return null;
        }

        public static bool IsActionActiveInMode(MenuItemActionBase action, TimelineModes mode)
        {
            ActiveInModeAttribute attr = GetActiveInModeAttribute(action);
            return attr != null && (attr.modes & mode) != 0;
        }
    }
}
