using UnityEngine;

namespace OLDD_camera
{
    public enum ShaderType
    {
        OldTV,
        NightVisionNoise1,
        Noise,
        NoiseNightVision,
        NightVisionClear,
        Grayscale,
        None
    }
    public enum ShaderType1
    {
        OldTV,
        NightVisionNoise1,
    }
    public enum ShaderType2
    {
        None,
        Grayscale,
        NightVisionClear
    }
    public enum ShaderType3
    {
        Noise,
        NoiseNightVision,
    }
    class CameraShaders
    {
        public static Material GetShader(ShaderType type)
        {
            switch (type)
            {
                case ShaderType.OldTV: return OldTV;
                case ShaderType.NightVisionNoise1: return NightVisionNoise1;
                case ShaderType.Noise: return Noise;
                case ShaderType.NoiseNightVision: return NoiseNightVision;
                case ShaderType.NightVisionClear: return NightVisionClear;
                case ShaderType.Grayscale: return Grayscale;
                case ShaderType.None: return null;
                default: return null;
            }
        }
        public static Material GetShader1(ShaderType1 type)
        {
            switch (type)
            {
                case ShaderType1.OldTV: return OldTV;
                case ShaderType1.NightVisionNoise1: return NightVisionNoise1;
                default: return null;
            }
        }

        public static Material GetShader2(ShaderType2 type)
        {
            switch (type)
            {
                case ShaderType2.None: return null;
                case ShaderType2.NightVisionClear: return NightVisionClear;
                case ShaderType2.Grayscale: return Grayscale;
                default: return null;
            }
        }

        static Material oldtv = null;
        public static Material OldTV
        {
            get
            {
                if (oldtv == null)
                {
                    oldtv = AssetLoader.matOldTV;
                }
                return oldtv;
            }
        }

        static Material grayscale = null;
        public static Material Grayscale
        {
            get
            {
                if (grayscale == null)
                {
                    grayscale = AssetLoader.matGrayscale; //new Material(Shader.Find("Hidden/Grayscale Effect"));
                }
                return grayscale;
            }
        }

        static Material nightvisionclear = null;
        public static Material NightVisionClear
        {
            get
            {
                if (nightvisionclear == null)
                {
                    nightvisionclear = AssetLoader.matNightVisionClear;
                }
                return nightvisionclear;
            }
        }

        static Material nightvisionnoise1 = null;
        public static Material NightVisionNoise1
        {
            get
            {
                if (nightvisionnoise1 == null)
                {
                    nightvisionnoise1 = AssetLoader.matNightVisionNoise1;
                }
                return nightvisionnoise1;
            }
        }

        static Material noise = null;
        public static Material Noise
        {
            get
            {
                if (noise == null)
                {
                    noise = AssetLoader.matNoise;
                }
                return noise;
            }
        }

        static Material noisenightvision = null;
        public static Material NoiseNightVision
        {
            get
            {
                if (noisenightvision == null)
                {
                    noisenightvision = AssetLoader.matNoiseNightVision;
                }
                return noisenightvision;
            }
        }
    }
}
