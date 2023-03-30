using LabFusion.SDK.Modules;
using LabFusion.Utilities;
using MelonLoader;

namespace SpiderlabFusionSync
{
    public class ModuleClass : Module
    {
        public override void OnModuleLoaded()
        {
            MelonLogger.Msg("Loaded SpiderlabFusionSync Module!");
            // Spiderlab assets, Parzival made their bundle class internal so I can't access it unless I wanna use reflection and ehhhhhhhh
            MelonLogger.Msg("Loading SpiderLab assets... Again...");
            BundleModuleAssets.LoadAssets();
            MelonLogger.Msg("Finished reloading SpiderLab assets.");
        }
    }
}