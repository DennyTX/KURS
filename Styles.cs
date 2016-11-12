using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OLDD_camera
{
    public static class Styles
    {
        static Styles()
        {
            GUIStyleLabelBold = new GUIStyle { fontSize = 13, fontStyle = FontStyle.Bold };
            GUIStyleGreenLabelSmall = new GUIStyle { fontSize = 11, normal = new GUIStyleState() { textColor = Color.green } };
            GUIStyleGreenLabelStandart = new GUIStyle(GUIStyleGreenLabelSmall) { fontSize = 13 };
            GUIStyleGreenLabelBold = new GUIStyle(GUIStyleGreenLabelSmall) { fontSize = 15, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            GUIStyleRedLabelBoldLarge = new GUIStyle("label") { fontSize = 25, fontStyle = FontStyle.Bold, normal = new GUIStyleState() { textColor = Color.red }, alignment = TextAnchor.MiddleCenter };
            GUIStyleRedLabel = new GUIStyle(HighLogic.Skin.label) { normal = new GUIStyleState() { textColor = Color.red } };
            GUIStyleGreenLabel = new GUIStyle(HighLogic.Skin.label) { normal = new GUIStyleState() { textColor = Color.green } };
        }

        public static GUIStyle GUIStyleLabelBold { get; private set; }
        public static GUIStyle GUIStyleGreenLabelSmall { get; private set; }
        public static GUIStyle GUIStyleGreenLabelStandart { get; private set; }
        public static GUIStyle GUIStyleGreenLabelBold { get; private set; }
        public static GUIStyle GUIStyleRedLabelBoldLarge { get; private set; }
        public static GUIStyle GUIStyleRedLabel { get; private set; }
        public static GUIStyle GUIStyleGreenLabel { get; private set; }
    }
}
