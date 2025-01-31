﻿/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;

namespace InfinityCode.uContext
{
    public static partial class Prefs
    {
        public static bool jumpToPoint = true;
        public static bool highJumpToPoint = true;
        public static bool alternativeJumpShortcut = false;

        private class JumpToPointManager : PrefManager, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Jump To Point", "High"
                    };
                }
            }

            public override float order
            {
                get { return -35; }
            }

            public override void Draw()
            {
                jumpToPoint = EditorGUILayout.ToggleLeft("Jump To Point", jumpToPoint, EditorStyles.boldLabel);
#if UCONTEXT_PRO
                string label = "High Jump To Point";
#else
                string label = "High Jump To Point (PRO)";
#endif
                highJumpToPoint = EditorGUILayout.ToggleLeft(label, highJumpToPoint, EditorStyles.boldLabel);

#if UNITY_EDITOR_OSX
                string alternativeLabel = "Alternative Jump Shortcuts (SHIFT + SHIFT, CMD + SHIFT + SHIFT)";
#else
                string alternativeLabel = "Alternative Jump Shortcuts (SHIFT + SHIFT, CTRL + SHIFT + SHIFT)";
#endif
                alternativeJumpShortcut = EditorGUILayout.ToggleLeft(alternativeLabel, alternativeJumpShortcut, EditorStyles.boldLabel);
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                List<Shortcut> shortcuts = new List<Shortcut>();

                if (jumpToPoint)
                {
                    shortcuts.Add(new Shortcut("Jump To Point", "Scene View", "SHIFT + MMB"));
                    if (alternativeJumpShortcut) shortcuts.Add(new Shortcut("Jump To Point", "Scene View", "SHIFT + SHIFT"));
                }

#if UCONTEXT_PRO
                if (highJumpToPoint)
                {
#if UNITY_EDITOR_OSX
                    string shortcut = "CMD + SHIFT + MMB";
                    string shortcut2 = "CMD + SHIFT + SHIFT";
#else
                    string shortcut = "CTRL + SHIFT + MMB";
                    string shortcut2 = "CTRL + SHIFT + SHIFT";
#endif
                    shortcuts.Add(new Shortcut("High Jump To Point", "Scene View", shortcut));
                    if (alternativeJumpShortcut) shortcuts.Add(new Shortcut("High Jump To Point", "Scene View", shortcut2));
                }
#endif
                    return shortcuts;
            }
        }
    }
}