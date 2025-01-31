﻿/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext
{
    public class GenericMenuItem
    {
        public float order;
        public GUIContent content;
        public bool on;
        public GenericMenu.MenuFunction action;
        public GenericMenu.MenuFunction2 action2;
        public object userdata;
        public bool separator;
        public string path;
        public bool disabled;
    }
}