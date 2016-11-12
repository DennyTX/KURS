using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OLDD_camera.Camera
{
    class DockingCamera:BaseKspCamera
    {
        private static HashSet<int> usedId = new HashSet<int>();

        public float MaxSpeed = 2;

        private static List<Texture2D>[] _textureWhiteNoise;
        private static GUIStyle _guiStyleRedLabel;
        private static GUIStyle _guiStyleGreenLabel;

        private string _lastVesselName;
        private string _windowLabelSuffix;

        private static float _currentY = 64;

        private int _id;
        private int _idTextureNoise; 

        private Texture2D _textureVLineOldd;
        private Texture2D _textureHLineOldd; 
        private Texture2D _textureVLine;
        private Texture2D _textureHLine;
        private Texture2D _textureVLineBack;
        private Texture2D _textureHLineBack;

        private GameObject _moduleDockingNodeGameObject; 
        private TargetHelper _target;

        private bool _noiseActive;
        private bool _cameraData = true;
        private bool _rotatorState = true;
        private bool _targetCrossOLDD = false;
        private bool _targetCrossDPAI = true;

        private Color _targetCrossColorOLDD = new Color(0, 0, 0.9f, 1);
        private Color _targetCrossColor = new Color(0.5f, .0f, 0, 1);
        private Color _targetCrossColorBack = new Color(.9f, 0, 0, 1);

        public Color TargetCrossColorOLDD
        {
            get { return _targetCrossColorOLDD; }
            set
            {
                _targetCrossColorOLDD = value;
                _textureVLineOldd = Util.MonoColorVerticalLineTexture(_targetCrossColorOLDD, (int)windowSize * windowSizeCoef);
                _textureHLineOldd = Util.MonoColorHorizontalLineTexture(_targetCrossColorOLDD, (int)windowSize * windowSizeCoef);
            }
        }

        public Color TargetCrossColor
        {
            get { return _targetCrossColor; }
            set
            {
                _targetCrossColor = value;
                _textureVLine = Util.MonoColorVerticalLineTexture(TargetCrossColor, (int)windowSize * windowSizeCoef);
                _textureHLine = Util.MonoColorHorizontalLineTexture(TargetCrossColor, (int)windowSize * windowSizeCoef);
            }
        }

        public Color TargetCrossColorBack
        {
            get { return _targetCrossColorBack; }
            set
            {
                _targetCrossColorBack = value;
                _textureVLineBack = Util.MonoColorVerticalLineTexture(TargetCrossColorBack, (int)windowSize * windowSizeCoef);
                _textureHLineBack = Util.MonoColorHorizontalLineTexture(TargetCrossColorBack, (int)windowSize * windowSizeCoef);
            }
        }

        public DockingCamera(Part part, bool noiseActive, int windowSize, string windowLabel = "DockCam")
            : base(part, windowSize, windowLabel)
        {
            _noiseActive = noiseActive;
            _target = new TargetHelper(part);
            _guiStyleRedLabel = new GUIStyle(HighLogic.Skin.label);
            _guiStyleRedLabel.normal.textColor = Color.red;
            _guiStyleGreenLabel = new GUIStyle(HighLogic.Skin.label);
            _guiStyleGreenLabel.normal.textColor = Color.green;

            //GameEvents.onVesselChange.Add(vesselChange);

            _moduleDockingNodeGameObject = partGameObject.GetChild("dockingNode") ?? partGameObject;  //GET orientation from dockingnode

            if (_textureWhiteNoise != null || !noiseActive) return;
            _textureWhiteNoise = new List<Texture2D>[3];
            for (int j = 0; j < 3; j++)
            {
                _textureWhiteNoise[j] = new List<Texture2D>();
                for (int i = 0; i < 4; i++)
                    _textureWhiteNoise[j].Add(Util.WhiteNoiseTexture((int)texturePosition.width, (int)texturePosition.height));
            }
        }

        //~DockingCamera()  //desctruction
        //{
        //    GameEvents.onVesselChange.Remove(vesselChange);
        //}

        //private void vesselChange(Vessel vessel)
        //{
        //    //if (TargetHelper.IsTargetSelect)
        //    //    windowLabelSuffix = TargetHelper.Target.GetName();
        //    //else
        //    //    windowLabelSuffix = " NO TARGET";
        //    windowLabel = subWindowLabel + " " + ID + " to " + lastVesselName;  //TargetHelper.Target.GetName(); //
        //}

        protected override void InitTextures()
        {
            base.InitTextures();
            _textureVLineOldd = Util.MonoColorVerticalLineTexture(TargetCrossColorOLDD, (int)windowSize * windowSizeCoef);
            _textureHLineOldd = Util.MonoColorHorizontalLineTexture(TargetCrossColorOLDD, (int)windowSize * windowSizeCoef); 
            _textureVLine = Util.MonoColorVerticalLineTexture(TargetCrossColor, (int)windowSize * windowSizeCoef);
            _textureHLine = Util.MonoColorHorizontalLineTexture(TargetCrossColor, (int)windowSize * windowSizeCoef);
            _textureVLineBack = Util.MonoColorVerticalLineTexture(_targetCrossColorBack, (int)windowSize * windowSizeCoef);
            _textureHLineBack = Util.MonoColorHorizontalLineTexture(_targetCrossColorBack, (int)windowSize * windowSizeCoef);
        }

        protected override void ExtendedDrawWindowL1()
        {
            var widthOffset = windowPosition.width - 92;
            _cameraData = GUI.Toggle(new Rect(widthOffset, 34, 88, 20), _cameraData, "Flight data");
            _rotatorState = GUI.Toggle(new Rect(widthOffset, 54, 88, 20), _rotatorState, "Rotator");
            _targetCrossDPAI = GUI.Toggle(new Rect(widthOffset, 74, 88, 20), _targetCrossDPAI, "Cross DPAI");
            _targetCrossOLDD = GUI.Toggle(new Rect(widthOffset, 94, 88, 20), _targetCrossOLDD, "Cross OLDD");
            _noiseActive = GUI.Toggle(new Rect(widthOffset, 253, 88, 20), _noiseActive, "Noise");
            base.ExtendedDrawWindowL1();
        }

        protected override void ExtendedDrawWindowL2()
        {
            GUI.DrawTexture(texturePosition, AssetLoader.texDockingCam);
            if (_noiseActive)
                GUI.DrawTexture(texturePosition, _textureWhiteNoise[windowSizeCoef-2][_idTextureNoise]);  //whitenoise
            base.ExtendedDrawWindowL2();
        }

        protected override void ExtendedDrawWindowL3()
        {
            if (GUI.RepeatButton(new Rect(7, 33, 20, 20), "-"))
            {
                currentZoom += 0.5f;
                if (currentZoom > maxZoom)
                    currentZoom = maxZoom;

            }
            if (GUI.RepeatButton(new Rect(26, 33, 20, 20), "+"))
            {
                currentZoom -= 0.5f;
                if (currentZoom < minZoom)
                    currentZoom = minZoom;
            }

            //LAMP&Seconds Block
            if (_target.isMoveToTarget)
            {
                GUI.DrawTexture(new Rect(texturePosition.xMin + 20, texturePosition.yMax - 20, 20, 20),
                    AssetLoader.texLampOn);
                GUI.Label(new Rect(texturePosition.xMin + 40, texturePosition.yMax - 20, 140, 20),
                    String.Format("Time to dock:{0:f0}s", _target.SecondsToDock));
            }
            else
                GUI.DrawTexture(new Rect(texturePosition.xMin + 20, texturePosition.yMax - 20, 20, 20),
                    AssetLoader.texLampOff);

            GetWindowLabel();
            GetFlightData();
            GetCross();

            if (_rotatorState) // && TargetHelper.IsTargetSelect && part.vessel.Equals(FlightGlobals.ActiveVessel))
            {
                var size = texturePosition.width / 8  ;
                var x = texturePosition.xMin + texturePosition.width / 2 - size / 2;
                GUI.DrawTexture(new Rect(x, texturePosition.yMax - size, size, size), AssetLoader.texSelfRot);
                Matrix4x4 matrixBackup = GUI.matrix;
                var position = new Rect(x, texturePosition.yMax - size, size, size);
                GUIUtility.RotateAroundPivot(_target.AngleZ, position.center);
                GUI.DrawTexture(position, AssetLoader.texTargetRot);
                GUI.matrix = matrixBackup;
            }

            base.ExtendedDrawWindowL3();
        }

        private void GetWindowLabel()
        {
            if (part.vessel.Equals(FlightGlobals.ActiveVessel))
                if (TargetHelper.IsTargetSelect) // && part.vessel.Equals(FlightGlobals.ActiveVessel))
                {
                    _lastVesselName = TargetHelper.Target.GetName();
                    _windowLabelSuffix = " to " + _lastVesselName;
                    windowLabel = subWindowLabel + " " + _id + _windowLabelSuffix;
                }
                else
                {
                    if (part.vessel.Equals(FlightGlobals.ActiveVessel))
                    {
                        windowLabel = subWindowLabel + " " + _id;
                        _lastVesselName = "";
                        _windowLabelSuffix = _lastVesselName;
                    }
                }
            else
            {
                windowLabel = subWindowLabel + " " + _id + _windowLabelSuffix;    
            }
            //if (!part.vessel.Equals(FlightGlobals.ActiveVessel))  //autoaim?
            //    windowLabel = subWindowLabel + " " + ID + windowLabelSuffix;  
           
        }

        private void GetCross()
        {
            if (_targetCrossDPAI)
            {
                ////RotationXY Block
                var textV = _target.LookForward ? _textureVLine : _textureVLineBack;
                var textH = _target.LookForward ? _textureHLine : _textureHLineBack;
                var tx = _target.TargetMoveHelpX;
                var ty = _target.TargetMoveHelpY;
                if (!_target.LookForward)
                {
                    tx = 1 - tx;
                    ty = 1 - ty;
                }
                GUI.DrawTexture(new Rect(texturePosition.xMin + Math.Abs(tx*texturePosition.width)%texturePosition.width,
                        texturePosition.yMin, 1, texturePosition.height), textV);
                GUI.DrawTexture(new Rect(texturePosition.xMin, texturePosition.yMin + Math.Abs(ty*texturePosition.height)%texturePosition.height,
                        texturePosition.width, 1), textH);
            }

            if (_targetCrossOLDD)
            {
                ////RotationXY Block
                var tx = texturePosition.width/2;
                var ty = texturePosition.height/2;
                if (Mathf.Abs(_target.AngleX) > 20)
                    tx += (_target.AngleX > 0 ? -1 : 1)*(texturePosition.width/2 - 1);
                else
                    tx += (texturePosition.width/40)*-_target.AngleX;
                if (Mathf.Abs(_target.AngleY) > 20)
                    ty += (_target.AngleY > 0 ? -1 : 1)*(texturePosition.height/2 - 1);
                else
                    ty += (texturePosition.height/40)*-_target.AngleY;

                GUI.DrawTexture(
                    new Rect(texturePosition.xMin + tx, texturePosition.yMin, 1, texturePosition.height),
                    _textureVLineOldd);
                GUI.DrawTexture(
                    new Rect(texturePosition.xMin, texturePosition.yMin + ty, texturePosition.width, 1),
                    _textureHLineOldd);
            }
        }

        private void GetFlightData()
        {
            if (_cameraData)
            {
                if (TargetHelper.IsTargetSelect && part.vessel.Equals(FlightGlobals.ActiveVessel))
                {
                    /// DATA block
                    /// <summary>
                    float i = 0;
                    _target.Update();

                    if (!_target.isDockPort)
                    {
                        GUI.Label(new Rect(texturePosition.xMin + 10, 54, 100, 40),
                            "Target is not\n a DockPort");
                        if (_target.Destination < 200f)
                            GUI.Label(new Rect(texturePosition.xMin + 10, 84, 96, 40),
                                "DockPort is\n available", _guiStyleGreenLabel);
                    }

                    /// <summary>
                    /// FlightDATA
                    /// <summary>
                    GUI.Label(new Rect(texturePosition.xMax - 70, 32 + i++*20, 70, 20),
                        String.Format("Dist:{0:f2}", _target.Destination));
                    i += .2f;

                    GUI.Label(new Rect(texturePosition.xMax - 70, 32 + i++*20, 70, 20),
                        String.Format("dx:{0:f2}", _target.DX));
                    GUI.Label(new Rect(texturePosition.xMax - 70, 32 + i++*20, 70, 20),
                        String.Format("dy:{0:f2}", _target.DY));
                    GUI.Label(new Rect(texturePosition.xMax - 70, 32 + i++*20, 70, 20),
                        String.Format("dz:{0:f2}", _target.DZ));
                    i += .2f;

                    GUI.Label(new Rect(texturePosition.xMax - 70, 32 + i++*20, 70, 20),
                        String.Format("vX:{0:f2}", _target.SpeedX));
                    GUI.Label(new Rect(texturePosition.xMax - 70, 32 + i++*20, 70, 20),
                        String.Format("vY:{0:f2}", _target.SpeedY));
                    if (_target.SpeedZ < -MaxSpeed)
                        GUI.Label(new Rect(texturePosition.xMax - 70, 32 + i++*20, 70, 20),
                            String.Format("vZ:{0:f2}", _target.SpeedZ), _guiStyleRedLabel);
                    else
                        GUI.Label(new Rect(texturePosition.xMax - 70, 32 + i++*20, 70, 20),
                            String.Format("vZ:{0:f2}", _target.SpeedZ));
                    i += .2f;

                    GUI.Label(new Rect(texturePosition.xMax - 70, 32 + i++*20, 70, 20),
                        String.Format("Yaw:{0:f0}°", _target.AngleX));
                    GUI.Label(new Rect(texturePosition.xMax - 70, 32 + i++*20, 70, 20),
                        String.Format("Pitch:{0:f0}°", _target.AngleY));
                    GUI.Label(new Rect(texturePosition.xMax - 70, 32 + i++*20, 70, 20),
                        String.Format("Roll:{0:f0}°", _target.AngleZ));
                }
            }
        }

        public override void Activate()
        {
            if (IsActivated) return;
            SetFreeId();
            windowPosition.y = _currentY;
            _currentY = windowPosition.y+windowPosition.height;
            base.Activate();
        }

        public override void Deactivate()
        {
            if (!IsActivated) return;
            _currentY = windowPosition.y;
            windowPosition.y = _currentY - windowPosition.height;
            usedId.Remove(_id);
            base.Deactivate();
        }

        public void UpdateNoise() //whitenoise
        {
            _idTextureNoise++;
            if (_idTextureNoise >= 4)
                _idTextureNoise = 0;
        }

        private void SetFreeId()
        {
            for (int i = 1; i < int.MaxValue; i++)
            {
                if (!usedId.Contains(i))
                {
                    _id = i;
                    //lastVesselName = TargetHelper.Target.GetName();
                    //windowLabel = subWindowLabel + " " + ID + " to " + lastVesselName;
                    usedId.Add(i);
                    return;
                }
            }
        }

        public override void Update()
        {
            UpdateWhiteNoise();
            
            allCamerasGameObject.Last().transform.position = _moduleDockingNodeGameObject.transform.position; // near&&far
            allCamerasGameObject.Last().transform.rotation = _moduleDockingNodeGameObject.transform.rotation;
            
            allCamerasGameObject[0].transform.rotation = allCamerasGameObject.Last().transform.rotation; // skybox galaxy
            allCamerasGameObject[1].transform.rotation = allCamerasGameObject.Last().transform.rotation; // nature object
            allCamerasGameObject[2].transform.rotation = allCamerasGameObject.Last().transform.rotation; // middle 
            allCamerasGameObject[2].transform.position = allCamerasGameObject.Last().transform.position;
            allCameras.ForEach(cam => cam.fieldOfView = currentZoom);
        }
    }
}
