using OLDD_camera.Modules;
using UnityEngine;

namespace OLDD_camera
{
    /// <summary>
    /// Destroyer cameras
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Flight, true)]
    class CameraDestroyer: MonoBehaviour
    {
        //bool isInit = false;
        /// <summary>
        /// Subscription events
        /// </summary>
        protected void Awake()
        {
            GameEvents.onPartDestroyed.Add(PartCameraDeactivate);
            GameEvents.onVesselDestroy.Add(VesselDestroy);
            GameEvents.onVesselChange.Add(RemoveLines);
        }

        private void RemoveLines(Vessel data)
        {
            Destroy(GameObject.Find("scanningRay"));
            Destroy(GameObject.Find("visibilityRay"));
        }

        protected void OnDestroy()
        {
            GameEvents.onPartDestroyed.Remove(PartCameraDeactivate);
            GameEvents.onVesselDestroy.Remove(VesselDestroy);
            GameEvents.onVesselChange.Remove(RemoveLines);
        }
        /// <summary>
        /// Destroys cameras on the ship
        /// </summary>
        private void VesselDestroy(Vessel vessel)
        {
            vessel.FindPartModulesImplementing<ICamPart>().ForEach(camPart => camPart.Deactivate());
        }

        /// <summary>
        /// Destroy camera on the part
        /// </summary>
        private void PartCameraDeactivate(Part part)
        {
            part.FindModulesImplementing<ICamPart>().ForEach(camPart => camPart.Deactivate());
        }
    }
}
