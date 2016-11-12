using System.Collections;
using System.Linq;
using OLDD_camera.Camera;
using UnityEngine;

namespace OLDD_camera.Modules
{
    class DockingCameraModule : PartModule, ICamPart
    {
        /// <summary>
        /// Module adds an external camera and gives control over it
        /// </summary>
        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Camera", isPersistant = true)]
        [UI_Toggle(controlEnabled = true, enabledText = "On", disabledText = "Off", scene = UI_Scene.All)]
        public bool IsEnabled;

        [KSPField]
        public int allowedDistance = 1000;

        [KSPField]
        public float maxSafeSpeed = 2;

        [KSPField]
        public int windowSize = 256;
        
        [KSPField]
        public bool noise = false;

        [KSPField]
        public string targetCrossColorDPAI = "0.9,0.0,0.0,1.0";

        [KSPField]
        public string targetCrossColorOLDD = "0.0,0.9,0.0,1.0";

        private DockingCamera _camera;

        public override void OnStart(StartState state = StartState.Flying)
        {
            if (state == StartState.Editor || _camera != null)
                return;
            Start();
        }

        public void Start()
        {
            if (_camera == null)
                _camera = new DockingCamera(part, noise, windowSize);

            _camera.MaxSpeed = maxSafeSpeed;
            _camera.MaxSpeed = maxSafeSpeed;
            var colorOLDD = targetCrossColorOLDD.Split(',').Select(float.Parse).ToList(); // parsing color to RGBA
            _camera.TargetCrossColorOLDD = new Color(colorOLDD[0], colorOLDD[1], colorOLDD[2], colorOLDD[3]);
            var colorDPAI = targetCrossColorDPAI.Split(',').Select(float.Parse).ToList(); // parsing color to RGBA
            _camera.TargetCrossColor = new Color(colorDPAI[0], colorDPAI[1], colorDPAI[2], colorDPAI[3]); 
        }

        public override void OnUpdate()
        {
            if (_camera == null) 
                return;
            if (_camera.IsActivated)
                _camera.Update();
            if (_camera.IsButtonOff)
            {
                IsEnabled = false;
                _camera.IsButtonOff = false;
            }
            if (IsEnabled)
                Activate();
            else
                Deactivate();
            if (_camera.IsAuxiliaryWindowButtonPres)
                StartCoroutine(_camera.ResizeWindow());
        }

        private IEnumerator WhiteNoiseUpdate() //whitenoise
        {
            while (_camera.IsActivated)
            {
                _camera.UpdateNoise();
                yield return new WaitForSeconds(.1f);
            }
        }
        public void Activate()
        {
            if (_camera.IsActivated) return;
            if (TargetHelper.IsTargetSelect)
            {
                var target = new TargetHelper(part);
                target.Update();
                if (target.Destination > allowedDistance)
                {
                    ScreenMessages.PostScreenMessage("You need to set target and be closer than " + allowedDistance + " meters from target", 5f, ScreenMessageStyle.UPPER_CENTER);
                    IsEnabled = false;
                }
                else
                {
                    _camera.Activate();
                    StartCoroutine("WhiteNoiseUpdate"); //whitenoise
                }
            }
            else
            {
                ScreenMessages.PostScreenMessage("You need to set target", 5f, ScreenMessageStyle.UPPER_CENTER);
                IsEnabled = false;
            }
        }
        public void Deactivate()
        {
            if (!_camera.IsActivated) return;
            _camera.Deactivate();
        }

    }
}
