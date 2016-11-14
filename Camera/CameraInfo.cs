using System;

namespace OLDD_camera.Camera
{
    [Serializable]
    public class CameraInfo //: IConfigNode
    {
        //public CameraInfo()
        //{
        //    WindowSize = 256;
        //    CurrentHits = -1;
        //    BulletName = "Sphere";
        //    CameraName = "CamExt";
        //    RotatorZ = "Case";
        //    RotatorY = "Tube";
        //    Zoommer = "Lenz";
        //    Cap = "Cap";
        //    Stepper = 1000; // using for zooming visualization. for different camera models
        //    AllowedScanDistance = 1000; //max allowed distance for scanning experiment
        //    ResourceScanning = "ElectricCharge.50"; //(resourseName/resourceUsage) for scanning
        //    IsOnboard = false;
        //    IsLookAtMe = false;
        //    IsLookAtMeAutoZoom = false;
        //    IsFollowMe = false;
        //    IsFollowMeOffsetX = 0;
        //    IsFollowMeOffsetY = 0;
        //    IsFollowMeOffsetZ = 0;
        //    IsTargetCam = false;
        //}

        public int WindowSize;// { get; set; }

        public string ResourceScanning { get; set; }

        public string BulletName { get; set; }

        public int CurrentHits { get; set; }

        public string RotatorZ { get; set; }

        public string RotatorY { get; set; }

        public string Cap { get; set; }

        public string Zoommer { get; set; }

        public float Stepper { get; set; }

        public string CameraName { get; set; }

        public int AllowedScanDistance { get; set; }

        public bool IsOnboard { get; set; }

        public bool IsLookAtMe { get; set; }

        public bool IsLookAtMeAutoZoom { get; set; }

        public bool IsFollowMe { get; set; }

        public float IsFollowMeOffsetX { get; set; }

        public float IsFollowMeOffsetY { get; set; }

        public float IsFollowMeOffsetZ { get; set; }

        public bool IsTargetCam { get; set; }

        public void Load(ConfigNode node)
        {
            var config = node.GetNode("cameraConfig");
            if (config == null) return;
            if (config.HasValue("windowSize"))
                WindowSize = int.Parse(config.GetValue("windowSize"));
            if (config.HasValue("cameraName"))
                CameraName = config.GetValue("cameraName");
            if (config.HasValue("rotatorZ"))
                RotatorZ = config.GetValue("rotatorZ");
            if (config.HasValue("rotatorY"))
                RotatorY = config.GetValue("rotatorY");
            if (config.HasValue("cap"))
                Cap = config.GetValue("cap");
            if (config.HasValue("bulletName"))
                BulletName = config.GetValue("bulletName");
            if (config.HasValue("stepper"))
                Stepper = int.Parse(config.GetValue("stepper"));
            if (config.HasValue("allowedScanDistance"))
                AllowedScanDistance = int.Parse(config.GetValue("allowedScanDistance"));
            if (config.HasValue("IsOnboard"))
                IsOnboard = bool.Parse(config.GetValue("IsOnboard"));
            if (config.HasValue("IsLookAtMe"))
                IsLookAtMe = bool.Parse(config.GetValue("IsLookAtMe"));
            if (config.HasValue("IsLookAtMeAutoZoom"))
                IsLookAtMeAutoZoom = bool.Parse(config.GetValue("IsLookAtMeAutoZoom"));
            if (config.HasValue("IsFollowMe"))
                IsFollowMe = bool.Parse(config.GetValue("IsFollowMe"));
            if (config.HasValue("IsFollowMeOffsetX"))
                IsFollowMeOffsetX = float.Parse(config.GetValue("IsFollowMeOffsetX"));
            if (config.HasValue("IsFollowMeOffsetY"))
                IsFollowMeOffsetY = float.Parse(config.GetValue("IsFollowMeOffsetY"));
            if (config.HasValue("IsFollowMeOffsetZ"))
                IsFollowMeOffsetZ = float.Parse(config.GetValue("IsFollowMeOffsetZ"));
            if (config.HasValue("IsTargetCam"))
                IsTargetCam = bool.Parse(config.GetValue("IsTargetCam"));
        }

        public void Save(ConfigNode node)
        {
            var config = node.AddNode("cameraConfig");

            config.AddValue("windowSize", WindowSize);
            config.AddValue("cameraName", CameraName);
            config.AddValue("rotatorZ", RotatorZ);
            config.AddValue("rotatorY", RotatorY);
            config.AddValue("cap", Cap);
            config.AddValue("bulletName", BulletName);
            config.AddValue("stepper", Stepper);
            config.AddValue("allowedScanDistance", AllowedScanDistance);
            config.AddValue("IsOnboard", IsOnboard);
            config.AddValue("IsLookAtMe", IsLookAtMe);
            config.AddValue("IsLookAtMeAutoZoom", IsLookAtMeAutoZoom);
            config.AddValue("IsFollowMe", IsFollowMe);
            config.AddValue("IsFollowMeOffsetX", IsFollowMeOffsetX);
            config.AddValue("IsFollowMeOffsetY", IsFollowMeOffsetY);
            config.AddValue("IsFollowMeOffsetZ", IsFollowMeOffsetZ);
            config.AddValue("IsTargetCam", IsTargetCam);
        }
    }
}