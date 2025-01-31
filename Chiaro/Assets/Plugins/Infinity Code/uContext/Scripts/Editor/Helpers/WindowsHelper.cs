﻿/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using InfinityCode.uContext.Integration;
using InfinityCode.uContext.UnityTypes;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext
{
    public static class WindowsHelper
    {
        public const string MenuPath = "Window/Infinity Code/uContext/";

        public static void ShowInspector()
        {
            Event e = Event.current;
            Type windowType = InspectorWindowRef.type;

            Vector2 size = Prefs.contextMenuWindowSize;
            Rect rect = new Rect(GUIUtility.GUIToScreenPoint(e.mousePosition) - size / 2, Vector2.zero);

            Rect windowRect = new Rect(rect.position, size);
            EditorWindow window = ScriptableObject.CreateInstance(windowType) as EditorWindow;
            window.Show();
            window.position = windowRect;
        }

        public static bool IsMaximized(EditorWindow window)
        {
            if (window == null) return false;
            if (FullscreenEditor.IsFullscreen(window)) return true;
            return window.maximized;
        }

        public static void SetMaximized(EditorWindow window, bool maximized)
        {
            if (window == null) return;

            bool state = IsMaximized(window);
            if (state == maximized) return;

            if (maximized) window.maximized = true;
            else
            {
                if (FullscreenEditor.IsFullscreen(window)) FullscreenEditor.ToggleFullscreen(window);
                window.maximized = false;
            }
        }

        public static void ToggleMaximized(EditorWindow window)
        {
            SetMaximized(window, !IsMaximized(window));
        }
    }
}