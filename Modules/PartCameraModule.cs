using System;
using System.Collections;
using System.Collections.Generic;
using KspHelper.Events;
using OLDD_camera.Camera;
using UnityEngine;

namespace OLDD_camera.Modules
{
    /// <summary>
    /// Module adds an external camera and gives control over it
    /// </summary>
    [Serializable]
    public class PartCameraModule : PartModule, ICamPart, ISerializationCallbackReceiver
    {
        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Camera powered: ")]
        public string IsPowered;

        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Bullets: ")]
        public string AboutHits;

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Camera", isPersistant = true),
        UI_Toggle(controlEnabled = true, enabledText = "ON", disabledText = "OFF", scene = UI_Scene.All)]
        public bool IsEnabled;

        [KSPField(isPersistant = true)]
        private int currentHits = -1;

        //[KSPField]
        //public string bulletName = "Sphere";

        //[KSPField]
        //public int windowSize = 256;

        //[KSPField]
        //public string cameraName = "CamExt";

        //[KSPField]
        //public string rotatorZ ;

        //[KSPField]
        //public string rotatorY;

        //[KSPField]
        //public string cap;

        //[KSPField]
        //public string zoommer;

        //[KSPField]
        //public float stepper;

        //[KSPField]
        //public int allowedScanDistance;

        //[KSPField]
        //public string resourceScanning;

        private GameObject _capObject;
        private GameObject _camObject;
        private PartCamera _camera;
        private Vector3 _initialUpVector;

        private float _targetOffset = 100;

        //[KSPField(isPersistant = true)]
        //private bool _IsOnboard;
        //[KSPField(isPersistant = true)]
        //private bool _IsLookAtMe;
        //[KSPField(isPersistant = true)]
        //private bool _IsLookAtMeAutoZoom;
        //[KSPField(isPersistant = true)]
        //private bool _IsFollowMe;
        //[KSPField(isPersistant = true)]
        //private bool _IsTargetCam;

        //[KSPField(isPersistant = true)]
        //private float _IsFollowMeOffsetXXX;
        //[KSPField(isPersistant = true)]
        //private float _IsFollowMeOffsetYYY;
        //[KSPField(isPersistant = true)]
        //private float _IsFollowMeOffsetZZZ;

        //[KSPField(isPersistant = true)]
        [SerializeField]
        public CameraInfo _cameraInfo = new CameraInfo();

        public override void OnStart(StartState state)
        {
            if (HighLogic.LoadedSceneIsEditor || MapView.MapIsEnabled || _camera != null) return;
            //public override void OnStart(StartState state = StartState.Flying)
            //{
            //    if (_camera != null) return;

            //var node = new ConfigNode("cameraConfig");
            //CameraInfo.Load(node);

            _camera = new PartCamera(part, _cameraInfo);
          
            _capObject = part.gameObject.GetChild(_cameraInfo.Cap);
            _camObject = part.gameObject.GetChild(_cameraInfo.CameraName);
            _initialUpVector = _camObject.transform.up;
            _camera._initialCamRotation = _camera._currentCamRotation = _camObject.transform.rotation;
            _camera._initialCamPosition = _camera._currentCamPosition = _camObject.transform.position;
            _camera._initialCamLocalRotation = _camera._currentCamLocalRotation = _camObject.transform.localRotation;
            _camera._initialCamLocalPosition = _camera._currentCamLocalPosition = _camObject.transform.localPosition;
        }

        public override void OnLoad(ConfigNode node)
        {
            _cameraInfo.Load(node);
        }

        public override void OnSave(ConfigNode node)
        {
            _cameraInfo.Save(node);
        }

        public override string GetInfo()
        {
            return "External camera for various purposes. Provides 'Onboard', 'Look at Me', 'Follow Me' and 'Target Cam' modes. " +
                   "Can received commands from other vessels in a short distance";
        }

        public override void OnUpdate()
        {
            if (_camera == null) return;

            if (_camera.IsButtonOff)
            {
                IsEnabled = false;
                _camera.IsButtonOff = false;
            }

            if (_camera.IsAuxiliaryWindowButtonPres)
                StartCoroutine(_camera.ResizeWindow());

            if (_camera.IsToZero)
            {
                _camera.IsToZero = false;
                StartCoroutine(_camera.ReturnCamToZero());
            }

            if (_camera.IsWaitForRay)
            {
                _camera.IsWaitForRay = false;
                StartCoroutine(_camera.WaitForRay());
            }

            _cameraInfo.IsOnboard = _camera.IsOnboard;
            _cameraInfo.IsLookAtMe = _camera.IsLookAtMe;
            _cameraInfo.IsLookAtMeAutoZoom = _camera.IsLookAtMeAutoZoom;
            _cameraInfo.IsFollowMe = _camera.IsFollowMe;
            _cameraInfo.IsTargetCam = _camera.IsTargetCam;

            if (_cameraInfo.IsFollowMe)
            {
                _cameraInfo.IsFollowMeOffsetX = _camera.IsFollowMeOffsetX;
                _cameraInfo.IsFollowMeOffsetY = _camera.IsFollowMeOffsetY;
                _cameraInfo.IsFollowMeOffsetZ = _camera.IsFollowMeOffsetZ;               
            }
            //FlightGlobals.fetch.SetVesselTarget(FlightGlobals.ActiveVessel); 
            if (_cameraInfo.IsOnboard) Onboard();
            if (_cameraInfo.IsLookAtMe) LookAtMe();
            if (_cameraInfo.IsFollowMe) FollowMe();
            if (_cameraInfo.IsTargetCam) TargetCam();

            if (_camera.IsActivated)
                _camera.Update();

            GetElectricConsumption();

            _cameraInfo.CurrentHits = _camera.hits;
            AboutHits = _cameraInfo.CurrentHits + "/4";
        }

        public void FixedUpdate()
        {
            if (IsEnabled)
                Activate();
            else
                Deactivate();
        }

        private void SetCurrentMode(bool a, bool b, bool c, bool d)
        {
            _camera.IsOnboardEnabled = a;
            _camera.IsLookAtMeEnabled = b;
            _camera.IsFollowEnabled = c;
            _camera.IsTargetCamEnabled = d;
        }

        private void Onboard()
        {
            SetCurrentMode(true, false, false, false);
        }

        private void LookAtMe()
        {
            SetCurrentMode(false, true, false, false);
            if (_camera.IsLookAtMeAutoZoom)
            {
                float dist = Vector3.Distance(_camObject.transform.position, FlightGlobals.ActiveVessel.vesselTransform.position);
                if (dist < 50) _camera.currentZoom = _camera.maxZoom;
                if (dist > 50 && dist < 100) _camera.currentZoom = 23;  //x10
                if (dist > 100 && dist < 200) _camera.currentZoom = 13; //x20
                if (dist > 200 && dist < 400) _camera.currentZoom = 3;  //x30 
                if (dist > 400) _camera.zoomMultiplier = true;
                if (_camera.zoomMultiplier)
                {
                    if (dist > 400 && dist < 800) _camera.currentZoom = 23;  //
                    if (dist > 800 && dist < 1600) _camera.currentZoom = 13; //
                    if (dist > 1600 && dist < 3200) _camera.currentZoom = 3; //
                }                
            }
            //camObject.transform.rotation = _initialCamRotation;
            _camObject.transform.LookAt(FlightGlobals.ActiveVessel.CoM, _initialUpVector);
        }

        private void FollowMe()
        {
            if (!_camera.IsFollowEnabled)
            {
                SetCurrentMode(false, false, true, false);
                _camera.CurrentCamTarget = FlightGlobals.ActiveVessel.vesselTransform;
                _camera.CurrentCam = _camObject.transform;
            }

            var offset = _camera.CurrentCamTarget.right * _cameraInfo.IsFollowMeOffsetX + _camera.CurrentCamTarget.up * _cameraInfo.IsFollowMeOffsetY +
                            _camera.CurrentCamTarget.forward * _cameraInfo.IsFollowMeOffsetZ;
            _camera.CurrentCam.position = _camera.CurrentCamTarget.position + offset;

            _camera.CurrentCam.LookAt(FlightGlobals.ActiveVessel.CoM, _camera.CurrentCamTarget.up);
        }

        private void TargetCam()
        {
            var target = TargetHelper.Target as Vessel;
            if (target == null)
            {
                ScreenMessages.PostScreenMessage("NO TARGET", 3f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }
            SetCurrentMode(false,false,false,true);
            var direction = target.transform.position - FlightGlobals.ActiveVessel.transform.position;
            direction.Normalize();
            _camObject.transform.position = target.CoM - direction * _targetOffset;

            var vectorUpNormalised = vessel.vesselTransform.up.normalized;

            _camObject.transform.LookAt(target.transform, vectorUpNormalised);                
        }

        private void GetElectricConsumption()
        {
            PartResourceDefinition definition = PartResourceLibrary.Instance.GetDefinition("ElectricCharge");
            List<Part> parts = new List<Part>();
            parts = FlightGlobals.ActiveVessel.Parts;
            double electricChargeAmount = 0;
            foreach (Part p in parts)
            {
                foreach (PartResource r in p.Resources)
                {
                    if (r.info.id == definition.id)
                    {
                        electricChargeAmount += r.amount;
                    }
                }
            }
            if (electricChargeAmount > 0)
                IsPowered = IsEnabled ? "ACTIVE" : "TRUE";
            else
                IsPowered = "FALSE";
        }

        public void Activate()
        {
            if (_camera.IsActivated) return;
            _camera.Activate();
            StartCoroutine("CapRotator");
        }
        public void Deactivate()
        {
            if (_camera == null || !_camera.IsActivated) return;
            _camera.Deactivate();
            StartCoroutine("CapRotator");
        }
        private IEnumerator CapRotator()
        {
            int step = _camera.IsActivated ? 5 : -5;
            for (int i = 0; i < 54; i++)
            {
                _capObject.transform.Rotate(new Vector3(0, 1f, 0), step);
                yield return new WaitForSeconds(1f / 270);
            }
        }

        public void OnBeforeSerialize()
        {
            //this.CombineEvent("Deploy collectors", Deactivate, true);
        }

        public void OnAfterDeserialize()
        {
        }
    }
    interface ICamPart
    {
        /// <summary>
        /// Activate camera
        /// </summary>
        void Activate();
        /// <summary>
        /// Deactivate camera
        /// </summary>
        void Deactivate();
    }
}
