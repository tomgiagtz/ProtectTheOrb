﻿/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext
{
    public static partial class Prefs
    {
        public static bool waila = true;
        public static bool wailaShowNameUnderCursor = true;
        public static bool wailaShowAllNamesUnderCursor = true;
        public static bool wailaSmartSelection = true;

#if !UNITY_EDITOR_OSX
        public static EventModifiers wailaShowNameUnderCursorModifiers = EventModifiers.Control;
        public static EventModifiers wailaShowAllNamesUnderCursorModifiers = EventModifiers.Control | EventModifiers.Shift;
        public static KeyCode wailaSmartSelectionKeyCode = KeyCode.Space;
        public static EventModifiers wailaSmartSelectionModifiers = EventModifiers.Control | EventModifiers.Shift;
#else
        public static EventModifiers wailaShowNameUnderCursorModifiers = EventModifiers.Command;
        public static EventModifiers wailaShowAllNamesUnderCursorModifiers = EventModifiers.Command | EventModifiers.Shift;
        public static KeyCode wailaSmartSelectionKeyCode = KeyCode.Space;
        public static EventModifiers wailaSmartSelectionModifiers = EventModifiers.Command | EventModifiers.Shift;
#endif

        private class WailaManager : PrefManager, IHasShortcutPref
        {
#if !UCONTEXT_PRO
            private const string showAllLabel = "Show All Names Under Cursor (PRO)";
            private const string smartSelectionLabel = "Smart Selection (PRO)";
#else
            private const string showAllLabel = "Show All Names Under Cursor";
            private const string smartSelectionLabel = "Smart Selection";
#endif

            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Show All Names Under Cursor",
                        "Smart Selection",
                        "Waila (What Am I Looking At)",
                        "Show Name Under Cursor"
                    };
                }
            }

            public override float order
            {
                get { return -50; }
            }

            public override void Draw()
            {
                waila = EditorGUILayout.ToggleLeft("Waila (What Am I Looking At)", waila, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                EditorGUI.BeginDisabledGroup(!waila);

                DrawFieldWithModifiers("Show Name Under Cursor", ref wailaShowNameUnderCursor, ref wailaShowNameUnderCursorModifiers, labelWidth + 40);
                DrawFieldWithModifiers(showAllLabel, ref wailaShowAllNamesUnderCursor, ref wailaShowAllNamesUnderCursorModifiers, labelWidth + 40);
                DrawFieldWithHotKey(smartSelectionLabel, ref wailaSmartSelection, ref wailaSmartSelectionKeyCode, ref wailaSmartSelectionModifiers);

                EditorGUI.indentLevel--;

                EditorGUI.EndDisabledGroup();
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!waila) return new Shortcut[0];

                List<Shortcut> shortcuts = new List<Shortcut>();

                if (wailaShowNameUnderCursor)
                {
                    shortcuts.Add(new Shortcut("Show Name Of GameObject Under Cursor", "Scene View", wailaShowNameUnderCursorModifiers));
                }

                if (wailaShowAllNamesUnderCursor)
                {
                    shortcuts.Add(new Shortcut("Show Names Of All GameObject Under Cursor", "Scene View", wailaShowAllNamesUnderCursorModifiers));
                }

                if (wailaSmartSelection)
                {
                    shortcuts.Add(new Shortcut("Start Smart Selection", "Scene View", wailaSmartSelectionModifiers, wailaSmartSelectionKeyCode));
                }

                return shortcuts;
            }
        }
    }
}