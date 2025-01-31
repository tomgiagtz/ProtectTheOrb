﻿/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using InfinityCode.uContext;
using InfinityCode.uContext.PopupWindows;
using InfinityCode.uContext.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContextPro.PopupWindows
{
    public class FavoriteWindows : PopupWindowItem, IValidatableLayoutItem
    {
        private GUIContent[] contents;
        private List<FavoriteWindowItem> records;
        private GUIContent labelContent;
        private Vector2 labelSize;
        private GUIContent editFavoritesContent;

        public override float order
        {
            get { return 95; }
        }

        protected override void CalcSize()
        {
            labelSize = EditorStyles.whiteLabel.CalcSize(labelContent);
            Vector2 editSize = EditorStyles.whiteLabel.CalcSize(editFavoritesContent);
            _size = labelSize;
            _size.x += editSize.x;
            _size.y = Mathf.Max(labelSize.y, editSize.y);
            _size.y += GUI.skin.label.margin.bottom;

            GUIStyle style = Styles.buttonWithToggleAlignLeft;
            int marginBottom = style.margin.bottom;

            for (int i = 0; i < records.Count; i++)
            {
                Vector2 s = style.CalcSize(contents[i]);
                _size.x = Mathf.Max(_size.x, s.x);
                _size.y += s.y + marginBottom;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            contents = null;
            contents = null;
            labelContent = null;
        }

        public override void Draw()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(labelContent, EditorStyles.whiteLabel, GUILayout.Width(labelSize.x));
            if (GUILayout.Button(editFavoritesContent, EditorStyles.whiteLabel, GUILayout.ExpandWidth(false)))
            {
                Settings.OpenFavoriteWindowsSettings();
                uContextMenu.Close();
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < records.Count; i++)
            {
                GUIContent content = contents[i];

                if (GUILayout.Button(content, Styles.buttonWithToggleAlignLeft))
                {
                    records[i].Open();
                    uContextMenu.Close();
                }
            }
        }

        protected override void Init()
        {
            records = ReferenceManager.favoriteWindows;
            contents = new GUIContent[records.Count];
            for (int i = 0; i < records.Count; i++)
            {
                contents[i] = new GUIContent(records[i].title);
            }

            labelContent = new GUIContent("Favorite Windows:");
            editFavoritesContent = EditorGUIUtility.IconContent("d_Preset.Context");
            editFavoritesContent.tooltip = "Edit Favorite Windows";
        }

        public bool Validate()
        {
            return Prefs.favoriteWindowsInContextMenu && ReferenceManager.favoriteWindows.Count > 0;
        }
    }
}