using System.Collections.Generic;
using System.Reflection;
using BoneLib;
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

        [HarmonyPatch(typeof(WebShooter), "HandleAiming")]
        public static class HandleAimingPatch
        {
            public static bool Prefix(WebShooter __instance)
            {
                Hand hand = ReflectionUtils.GetFieldValue<Hand>(__instance, "hand");
                if (Player.GetObjectInHand(hand) != null)
                {
                    ReflectionUtils.InvokeMethod(__instance, "ToggleReticle", new object[]{false});
                    return false;
                }

                Transform aimReticle = ReflectionUtils.GetFieldValue<Transform>(__instance, "aimReticle");
                LayerMask layerMask = ReflectionUtils.GetFieldValue<LayerMask>(__instance, "layerMask");
                aimReticle.rotation = Quaternion.LookRotation(Player.playerHead.forward);
                RaycastHit[] hits = Physics.RaycastAll(__instance.transform.position, __instance.transform.forward, 70f);
 
                RaycastHit hitPlayer = default(RaycastHit);
                bool foundPlayer = false;
                bool ignoreFound = false;
  
                List<RaycastHit> validHits = new List<RaycastHit>();
                foreach (var hit in hits)
                {
                    bool isInLayerMask = layerMask.Includes(hit.collider.gameObject.layer);
                    if (isInLayerMask)
                    {
                        validHits.Add(hit);
                    }
                    if (hit.rigidbody != null)
                    {
                        if (hit.rigidbody.transform.root.name == PlayerRepManager.PlayerRepName)
                        {
                            hitPlayer = hit;
                            foundPlayer = true;
                        }
                    }
                }

                if (foundPlayer)
                {
                    foreach (var validHit in validHits)
                    {
                        if (validHit.distance < hitPlayer.distance)
                        {
                            ignoreFound = true;
                        }
                    }
                }
                
                if (foundPlayer && !ignoreFound)
                {
                    aimReticle.position = hitPlayer.point;
                    ReflectionUtils.InvokeMethod(__instance, "ToggleReticle", new object[]{true});
                    if (ReflectionUtils.InvokeMethod<bool>(__instance, "CanShootWebLine", null))
                    {
                        ReflectionUtils.InvokeMethod(__instance, "CreateWebJoint", new object[]{hitPlayer});
                        ReflectionUtils.InvokeMethod(__instance, "PlayAudio", new object[] { BundleModuleAssets.Assets.webLineAudioHigh, BundleModuleAssets.Assets.webLineAudioLow });
                        hand.Controller.haptor.Haptic_Tap();
                        ReflectionUtils.InvokeMethod(__instance, "ToggleReticle", new object[]{false});
                        return false;
                    }
                    return false;
                }
                return true;
            }
        }
    }
    
    public static class LayerMaskExtensions
    {
        public static bool Includes(
            this LayerMask mask,
            int layer)
        {
            return (mask.value & 1 << layer) > 0;
        }
    }
}