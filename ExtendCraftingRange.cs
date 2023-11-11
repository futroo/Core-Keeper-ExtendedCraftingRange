using PugMod;
using UnityEngine;
using CoreLib;
using CoreLib.Util;
using CoreLib.Util.Extensions;
using HarmonyLib;
using System.Linq;
using System.Collections.Generic;

[Harmony]
public class ExtendCraftingRange : IMod
{
    public const string VERSION = "1.0.0";
    public const string NAME = "ExtendCraftingRange";
    public const string AUTHOR = "Futroo";

    private LoadedMod modInfo;

    private static float nearbyDistance = 7f;

    public static List<Chest> chestsToAdd = new List<Chest>();

    private bool wasCraftingUIShowing = false;

    public void EarlyInit()
    {
        UnityEngine.Debug.Log($"[{NAME}]: Version: {VERSION}");
        modInfo = API.ModLoader.LoadedMods.FirstOrDefault(obj => obj.Handlers.Contains(this));
        if (modInfo == null)
        {
            UnityEngine.Debug.Log($"[{NAME}]: Failed to load {NAME}!");
            return;
        }
        UnityEngine.Debug.Log($"[{NAME}]: Mod loaded successfully!");
    }

    public void Init()
    {
        
    }

    public void ModObjectLoaded(Object obj)
    {
        
    }

    public void Shutdown()
    {
        
    }

    public void Update()
    {
        if (GameManagers.GetMainManager() == null || !GameManagers.GetMainManager().currentSceneHandler.isInGame)
        {
            return;
        }

        bool isCraftingOpened = GameManagers.GetManager<UIManager>().activeCraftingUI != null;
        if (isCraftingOpened && !wasCraftingUIShowing)
        {
            SearchForChests();
            wasCraftingUIShowing = true;
        }
        else if (!isCraftingOpened && wasCraftingUIShowing)
        {
            wasCraftingUIShowing = false;
        }
    }

    private void SearchForChests()
    {
        PlayerController pl = GameManagers.GetMainManager().player;

        Transform cameraManager = GameObject.Find("Camera Manager").transform;
        List<Transform> origoTransforms = cameraManager.GetAllChildren().Where(obj => obj.name == "OrigoTransform").ToList();

        List<Chest> _chests = new();

        foreach (Transform t in origoTransforms)
        {
            foreach (var _chest in t.GetAllChildren().Where(obj => obj.name.Contains("Chest") && obj.gameObject.activeInHierarchy))
            {
                Chest _chestComponent = _chest.GetComponent<Chest>();
                if (_chestComponent != null && IsInRange(pl.WorldPosition, _chestComponent.WorldPosition, nearbyDistance))
                {
                    _chests.Add(_chestComponent);
                }
            }
        }
        chestsToAdd = _chests;
    }

    private static bool IsInRange(Vector3 position1, Vector3 position2, float distanceThreshold)
    {
        float distance = Vector3.Distance(position1, position2);
        return distance < distanceThreshold;
    }

    #region Harmony
    [HarmonyPrefix, HarmonyPatch(typeof(CraftingHandler), "GetAnyNearbyChests")]
    public static bool GetAnyNearbyChests(ref List<Chest> __result)
    {
        __result = chestsToAdd;
        return false;
    }
    #endregion Harmony
}
