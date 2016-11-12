﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OLDD_camera.Camera
{
    public abstract class BaseCamera
    {
        protected static int windowCount = 0;
        //protected static double ElectricChargeAmount;
        public static Material CurrentShader;
        protected UpdateGUIObject updateGUIObject;

        //internal static GUIStyle guiStyleGreenLabelSmall;
        //internal static GUIStyle guiStyleGreenLabelStandart;
        //internal static GUIStyle guiStyleGreenLabelBold;
        //internal static GUIStyle guiStyleLabelBold;
        //internal static GUIStyle guiStyleRedLabelBoldLarge;

        internal Rect windowPosition;
        internal Rect texturePosition;

        protected string windowLabel;
        protected string subWindowLabel; 
        protected GameObject partGameObject;
        protected Part part;

        protected Texture textureBackGroundCamera;
        protected Texture textureSeparator;
        protected Texture textureTargetMark;
        internal Texture[] textureNoSignal;
        internal int textureNoSignalId;
        protected RenderTexture renderTexture; 

        protected ShaderType shaderType;
        protected ShaderType1 shaderType1;
        protected ShaderType2 shaderType2;
        public static string CurrentShaderName;
        internal static int shadersToUse = 0;
        
        protected float windowSize = 128f;
        protected float windowAddition = 92f; 
        protected float rotateX = 0;
        protected float rotateY = 0;
        protected float rotateZ = 0;
        
        protected int minZoomMultiplier = 4; 
        internal float minZoom = 1f;
        internal float maxZoom = 32f;
        internal float currentZoom = 32f;
        internal int calculatedZoom;
        internal bool zoomMultiplier = false; 
        
        protected bool isTargetPoint = false;

        protected int windowSizeCoef = 2;
        protected int windowId = UnityEngine.Random.Range(1000, 10000);

        internal bool IsActivated = false;
        internal bool IsButtonOff = false;
        internal bool IsOrbital = false; 
        internal bool IsAuxiliaryWindowOpen = false;
        internal bool IsAuxiliaryWindowButtonPres = false;

        protected List<UnityEngine.Camera> AllCameras = new List<UnityEngine.Camera>();
        protected List<GameObject> AllCamerasGameObject = new List<GameObject>();
        protected List<string> CameraNames = new List<string>{"GalaxyCamera", "Camera ScaledSpace", "Camera 01", "Camera 00" };
        
        protected BaseCamera(Part part, int windowSizeIni, string windowLabelIni = "Camera")
        {
            windowSize = (float)windowSizeIni/2;
            this.part = part;
            subWindowLabel = windowLabel;
            windowLabel = windowLabelIni;
            partGameObject = part.gameObject;

            //InitWindow();
            InitTextures();

            GameEvents.OnFlightUIModeChanged.Add(FlightUIModeChanged);

            GameObject updateGUIHolder = new GameObject();
            updateGUIObject = updateGUIHolder.AddComponent<UpdateGUIObject>();
            //updateGUIHolder.transform.parent = part.transform;
        }

        ~BaseCamera()
        {
            GameEvents.OnFlightUIModeChanged.Remove(FlightUIModeChanged);
        }

        private void FlightUIModeChanged(FlightUIMode mode)
        {
            if (mode == FlightUIMode.ORBITAL)
                IsOrbital = true;
            else
                IsOrbital = false;
        }
        
        /// <summary>
        /// Initializes window
        /// </summary>
        //protected virtual void InitWindow()
        //{
        //    //windowId = UnityEngine.Random.Range(1000, 10000);
        //    windowPosition.width = windowSize * windowSizeCoef;
        //    windowPosition.height = windowSize * windowSizeCoef + 34f; 
        //}

        /// <summary>
        /// Initializes texture
        /// </summary>
        protected virtual void InitTextures()
        {
            texturePosition = new Rect(6, 34, windowPosition.width - 12f, windowPosition.height - 40f); //42f);
            renderTexture = new RenderTexture((int)windowSize * 4, (int)windowSize * 4, 24, RenderTextureFormat.RGB565);  
            RenderTexture.active = renderTexture;
            renderTexture.Create();
            textureBackGroundCamera = Util.MonoColorRectTexture(new Color(0.45f, 0.45f, 0.45f, 1));
            textureSeparator = Util.MonoColorVerticalLineTexture(Color.white, (int)texturePosition.height);
            textureTargetMark = AssetLoader.texTargetPoint;
            textureNoSignal = new Texture[8];
            for (int i = 0; i < textureNoSignal.Length; i++)
            {
                textureNoSignal[i] = Util.WhiteNoiseTexture((int) texturePosition.width, (int) texturePosition.height, 1f); 
            }
        }

        /// <summary>
        /// Initializes camera
        /// </summary>
        protected virtual void InitCameras()
        {
            AllCamerasGameObject = CameraNames.Select(a => new GameObject()).ToList();
            AllCameras = AllCamerasGameObject.Select((go, i) =>
                {
                    var camera = go.AddComponent<UnityEngine.Camera>();
                    var cameraExample = UnityEngine.Camera.allCameras.FirstOrDefault(cam => cam.name == CameraNames[i]);
                    if (cameraExample != null)
                    {
                        camera.CopyFrom(cameraExample);
                        camera.name = string.Format("{1} copy of {0}", CameraNames[i], windowCount);
                        camera.targetTexture = renderTexture;
                    }
                    return camera;
                }).ToList();
        }

        /// <summary>
        /// Destroy Cameras
        /// </summary>
        protected virtual void DestroyCameras()
        {
            AllCameras.ForEach(UnityEngine.Camera.Destroy);
            AllCameras.Clear();// = new List<UnityEngine.Camera>();
        }

        /// <summary>
        /// Create and activate cameras
        /// </summary>
        public virtual void Activate()
        {
            if (IsActivated) return;
            windowCount++;
            InitCameras();
            IsActivated = true;
            updateGUIObject.UpdateGUIFunction += Begin;
        }

        /// <summary>
        /// Destroy  cameras
        /// </summary>
        public virtual void Deactivate()
        {
            if (!IsActivated) return;
            windowCount--;
            DestroyCameras();
            IsActivated = false;
            updateGUIObject.UpdateGUIFunction -= Begin;
        }

        void Begin()
        {
            if (IsActivated)
            {
                windowPosition = GUI.Window(windowId, KSPUtil.ClampRectToScreen(windowPosition), DrawWindow, windowLabel); //draw main window
                int electricityId = PartResourceLibrary.Instance.GetDefinition("ElectricCharge").id;
                double electricChargeAmount = 0f;
                double electricChargeMaxAmount = 0f;
                part.GetConnectedResourceTotals(electricityId, out electricChargeAmount, out electricChargeMaxAmount);
                if (HighLogic.LoadedSceneIsFlight && !FlightDriver.Pause) part.RequestResource(electricityId, 0.02 * TimeWarp.fixedDeltaTime);
            }
		}

#region DRAW LAYERS 

        /// <summary>
        /// drawing method
        /// </summary>
        private void DrawWindow(int id)
        {

            windowPosition.width = windowSize * windowSizeCoef;
            windowPosition.height = windowSize * windowSizeCoef + 34f;

            ExtendedDrawWindowL1();
            ExtendedDrawWindowL2();
            ExtendedDrawWindowL3();
            GUI.DragWindow();
        }

        /// <summary>
        /// drawing method, first layer, for cameras
        /// </summary>
        protected virtual void ExtendedDrawWindowL1()
        {
            //GUI.depth = 10;
            var widthOffset = windowPosition.width - 90;
            var calculateZoom = (int)(maxZoom - currentZoom + minZoom);
            calculatedZoom = !zoomMultiplier ? calculateZoom : calculateZoom * minZoomMultiplier*6;

            GUI.Label(new Rect(widthOffset, 128, 80, 20), "zoom: " + calculatedZoom, Styles.GUIStyleLabelBold); 

            isTargetPoint = GUI.Toggle(new Rect(widthOffset-2, 233, 88, 20), isTargetPoint, "Target Mark");

            //Graphics.DrawTexture(texturePosition, textureBackGroundCamera);

            GUI.DrawTexture(texturePosition, textureBackGroundCamera);

            switch (shadersToUse)
            {
                case 0:
                    CurrentShader = CameraShaders.GetShader(shaderType);
                    break;
                case 1:
                    CurrentShader = CameraShaders.GetShader1(shaderType1);
                    break;
                case 2:
                    CurrentShader = CameraShaders.GetShader2(shaderType2);
                    break;
            }
            CurrentShaderName = CurrentShader == null ? "none" : CurrentShader.name;

            if (Event.current.type.Equals(EventType.Repaint))
            {
                Graphics.DrawTexture(texturePosition, Render(), CurrentShader);
            }
        }

        /// <summary>
        /// drawing method, second layer (draw any texture between cam and interface)
        /// </summary>
        protected virtual void ExtendedDrawWindowL2()
        {
            if (TargetHelper.IsTargetSelect)
            {
                var camera = AllCameras.Last();
                var vessel = TargetHelper.Target as Vessel;

                if (vessel == null)
                {
                    var targetedDockingNode = TargetHelper.Target as ModuleDockingNode;
                    vessel = targetedDockingNode.vessel;
                }

                var point = camera.WorldToViewportPoint(vessel.transform.position); //get current targeted vessel 
                var x = point.x; //(0;1)
                var y = point.y;
                var z = point.z;
 
                x = GetX(x,z);
                y = GetY(y,z);

                var offsetX = texturePosition.width * x;
                var offsetY = texturePosition.height * y;

                if (isTargetPoint)
                {
                    GUI.DrawTexture(new Rect(texturePosition.xMin + offsetX-10, texturePosition.yMax - offsetY-10, 20, 20), textureTargetMark);
                }
            }
            if (IsOrbital)
            {
                GUI.DrawTexture(texturePosition, textureNoSignal[textureNoSignalId]); 
            }
        }

        /// <summary>
        /// drawing method, third layer, interface
        /// </summary>
        protected virtual void ExtendedDrawWindowL3()  
        {
            if (!part.vessel.Equals(FlightGlobals.ActiveVessel))
            {
                GUI.Label(new Rect(30, 30, 222, 22), "Broadcast from: " + part.vessel.vesselName, Styles.GUIStyleGreenLabelStandart);
            }
            if (IsAuxiliaryWindowOpen)
                GUI.DrawTexture(new Rect(texturePosition.width+8, 34, 1, texturePosition.height), textureSeparator);  //vert line, textureSeparator

            if (GUI.Button(new Rect(windowPosition.width - 20, 3, 15, 15), " ")) // destroy cam window
            {
                IsButtonOff = true;
            } 
            if (GUI.Button(new Rect(windowPosition.width - 29, 18, 24, 15), IsAuxiliaryWindowOpen ? "◄" : "►")) //extend window
            {
                IsAuxiliaryWindowOpen = !IsAuxiliaryWindowOpen;
                IsAuxiliaryWindowButtonPres = true;
            }

            var tooltip = new GUIContent("☼", CurrentShaderName);
            GUI.Box(new Rect(8, texturePosition.yMax - 22, 20, 20), tooltip);
            GUI.Label(new Rect(64, 128, 200, 40), GUI.tooltip, Styles.GUIStyleGreenLabelBold);
            if (GUI.Button(new Rect(8, texturePosition.yMax - 22, 20, 20), "☼")) 
            {
                switch (shadersToUse)
                {
                    case 0:
                        shaderType++;
                        if (!Enum.IsDefined(typeof (ShaderType), shaderType))
                            shaderType = ShaderType.OldTV;
                        break;
                    case 1:
                        shaderType1++;
                        if (!Enum.IsDefined(typeof(ShaderType1), shaderType1))
                            shaderType1 = ShaderType1.OldTV;
                        break;
                    case 2:
                        shaderType2++;
                        if (!Enum.IsDefined(typeof (ShaderType2), shaderType2))
                            shaderType2 = ShaderType2.None;
                        break;
                }
            }

            if (GUI.RepeatButton(new Rect(texturePosition.xMax - 22, texturePosition.yMax - 22, 20, 20), "±") && 	
                UnityEngine.Camera.allCameras.FirstOrDefault(x => x.name == "Camera 00") != null) //Size of main window
            {
                switch (windowSizeCoef)
                {
                    case 2:
                        windowSizeCoef = 3;
                        break;
                    case 3:
                        windowSizeCoef = 2; 
                        break;
                }
                Deactivate();
                //InitWindow();
                InitTextures();
                Activate();
                IsAuxiliaryWindowOpen = false;
            }

            currentZoom = GUI.HorizontalSlider(new Rect(texturePosition.width / 2 - 80, 20, 160, 10), currentZoom, maxZoom, minZoom);
        }

#endregion DRAW LAYERS

        private float GetX(float x, float z)
        {
            if (x < 0 && z > 0 && x <= 0) return 0f;
            if (x > 0 && z < 0) return 0f;
            if (x < 0 && z < 0) return 1f;
            if (x > 0 && z > 0 && x >= 1) return 1f;
            return x;
        }
        private float GetY(float y, float z)
        {
            if (z > 0)
            {
                if (y <= 0f) return 0f;
                if (y >= 1f) return 1f;
            }
            if (z < 0)
            {
                if (y <= 0) return 0f;
                if (y >= 1f) return 1f;
            }
            return y;
        }

        /// <summary>
        /// render texture camera
        /// </summary>
        protected virtual RenderTexture Render()
        {
            AllCameras.ForEach(a => a.Render());
            return renderTexture;
        }

        public IEnumerator ResizeWindow()
        {
            IsAuxiliaryWindowButtonPres = false;
            while (true)
            {
                if (IsAuxiliaryWindowOpen && windowPosition.width < windowSize * windowSizeCoef + windowAddition)
                {
                    windowPosition.width += 4;
                    if (windowPosition.width >= windowSize * windowSizeCoef + windowAddition)
                        break;
                }
                else if (windowPosition.width > windowSize*windowSizeCoef)
                {
                    windowPosition.width -= 4;
                    if (windowPosition.width <= windowSize*windowSizeCoef)
                        break;
                }
                else
                    break;
                yield return new WaitForSeconds(1/23);
            }
        }

        protected void UpdateWhiteNoise()
        {
            //if (!IsOrbital) return;
            textureNoSignalId++;
            if (textureNoSignalId >= textureNoSignal.Length)
                textureNoSignalId = 0;
        }

        /// <summary>
        /// Update position and rotation of the cameras
        /// </summary>
        public virtual void Update()
        {
            AllCamerasGameObject.Last().transform.position = partGameObject.transform.position;
            AllCamerasGameObject.Last().transform.rotation = partGameObject.transform.rotation;

            AllCamerasGameObject.Last().transform.Rotate(new Vector3(-1f, 0, 0), 90);
            AllCamerasGameObject.Last().transform.Rotate(new Vector3(0, 1f, 0), rotateY);
            AllCamerasGameObject.Last().transform.Rotate(new Vector3(1f, 0, 0), rotateX);
            AllCamerasGameObject.Last().transform.Rotate(new Vector3(0, 0, 1f), rotateZ);

            AllCamerasGameObject[0].transform.rotation = AllCamerasGameObject.Last().transform.rotation;
            AllCamerasGameObject[1].transform.rotation = AllCamerasGameObject.Last().transform.rotation;
            AllCamerasGameObject[2].transform.rotation = AllCamerasGameObject.Last().transform.rotation;
            AllCamerasGameObject[2].transform.position = AllCamerasGameObject.Last().transform.position;
            AllCameras.ForEach(cam => cam.fieldOfView = currentZoom);
        }
    }
}
