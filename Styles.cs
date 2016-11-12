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
            GUIStyleGreenLabelSmall = new GUIStyle { fontSize = 11, normal = new GUIStyleState { textColor = Color.green } };
            GUIStyleGreenLabelStandart = new GUIStyle(GUIStyleGreenLabelSmall) { fontSize = 13 };
            GUIStyleGreenLabelBold = new GUIStyle(GUIStyleGreenLabelSmall) { fontSize = 15, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            GUIStyleRedLabelBoldLarge = new GUIStyle { fontSize = 25, fontStyle = FontStyle.Bold, normal = new GUIStyleState { textColor = Color.red }, alignment = TextAnchor.MiddleCenter };
            GUIStyleRedLabel = new GUIStyle { normal = new GUIStyleState { textColor = Color.red } };
            GUIStyleGreenLabel = new GUIStyle { normal = new GUIStyleState { textColor = Color.green } };
        }

        public static GUIStyle GUIStyleLabelBold { get; set; }
        public static GUIStyle GUIStyleGreenLabelSmall { get; set; }
        public static GUIStyle GUIStyleGreenLabelStandart { get; set; }
        public static GUIStyle GUIStyleGreenLabelBold { get; set; }
        public static GUIStyle GUIStyleRedLabelBoldLarge { get; set; }
        public static GUIStyle GUIStyleRedLabel { get; set; }
        public static GUIStyle GUIStyleGreenLabel { get; set; }
    }
}
