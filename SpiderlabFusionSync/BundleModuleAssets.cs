using System;
using System.Linq;
using System.Reflection;
using SpiderLab.Data;
using SpiderLab.Utils;
using UnityEngine;

namespace SpiderlabFusionSync
{
    public class BundleModuleAssets
    {
        public static void LoadAssets()
        {
            Assembly spiderLabAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly asm) => asm.GetName().Name.ToLower().Contains("spiderlab"));
            Assets.spiderLabBundle = EmebeddedAssetBundle.LoadFromAssembly(spiderLabAssembly, "SpiderLab.src.Data.spiderlab.bundle");
            if (Assets.spiderLabBundle == null)
            {
                throw new NullReferenceException("SpiderLab bundle is null! Did you forget to compile the bundle into the dll?");
            }
            Assets.webLinePrefab = Assets.spiderLabBundle.LoadAssetWithFlags<GameObject>("prefabs/webline");
            Assets.webShooterReticlePrefab = Assets.spiderLabBundle.LoadAssetWithFlags<GameObject>("prefabs/webshooterreticle");
            Assets.webLineAudioHigh = Assets.spiderLabBundle.LoadAssetWithFlags<AudioClip>("audio/webline_high");
            Assets.webLineAudioLow = Assets.spiderLabBundle.LoadAssetWithFlags<AudioClip>("audio/webline_low");
            Assets.webShotAudioHigh = Assets.spiderLabBundle.LoadAssetWithFlags<AudioClip>("audio/webshot_high");
            Assets.webShotAudioLow = Assets.spiderLabBundle.LoadAssetWithFlags<AudioClip>("audio/webshot_low");
            Assets.spiderLabBundle.Unload(false);
        }
        
        public static class Assets
        {
            public static AssetBundle spiderLabBundle;
            public static GameObject webLinePrefab;
            public static GameObject webShooterReticlePrefab;
            public static AudioClip webLineAudioHigh;
            public static AudioClip webLineAudioLow;
            public static AudioClip webShotAudioHigh;
            public static AudioClip webShotAudioLow;
        }
    }
}