﻿/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using InfinityCode.uContext;
using InfinityCode.uContext.Tools;
using InfinityCode.uContext.UnityTypes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.uContextPro.Tools
{
    [InitializeOnLoad]
    public static class SmartSelection
    {
        private static GUIStyle _areaStyle;
        private static Rect screenRect;

        private static GUIStyle areaStyle
        {
            get
            {
                if (_areaStyle == null)
                {
                    _areaStyle = new GUIStyle(Waila.StyleID);
                    _areaStyle.fontSize = 10;
                    _areaStyle.stretchHeight = true;
                    _areaStyle.fixedHeight = 0;
                    _areaStyle.border = new RectOffset(8, 8, 8, 8);
                    _areaStyle.margin = new RectOffset(4, 4, 4, 4);
                }

                return _areaStyle;
            }
        }

        static SmartSelection()
        {
            Waila.OnClose += OnClose;
            Waila.OnDrawModeExternal += OnDrawModeExternal;
            Waila.OnUpdateTooltipsExternal += OnUpdateTooltipsExternal;
            Waila.OnStartSmartSelection += ShowSmartSelection;
        }

        private static void DrawButton(ref Rect r, Transform t, bool addSlash, ref bool state)
        {
            if (t.parent != null)
            {
                DrawButton(ref r, t.parent, true, ref state);
            }

            Rect r2 = new Rect(r);
            GUIContent content = new GUIContent(t.gameObject.name);
            GUIStyle style = Waila.labelStyle;
            r2.width = style.CalcSize(content).x + style.margin.horizontal;

            r.xMin += r2.width;

            if (GUI.Button(r2, content, style))
            {
                if (Event.current.control || Event.current.shift) SelectionRef.Add(t.gameObject);
                else Selection.activeGameObject = t.gameObject;
                state = true;
            }

            if (r2.Contains(Event.current.mousePosition))
            {
                Waila.Highlight(t.gameObject);
            }

            if (addSlash)
            {
                r2.xMin = r2.xMax;
                content.text = "/";
                r2.width = style.CalcSize(content).x + style.margin.horizontal;
                GUI.Label(r2, content, style);
                r.xMin += r2.width;
            }
        }

        private static void DrawSmartSelection(Event e)
        {
            if (!UnityEditor.Tools.hidden) UnityEditor.Tools.hidden = true;

            EventType type = e.type;

            try
            {
                Handles.BeginGUI();

                if (e.type == EventType.Repaint) areaStyle.Draw(screenRect, GUIContent.none, -1);

                GUIStyle style = Waila.labelStyle;
                RectOffset margin = style.margin;
                RectOffset padding = style.padding;

                Rect r = new Rect(screenRect.x + 5, screenRect.y + margin.top + padding.top, screenRect.width - 10, style.lineHeight + margin.vertical + padding.vertical);

                GUI.Label(r,  "Select GameObject:", style);
                r.y += r.height + margin.bottom;
                r.height = 1;
                EditorGUI.DrawRect(r, new Color(0.5f, 0.5f, 0.5f, 1));

                r.y += 2;
                r.height = style.lineHeight + margin.vertical + padding.vertical;

                try
                {
                    bool state = false;

                    for (int i = 0; i < Waila.targets.Count; i++)
                    {
                        Rect r2 = new Rect(r);
                        r2.y += i * (style.lineHeight + margin.vertical + padding.vertical);
                        Transform t = Waila.targets[i].transform;
                        try
                        {
                            DrawButton(ref r2, t, false, ref state);
                        }
                        catch
                        {
                            
                        }
                    }

                    if (state)
                    {
                        Waila.mode = 0;
                        UnityEditor.Tools.hidden = false;
                    }
                }
                catch(Exception ex)
                {
                    Log.Add(ex);
                }

                if (type == EventType.MouseUp)
                {
                    Waila.mode = 0;
                    UnityEditor.Tools.hidden = false;
                }
                else if (type == EventType.KeyDown)
                {
                    if (e.keyCode != KeyCode.LeftShift && e.keyCode != KeyCode.RightShift && e.keyCode != KeyCode.LeftControl && e.keyCode != KeyCode.RightControl)
                    {
                        Waila.mode = 0;
                        UnityEditor.Tools.hidden = false;
                    }
                }

                Handles.EndGUI();
            }
            catch
            {
            }
        }

        private static void OnClose()
        {
            Waila.Highlight(null);
        }

        private static void OnDrawModeExternal()
        {
            if (Waila.mode != 1) return;

            DrawSmartSelection(Event.current);
        }

        private static bool OnUpdateTooltipsExternal()
        {
            if (Prefs.wailaShowAllNamesUnderCursor && Event.current.modifiers == Prefs.wailaShowAllNamesUnderCursorModifiers)
            {
                UpdateAllTooltips();
                return true;
            }

            return false;
        }

        private static void ShowSmartSelection()
        {
            if (!(EditorWindow.mouseOverWindow is SceneView)) return;
            if (Waila.targets == null || Waila.targets.Count == 0) return;

            GUIStyle style = Waila.labelStyle;
            RectOffset margin = style.margin;
            RectOffset padding = style.padding;

            float width = style.CalcSize(new GUIContent("Select GameObject")).x + margin.horizontal + 10;

            int rightMargin = margin.right;
            Vector2 slashSize = style.CalcSize(new GUIContent("/"));

            float height = margin.top;

            int count = 0;

            try
            {
                for (int i = 0; i < Waila.targets.Count; i++)
                {
                    GameObject go = Waila.targets[i];
                    if (go == null) break;

                    float w = 0;
                    Transform t = go.transform;
                    Vector2 contentSize = style.CalcSize(new GUIContent(t.gameObject.name));
                    w += contentSize.x + margin.horizontal;
                    height += contentSize.y + margin.bottom + padding.bottom;

                    while (t.parent != null)
                    {
                        t = t.parent;
                        w += slashSize.x + rightMargin;
                        contentSize = style.CalcSize(new GUIContent(t.gameObject.name));
                        w += contentSize.x + rightMargin;
                    }

                    w += 5;
                    if (w > width) width = w;

                    count++;
                }
            }
            catch (Exception e)
            {
                Log.Add(e);
            }

            if (count == 0) return;

            Vector2 size = new Vector2(width + 12, height + 32);
            Vector2 position = Event.current.mousePosition - new Vector2(size.x / 2, size.y * 1.5f);

            if (position.x < 5) position.x = 5;
            else if (position.x + size.x > EditorWindow.focusedWindow.position.width - 5) position.x = EditorWindow.focusedWindow.position.width - size.x - 5;

            if (position.y < 5) position.y = 5;
            else if (position.y + size.y > EditorWindow.focusedWindow.position.height - 5) position.y = EditorWindow.focusedWindow.position.height - size.y - 5;

            screenRect = new Rect(position, size);
            Waila.mode = 1;
            Waila.tooltip = null;
        }

        private static void UpdateAllTooltips()
        {
            Waila.tooltip = null;

            int count = 0;

            StaticStringBuilder.Clear();

            Waila.targets.Clear();

            while (count < 20)
            {
                GameObject go = HandleUtility.PickGameObject(Event.current.mousePosition, false, Waila.targets.ToArray());
                if (go == null) break;

                Waila.targets.Add(go);
                if (count > 0) StaticStringBuilder.Append("\n");
                int length = StaticStringBuilder.Length;
                Transform t = go.transform;
                StaticStringBuilder.Append(t.gameObject.name);
                while (t.parent != null)
                {
                    t = t.parent;
                    StaticStringBuilder.Insert(length, " / ");
                    StaticStringBuilder.Insert(length, t.gameObject.name);
                }

                count++;
            }

            if (Waila.targets.Count > 0) Waila.Highlight(Waila.targets[0]);
            else Waila.Highlight(null);

            if (count > 0) Waila.tooltip = new GUIContent(StaticStringBuilder.GetString(true));
        }
    }
}