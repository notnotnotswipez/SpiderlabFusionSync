using System.Collections.Generic;
using LabFusion.Representation;
using MelonLoader;
using SLZ;
using SLZ.Interaction;
using SLZ.Rig;
using SLZ.SaveData;
using SpiderLab.Behaviours;
using SpiderLab.Utils;
using UnityEngine;

namespace SpiderlabFusionSync
{
    public class RepWebShooter
    {
        public static Dictionary<PlayerRep, RepWebShooter> webManagers = new Dictionary<PlayerRep, RepWebShooter>();

        private RigManager localManager;
        private ConfigurableJoint r_webJoint;
        private ConfigurableJoint l_webJoint;

        private WebLine r_webLine;
        private WebLine l_webLine;

        private AudioSource mainSource;

        private Hand leftHand;
        private Hand rightHand;

        private float r_low_web_length = 0;
        private float l_low_web_length = 0;
        
        public RepWebShooter(PlayerRep playerRep, RigManager manager)
        {
            if (webManagers.ContainsKey(playerRep))
            {
                webManagers.Remove(playerRep);
            }

            webManagers.Add(playerRep, this);
            localManager = manager;
            rightHand = localManager.physicsRig.rightHand;
            leftHand = localManager.physicsRig.leftHand;
            r_webLine = GameObject.Instantiate(BundleModuleAssets.Assets.webLinePrefab, rightHand.gameObject.transform).AddComponent<WebLine>();
            l_webLine = GameObject.Instantiate(BundleModuleAssets.Assets.webLinePrefab, leftHand.gameObject.transform).AddComponent<WebLine>();
            
            mainSource = localManager.physicsRig.m_pelvis.gameObject.AddComponent<AudioSource>();
            mainSource.playOnAwake = false;
            mainSource.minDistance = 0.5f;
            mainSource.spatialBlend = 1f;
        }
        
        public void Update()
        {
            if (l_webJoint != null)
            {
                Vector3 vector = l_webJoint.GetConnectedPoint();
                Vector3 anchorPoint = l_webJoint.GetAnchorPoint();
                l_webLine.UpdateLine(leftHand.palmPositionTransform.position, vector);
                float magnitude = (vector - anchorPoint).magnitude;
                if (magnitude < l_low_web_length)
                {
                    l_webJoint.SetLinearLimit(magnitude, 1f);
                    l_low_web_length = magnitude;
                }
            }
            if (r_webJoint != null)
            {
                Vector3 vector = r_webJoint.GetConnectedPoint();
                Vector3 anchorPoint = r_webJoint.GetAnchorPoint();
                r_webLine.UpdateLine(rightHand.palmPositionTransform.position, vector);
                float magnitude = (vector - anchorPoint).magnitude;
                if (magnitude < r_low_web_length)
                {
                    r_webJoint.SetLinearLimit(magnitude, 1f);
                    r_low_web_length = magnitude;
                }
            }
        }

        public void DestroyWeb(Handedness handedness)
        {
            if (handedness == Handedness.RIGHT)
            {
                r_webLine.SetActive(false);
                GameObject.Destroy(r_webJoint);
            }
            else
            {
                l_webLine.SetActive(false);
                GameObject.Destroy(l_webJoint);
            }
        }

        public void MakeWeb(Handedness handedness, Vector3 vector3, float distance, Rigidbody body)
        {
            Hand hand = null;
            ConfigurableJoint webJoint = null;
            WebLine webLine = null;

            switch (handedness)
            {
                case Handedness.RIGHT:
                    hand = localManager.physicsRig.rightHand;
                    webLine = r_webLine;
                    r_low_web_length = distance;
                    break;
                case Handedness.LEFT:
                    hand = localManager.physicsRig.leftHand;
                    webLine = l_webLine;
                    l_low_web_length = distance;
                    break;
            }

            webJoint = hand.gameObject.AddComponent<ConfigurableJoint>();
            if (body != null)
            {
                webJoint.connectedBody = body;
            }
            
            webJoint.SetAnchorPoint(hand.gameObject.transform.position);
            webJoint.autoConfigureConnectedAnchor = false;
            
            // We need to rely on this vector being auto configured for us.
            // I.E (World space for world attachments, relative to RB for RB attachments)
            // This is for ping compensation. We dont want to rely on world space conversions on anyone elses computer than the simulator.
            webJoint.connectedAnchor = vector3;
            webJoint.SetLinearMotion(ConfigurableJointMotion.Limited);
            webJoint.SetLinearLimit(distance + 0.075f, 1f);
            
            webJoint.SetSpringLimit(504857.16f, 50485.715f);
            webJoint.SetSpringDrive(0f, 0f, 0f);
            webJoint.breakForce = 1000000f;
            webJoint.enableCollision = true;
            webLine.SetActive(true);
            webLine.UpdateLine(hand.palmPositionTransform.position, vector3);

            if (handedness == Handedness.RIGHT)
            {
                r_webJoint = webJoint;
            }
            else
            {
                l_webJoint = webJoint;
            }

            PlayAudio(BundleModuleAssets.Assets.webLineAudioHigh, BundleModuleAssets.Assets.webLineAudioLow);
        }
        
        // Ripped from Spiderlab. Thank you Parzival.
        private void PlayAudio(AudioClip high, AudioClip low)
        {
            AudioClip clip = (Random.value > 0.5f) ? high : low;
            float volumeScale = 0.5f * (DataManager.ActiveSave.PlayerSettings.VolumeSFX / 10f);
            mainSource.loop = false;
            mainSource.PlayOneShot(clip, volumeScale);
        }
    }
}