using System;
using System.Runtime.CompilerServices;
using DunGen;
using HarmonyLib;
using LC_API.ServerAPI;
using Unity.Netcode;
using UnityEngine;
using Newtonsoft.Json;

namespace LockDoorsMod;

[JsonObject]
internal class LockDoorData
{
    [JsonProperty]
    public ulong networkObjectId { get; set; }

    [JsonProperty]
    public ulong clientId { get; set; }
}

[HarmonyPatch(typeof(DoorLock))]
public class DoorLockPatch
{
    [HarmonyPatch(nameof(DoorLock.Awake))]
    [HarmonyPrefix]
    private static void Awake()
    {
        Logger.logger.LogInfo($"[BROADCAST_RECEIVE] Awaken");
        Networking.GetString += CloseDoorReceived;
    }

    private static void CloseDoorReceived(string data, string signature)
    {
        var dataDeserialized = JsonConvert.DeserializeObject<LockDoorData>(data);
        
        // Hierarchy: Door -> Cube(DoorLock)
        
        Logger.logger.LogInfo($"[BROADCAST_RECEIVE] Got message \n data: {data} \n signature: {signature}");
        switch (signature)
        {
            case "lock_door":
                var doorComponent = FindObjectById(Convert.ToUInt64(dataDeserialized.networkObjectId));
                var doorLock = doorComponent.GetComponentInChildren<DoorLock>();
                Logger.logger.LogInfo($"[BROADCAST_RECEIVE] found object with id {dataDeserialized.networkObjectId}: {doorComponent} \n {doorLock} \n doorLockNID: {doorLock.NetworkObjectId}");
                doorLock.LockDoor();
                break;
        }
    }
    
    [HarmonyPatch(nameof(DoorLock.LockDoor))]
    [HarmonyPrefix]
    private static bool LockDoorPatch(ref DoorLock __instance, ref bool ___isDoorOpened, ref bool ___isLocked, ref AudioSource ___doorLockSFX, ref AudioClip ___unlockSFX)
    {
        if (___isDoorOpened || ___isLocked)
        {
            Logger.logger.LogInfo("The door is opened or locked. Can't lock it");
            return false;
        }

        var obj = ___doorLockSFX;
        obj.PlayOneShot(___unlockSFX);
        
        return true;
    }
    
    public static void LockDoorPatchSyncWithServer(ulong networkObjectId)
    {
        var data = new LockDoorData()
        {
            networkObjectId = networkObjectId,
            clientId = GameNetworkManager.Instance.localPlayerController.playerClientId
        };
        
        Logger.logger.LogInfo("[BROADCAST] Locking the door");
        Networking.Broadcast(
            JsonConvert.SerializeObject(data, Formatting.None), "lock_door");
    }
    
    private static GameObject FindObjectById(ulong networkId)
    {
        return networkId == 0 ? null : NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkId].gameObject;
    }
}