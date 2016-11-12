using System.Collections;
using System.Collections.Generic;
using OLDD_camera.Camera;
using UnityEngine;

namespace OLDD_camera.Modules
{
    /// <summary>
    /// Module adds an external camera and gives control over it
    /// </summary>
    class PartCameraModule : PartModule, ICamPart
    {
        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Camera powered: ")]
        public string IsPowered;

        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Bullets: ")]
        public string aboutHits;

        //[KSPField(isPersistant = true)]
        //public int currentHits = -1;
        
        //[KSPField]
        //public string bulletName = "Sphere";

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Camera", isPersistant = true),
        UI_Toggle(controlEnabled = true, enabledText = "ON", disabledText = "OFF", scene = UI_Scene.All)]
        public bool IsEnabled;

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

        private GameObject capObject;
        private GameObject camObject;
        private PartCamera camera;
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

        [KSPField(isPersistant = true)]
        private CameraInfo _cameraInfo;

        public override void OnStart(StartState state)
        {
            if (HighLogic.LoadedSceneIsEditor || MapView.MapIsEnabled || camera != null) return;

            camera = new PartCamera(part, _cameraInfo);
          
            capObject = part.gameObject.GetChild(_cameraInfo.Cap);
            camObject = part.gameObject.GetChild(_cameraInfo.CameraName);
            _initialUpVector = camObject.transform.up;
            camera._initialCamRotation = camera._currentCamRotation = camObject.transform.rotation;
            camera._initialCamPosition = camera._currentCamPosition = camObject.transform.position;
            camera._initialCamLocalRotation = camera._currentCamLocalRotation = camObject.transform.localRotation;
            camera._initialCamLocalPosition = camera._currentCamLocalPosition = camObject.transform.localPosition;
        }

        public override string GetInfo()
        {
            return "External camera for various purposes. Provides 'Onboard', 'Look at Me', 'Follow Me' and 'Target Cam' modes. " +
                   "Can received commands from other vessels in a short distance";
        }

        public override void OnUpdate()
        {
            if (camera == null) return;

            if (camera.IsButtonOff)
            {
                IsEnabled = false;
                camera.IsButtonOff = false;
            }

            if (camera.IsAuxiliaryWindowButtonPres)
                StartCoroutine(camera.ResizeWindow());

            if (camera.IsToZero)
            {
                camera.IsToZero = false;
                StartCoroutine(camera.ReturnCamToZero());
            }

            if (camera.IsWaitForRay)
            {
                camera.IsWaitForRay = false;
                StartCoroutine(camera.WaitForRay());
            }

            _cameraInfo.IsOnboard = camera.IsOnboard;
            _cameraInfo.IsLookAtMe = camera.IsLookAtMe;
            _cameraInfo.IsLookAtMeAutoZoom = camera.IsLookAtMeAutoZoom;
            _cameraInfo.IsFollowMe = camera.IsFollowMe;
            _cameraInfo.IsTargetCam = camera.IsTargetCam;

            if (_cameraInfo.IsFollowMe)
            {
                _cameraInfo.IsFollowMeOffsetX = camera.IsFollowMeOffsetXXX;
                _cameraInfo.IsFollowMeOffsetY = camera.IsFollowMeOffsetYYY;
                _cameraInfo.IsFollowMeOffsetZ = camera.IsFollowMeOffsetZZZ;               
            }
            //FlightGlobals.fetch.SetVesselTarget(FlightGlobals.ActiveVessel); 
            if (_cameraInfo.IsOnboard) Onboard();
            if (_cameraInfo.IsLookAtMe) LookAtMe();
            if (_cameraInfo.IsFollowMe) FollowMe();
            if (_cameraInfo.IsTargetCam) TargetCam();

            if (camera.IsActivated)
                camera.Update();

            GetElectricConsumption();

            _cameraInfo.CurrentHits = camera.hits;
            aboutHits = _cameraInfo.CurrentHits + "/4";
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
            camera.IsOnboardEnabled = a;
            camera.IsLookAtMeEnabled = b;
            camera.IsFollowEnabled = c;
            camera.IsTargetCamEnabled = d;
        }

        private void Onboard()
        {
            SetCurrentMode(true, false, false, false);
        }

        private void LookAtMe()
        {
            SetCurrentMode(false, true, false, false);
            if (camera.IsLookAtMeAutoZoom)
            {
                float dist = Vector3.Distance(camObject.transform.position, FlightGlobals.ActiveVessel.vesselTransform.position);
                if (dist < 50) camera.currentZoom = camera.maxZoom;
                if (dist > 50 && dist < 100) camera.currentZoom = 23;  //x10
                if (dist > 100 && dist < 200) camera.currentZoom = 13; //x20
                if (dist > 200 && dist < 400) camera.currentZoom = 3;  //x30 
                if (dist > 400) camera.zoomMultiplier = true;
                if (camera.zoomMultiplier)
                {
                    if (dist > 400 && dist < 800) camera.currentZoom = 23;  //
                    if (dist > 800 && dist < 1600) camera.currentZoom = 13; //
                    if (dist > 1600 && dist < 3200) camera.currentZoom = 3; //
                }                
            }
            //camObject.transform.rotation = _initialCamRotation;
            camObject.transform.LookAt(FlightGlobals.ActiveVessel.CoM, _initialUpVector);
        }

        private void FollowMe()
        {
            if (!camera.IsFollowEnabled)
            {
                SetCurrentMode(false, false, true, false);
                camera.CurrentCamTarget = FlightGlobals.ActiveVessel.vesselTransform;
                camera.CurrentCam = camObject.transform;
            }

            var offset = camera.CurrentCamTarget.right * _cameraInfo.IsFollowMeOffsetX + camera.CurrentCamTarget.up * _cameraInfo.IsFollowMeOffsetY +
                            camera.CurrentCamTarget.forward * _cameraInfo.IsFollowMeOffsetZ;
            camera.CurrentCam.position = camera.CurrentCamTarget.position + offset;

            camera.CurrentCam.LookAt(FlightGlobals.ActiveVessel.CoM, camera.CurrentCamTarget.up);
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
            camObject.transform.position = target.CoM - direction * _targetOffset;

            var vectorUpNormalised = vessel.vesselTransform.up.normalized;

            camObject.transform.LookAt(target.transform, vectorUpNormalised);                
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
            if (camera.IsActivated) return;
            camera.Activate();
            StartCoroutine("CapRotator");
        }
        public void Deactivate()
        {
            if (!camera.IsActivated) return;
            camera.Deactivate();
            StartCoroutine("CapRotator");
        }
        private IEnumerator CapRotator()
        {
            int step = camera.IsActivated ? 5 : -5;
            for (int i = 0; i < 54; i++)
            {
                capObject.transform.Rotate(new Vector3(0, 1f, 0), step);
                yield return new WaitForSeconds(1f / 270);
            }
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
