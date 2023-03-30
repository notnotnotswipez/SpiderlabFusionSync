using LabFusion.Representation;
using LabFusion.SDK.Modules;
using LabFusion.Utilities;
using MelonLoader;
using SLZ.Rig;
using UnityEngine;

namespace SpiderlabFusionSync
{
    public class Class1 : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("Loading internal module...");
            ModuleHandler.LoadModule(System.Reflection.Assembly.GetExecutingAssembly());
            MultiplayerHooking.OnPlayerRepCreated += MultiplayerHooking_OnPlayerRepCreated;
        }

        public override void OnDeinitializeMelon()
        {
            MultiplayerHooking.OnPlayerRepCreated -= MultiplayerHooking_OnPlayerRepCreated;
        }

        public override void OnUpdate()
        {
            foreach (var keyPair in RepWebShooter.webManagers)
            {
                keyPair.Value.Update();
            }
        }

        private void MultiplayerHooking_OnPlayerRepCreated(RigManager rigManager)
        {
            PlayerRep playerRep;
            if (!PlayerRepManager.TryGetPlayerRep(rigManager, out playerRep))
                return;
            new RepWebShooter(playerRep, rigManager);
        }
    }
}