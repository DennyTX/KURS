using System.Collections.Generic;
using KSP.IO;
using OLDD_camera.Camera;
using UnityEngine;

namespace OLDD_camera
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class DockCamToolbarButton : MonoBehaviour
    {
        private static IButton _toolbarButton;
        private static PluginConfiguration _config;
        private static Rect _settingsWindowPosition;
        private static Rect _lastWindowPosition;
        private static Rect _toolbarWindowPosition;

        private const int WindowTextureWidth = 256; 
        
        public static bool WindowVisiblityOn;
        private static bool _blizzyToolbarAvailable;
        private static bool _showWindow;
        private static bool _shadersToUse0 = true;
        private static bool _shadersToUse1;
        private static bool _shadersToUse2;
        private static bool _dist2500 = true;
        private static bool _dist9999;

        private readonly int _modulePartCameraId = "PartCameraModule".GetHashCode();
        private static readonly VesselRanges DefaultRanges = PhysicsGlobals.Instance.VesselRangesDefault;
        private readonly VesselRanges.Situation _myRanges = new VesselRanges.Situation(10000, 10000, 2500, 2500);
        private static List<Vessel> _vesselsWithCamera = new List<Vessel>();

        public void Awake()
        {
            _blizzyToolbarAvailable = ToolbarManager.ToolbarAvailable;
            if (_blizzyToolbarAvailable)
            {
                _toolbarButton = ToolbarManager.Instance.add("DockCamera", "dockCam");
                _toolbarButton.TexturePath = "OLDD/DockingCam/DockingCamIcon";
                _toolbarButton.ToolTip = "Show/Hide Docking Camera";
                _toolbarButton.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
                _toolbarButton.Visible = true;
                _toolbarButton.Enabled = true;
                _toolbarButton.OnClick += (e) =>
                {
                    WindowVisiblityOn = !WindowVisiblityOn;
                };
            }
        }
        public void Start()
        {
            LoadWindowData();
            _settingsWindowPosition.y = _toolbarWindowPosition.yMax;
            if (HighLogic.LoadedSceneIsFlight)
            {
                GameEvents.onVesselCreate.Add(NewVesselCreated);
                GameEvents.onVesselChange.Add(NewVesselCreated);
                GameEvents.onVesselLoaded.Add(NewVesselCreated);
            }
        }

        public void Update()
        {
            _showWindow = WindowVisiblityOn && HighLogic.LoadedSceneIsFlight && !FlightGlobals.ActiveVessel.isEVA && !MapView.MapIsEnabled;
            if (_shadersToUse0)
                BaseCamera.shadersToUse = 0;
            else if (_shadersToUse1)
                BaseCamera.shadersToUse = 1;
            else if (_shadersToUse2)
                BaseCamera.shadersToUse = 2;
        }

        private void OnGUI()
        {
            OnWindowOLDD();
        }

        private void OnWindowOLDD()
        {
            if (_showWindow)
            {
                _toolbarWindowPosition.width = WindowTextureWidth;
                var vesselsCount = _vesselsWithCamera.Count;
                var height = 20 * vesselsCount;
                _toolbarWindowPosition.height = 120 + height + 10;
                _toolbarWindowPosition = Util.ConstrainToScreen(GUI.Window(2222, _toolbarWindowPosition, drawOnWindowOLDD, "OLDD Camera Settings"), 200);//, labelStyle));
            }
        }
        
        public static void drawOnWindowOLDD(int windowID)
        {
            var CheckDist = FlightGlobals.ActiveVessel.vesselRanges.landed.load;
            if (GUI.Toggle(new Rect(20, 20, 88, 22), _dist2500, "2500"))
            {
                _dist2500 = true;
                _dist9999 = false;
                if (CheckDist > 3333 && _dist2500)
                    GameEvents.onVesselChange.Fire(FlightGlobals.ActiveVessel);
            }
            if (GUI.Toggle(new Rect(80, 20, 88, 22), _dist9999, "9999"))
            {
                _dist9999 = true;
                _dist2500 = false;
                if (CheckDist < 3333 && _dist9999)
                    GameEvents.onVesselChange.Fire(FlightGlobals.ActiveVessel);
            }
            var UnloadDistance = "Unload at: " + FlightGlobals.ActiveVessel.vesselRanges.landed.load;
            GUI.Label(new Rect(140, 20, 111, 22), UnloadDistance, Styles.GUIStyleLabelBold);

            if (GUI.Toggle(new Rect(20, 40, 222, 20), _shadersToUse0, "Shaders pack Full (7 choices)"))
            {
                BaseCamera.shadersToUse = 0;
                _shadersToUse0 = true;
                _shadersToUse1 = false;
                _shadersToUse2 = false;
                SaveWindowData();
            }
            if (GUI.Toggle(new Rect(20, 60, 222, 20), _shadersToUse1, "Shaders pack Noisy (2 choices)"))
            {
                BaseCamera.shadersToUse = 1;
                _shadersToUse0 = false;
                _shadersToUse1 = true;
                _shadersToUse2 = false;
                SaveWindowData();
            }
            if (GUI.Toggle(new Rect(20, 80, 222, 20), _shadersToUse2, "Shaders pack Standart (3 choices)"))
            {
                BaseCamera.shadersToUse = 2;
                _shadersToUse0 = false;
                _shadersToUse1 = false;
                _shadersToUse2 = true;
                SaveWindowData();
            }

            GUI.Label(new Rect(2, 102, WindowTextureWidth, 24), "- Vessels with camera in range -", Styles.GUIStyleGreenLabelBold);

            var vessels = _vesselsWithCamera;
            vessels.Remove(FlightGlobals.ActiveVessel);
            foreach (var vessel in vessels)
            {
                var i = vessels.IndexOf(vessel) + 1;
                var range = Mathf.Round(Vector3.Distance(vessel.transform.position, FlightGlobals.ActiveVessel.transform.position));
                var situation = vessel.RevealSituationString();
                var str = string.Format("{0}. {1} ({2:N} m) - {3} ", i, vessel.vesselName, range, situation);
                if (range <= CheckDist)
                    GUI.Label(new Rect(20, 104 + 20 * i, 222, 20), str, Styles.GUIStyleGreenLabelStandart);
                else
                    GUI.Label(new Rect(20, 104 + 20 * i, 222, 20), str);
            }

            GUI.DragWindow();

            if (_toolbarWindowPosition.x != _lastWindowPosition.x || _toolbarWindowPosition.y != _lastWindowPosition.y)
            {
                _lastWindowPosition.x = _toolbarWindowPosition.x;
                _lastWindowPosition.y = _toolbarWindowPosition.y;
                SaveWindowData();
            }
        }

        private void NewVesselCreated(Vessel data)
        {
            var allVessels = FlightGlobals.Vessels;
            _vesselsWithCamera = GetVesselsWithCamera(allVessels);
            if (!_dist2500)
            {
                foreach (var vessel in _vesselsWithCamera)
                {
                    vessel.vesselRanges.subOrbital = _myRanges;
                    vessel.vesselRanges.landed = _myRanges;
                    vessel.vesselRanges.escaping = _myRanges;
                    vessel.vesselRanges.orbit = _myRanges;
                    vessel.vesselRanges.prelaunch = _myRanges;
                    vessel.vesselRanges.splashed = _myRanges;
                }
            }
            else
            {
                foreach (var vessel in _vesselsWithCamera)
                {
                    vessel.vesselRanges.subOrbital = DefaultRanges.subOrbital;
                    vessel.vesselRanges.landed = DefaultRanges.landed;
                    vessel.vesselRanges.escaping = DefaultRanges.escaping;
                    vessel.vesselRanges.orbit = DefaultRanges.orbit;
                    vessel.vesselRanges.prelaunch = DefaultRanges.prelaunch;
                    vessel.vesselRanges.splashed = DefaultRanges.splashed;
                }
            }
        }

        public List<Vessel> GetVesselsWithCamera(List<Vessel> allVessels)
        {
            List<Vessel> vesselsWithCamera = new List<Vessel>();
            foreach (var vessel in allVessels)
            {
                if (vessel.Parts.Count == 0) continue;
                if (vessel.vesselType == VesselType.Debris
                    || vessel.vesselType == VesselType.Flag
                    || vessel.vesselType == VesselType.Unknown) continue;
                foreach (Part part in vessel.Parts)
                {
                    if (part.Modules.Contains(_modulePartCameraId))
                    {
                        if (vesselsWithCamera.Contains(vessel)) continue;
                        vesselsWithCamera.Add(vessel);
                    }
                }
            }
            return vesselsWithCamera;
        }

        private static void SaveWindowData()
        {
            _config.SetValue("window_position", _toolbarWindowPosition);
            _config.SetValue("shadersToUse0", _shadersToUse0);
            _config.SetValue("shadersToUse1", _shadersToUse1);
            _config.SetValue("shadersToUse2", _shadersToUse2);
            _config.save();
        }

        private static void LoadWindowData()
        {
            _config = PluginConfiguration.CreateForType<DockCamToolbarButton>(null);
            _config.load();
            Rect defaultWindow = new Rect(); 
            _toolbarWindowPosition = _config.GetValue<Rect>("window_position", defaultWindow);
            _shadersToUse0 = _config.GetValue("shadersToUse0", _shadersToUse0);
            _shadersToUse1 = _config.GetValue("shadersToUse1", _shadersToUse1);
            _shadersToUse2 = _config.GetValue("shadersToUse2", _shadersToUse2);
        }

        private void OnDestroy()
        {
            if (_toolbarButton != null) _toolbarButton.Destroy();
            GameEvents.onVesselCreate.Remove(NewVesselCreated);
            GameEvents.onVesselChange.Remove(NewVesselCreated);
            GameEvents.onVesselLoaded.Remove(NewVesselCreated);
        }
    }
}
