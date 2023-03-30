using System.Reflection;
using HarmonyLib;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;
using MelonLoader;
using SLZ.Interaction;
using SpiderLab.Behaviours;
using UnityEngine;

namespace SpiderlabFusionSync
{
    public class SpiderlabPatches
    {
        [HarmonyPatch(typeof(WebShooter), "CreateWebJoint")]
        public static class CreateJointPatch
        {
            public static void Prefix(WebShooter __instance, RaycastHit hit)
            {
                if (NetworkInfo.HasServer)
                {
                    if (hit.rigidbody != null)
                    {
                        ImpactUtilities.OnHitRigidbody(hit.rigidbody);
                    }

                    // Use reflection to get the hand variable from the WebShooter (Its private :( )
                    // NOOOOOOOOOOOOOOOOOOOOOOOOOO
                    // ONE LAST PIC AND ILL BE GONE!!!!! MAKE IT COUNT PUT THE FLASH ON!!!!
                    Hand hand = __instance.GetType().GetField("hand", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as Hand;
                    using (var writer = FusionWriter.Create()) {
                        using (var data = SpiderWebData.Create(PlayerIdManager.LocalId.SmallId, hit, hand)) {
                            writer.Write(data);
                            using (var message = FusionMessage.ModuleCreate<SpiderWebMessage>(writer))
                            {
                                MessageSender.SendToServer(NetworkChannel.Reliable, message);
                            }
                        }
                    }
                }
            }
        }
        
        [HarmonyPatch(typeof(WebShooter), "DestroyWebJoint")]
        public static class DestroyJointPatch
        {
            public static void Prefix(WebShooter __instance)
            {
                if (NetworkInfo.HasServer)
                {
                    Hand hand = __instance.GetType().GetField("hand", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as Hand;
                    using (var writer = FusionWriter.Create()) {
                        using (var data = SpiderWebData.Create(PlayerIdManager.LocalId.SmallId, hand)) {
                            writer.Write(data);
                            using (var message = FusionMessage.ModuleCreate<SpiderWebMessage>(writer))
                            {
                                MessageSender.SendToServer(NetworkChannel.Reliable, message);
                            }
                        }
                    }
                }
            }
        }
    }
}