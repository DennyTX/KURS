namespace OLDD_camera.Camera
{
    public class CameraInfo: IConfigNode
    {
        public int WindowSize { get; set; }

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

            WindowSize = int.Parse(config.GetValue("windowSize"));
            CameraName = config.GetValue("cameraName");
            RotatorZ = config.GetValue("rotatorZ");
            RotatorY = config.GetValue("rotatorY");
            Cap = config.GetValue("cap");
            BulletName = config.GetValue("bulletName");
            Stepper = int.Parse(config.GetValue("stepper"));
            AllowedScanDistance = int.Parse(config.GetValue("allowedScanDistance"));
            IsOnboard = bool.Parse(config.GetValue("IsOnboard"));
            IsOnboard = bool.Parse(config.GetValue("IsLookAtMe"));
            IsOnboard = bool.Parse(config.GetValue("IsLookAtMeAutoZoom"));
            IsOnboard = bool.Parse(config.GetValue("IsFollowMe"));
            IsOnboard = bool.Parse(config.GetValue("IsFollowMeOffsetX"));
            IsOnboard = bool.Parse(config.GetValue("IsFollowMeOffsetY"));
            IsOnboard = bool.Parse(config.GetValue("IsFollowMeOffsetZ"));
            IsOnboard = bool.Parse(config.GetValue("IsTargetCam"));
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