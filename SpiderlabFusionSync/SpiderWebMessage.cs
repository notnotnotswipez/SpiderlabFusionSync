using System;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using MelonLoader;
using SLZ;
using SLZ.Interaction;
using UnityEngine;

namespace SpiderlabFusionSync
{
    public class SpiderWebData : IFusionSerializable, IDisposable
    {
        public byte playerShortId;
        public bool isDestroy;
        public SerializedGameObjectReference connectedRb;
        public Vector3 _position;
        public float distance;
        public Handedness Handedness;
        private byte _handedness;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public void Serialize(FusionWriter writer)
        {
            writer.Write(playerShortId);
            writer.Write(isDestroy);
            writer.Write(connectedRb);
            writer.Write(_position);
            writer.Write(distance);
            writer.Write(_handedness);
        }

        public void Deserialize(FusionReader reader)
        {
            playerShortId = reader.ReadByte();
            isDestroy = reader.ReadBoolean();
            connectedRb = reader.ReadFusionSerializable<SerializedGameObjectReference>();
            _position = reader.ReadVector3();
            distance = reader.ReadSingle();
            _handedness = reader.ReadByte();
            Handedness = (Handedness) _handedness;
        }
        
        public static SpiderWebData Create(byte shortId, Hand hand)
        {
            Handedness handedness = hand.handedness;
            byte handednessByte = (byte)handedness;

            return new SpiderWebData()
            {
                playerShortId = shortId,
                isDestroy = true,
                connectedRb = new SerializedGameObjectReference(null),
                _position = Vector3.zero,
                distance = 1,
                _handedness = handednessByte
            };
        }

        public static SpiderWebData Create(byte shortId, RaycastHit hit, Hand hand)
        {
            Vector3 position = hit.point;
            SerializedGameObjectReference serializedGameObjectReference = new SerializedGameObjectReference(null);
            if (hit.rigidbody != null)
            {
                serializedGameObjectReference = new SerializedGameObjectReference(hit.rigidbody.gameObject);
                position = hit.rigidbody.transform.InverseTransformPoint(position);
            }

            Handedness handedness = hand.handedness;
            byte handednessByte = (byte)handedness;

            return new SpiderWebData()
            {
                playerShortId = shortId,
                isDestroy = false,
                connectedRb = serializedGameObjectReference,
                _position = position,
                distance = hit.distance,
                _handedness = handednessByte
            };
        }
    }
    public class SpiderWebMessage : ModuleMessageHandler
    {
        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (var reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<SpiderWebData>())
                {
                    if (NetworkInfo.IsServer && isServerHandled)
                    {
                        using (var message = FusionMessage.ModuleCreate<SpiderWebMessage>(bytes))
                        {
                            MessageSender.BroadcastMessageExcept(data.playerShortId, NetworkChannel.Reliable, message);
                        }
                    }

                    PlayerId playerId = PlayerIdManager.GetPlayerId(data.playerShortId);
                    if (PlayerRepManager.TryGetPlayerRep(playerId, out var rep))
                    {
                        if (RepWebShooter.webManagers.ContainsKey(rep))
                        {
                            RepWebShooter webShooter = RepWebShooter.webManagers[rep];
                            if (data.isDestroy)
                            {
                                webShooter.DestroyWeb(data.Handedness);
                            }
                            else
                            {
                                Rigidbody rigidbody = null;
                                GameObject connectedRb = data.connectedRb.gameObject;
                                if (connectedRb != null)
                                {
                                    rigidbody = connectedRb.GetComponent<Rigidbody>();
                                }

                                webShooter.MakeWeb(data.Handedness, data._position, data.distance, rigidbody);
                            }
                        }
                    }
                }
            }
        }
    }
}