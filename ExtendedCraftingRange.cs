using CoreLib.Util;
using CoreLib.Util.Extensions;
using HarmonyLib;
using PugMod;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

[Harmony]
public class ExtendedCraftingRange : IMod
{
    public const string VERSION = "1.0.1";
    public const string NAME = "ExtendedCraftingRange";
    public const string AUTHOR = "Futroo";

    private LoadedMod modInfo;

    private static float nearbyDistance = 7f;

    public static List<Chest> chestsToAdd = new List<Chest>();

    public void EarlyInit()
    {
        Debug.Log($"[{NAME}]: Version: {VERSION}");
        modInfo = API.ModLoader.LoadedMods.FirstOrDefault(obj => obj.Handlers.Contains(this));
        if (modInfo == null)
        {
            Debug.Log($"[{NAME}]: Failed to load {NAME}!");
            return;
        }
        Debug.Log($"[{NAME}]: Mod loaded successfully!");
    }

    public void Init()
    {

    }


    public void Shutdown()
    {

    }
    public void ModObjectLoaded(UnityEngine.Object obj)
    {

    }

    public void Update()
    {

    }
    private static List<Chest> SearchForChests()
    {
        PlayerController pl = GameManagers.GetMainManager().player;
        if (pl == null || pl.playerInventoryHandler == null)
        {
            return null;
        }

        List<Chest> _output = new List<Chest>();

        Transform poolChest = GameObject.Find("Pool Chest").transform;
        Transform poolBossChest = GameObject.Find("Pool BossChest").transform;
        Transform poolNonPaintableChest = GameObject.Find("Pool NonPaintableChest").transform;

        List<Transform> allChests = poolChest.GetAllChildren().Where(obj => obj.gameObject.activeSelf).ToList();
        allChests.AddRange(poolBossChest.GetAllChildren().Where(obj => obj.gameObject.activeSelf).ToList());
        allChests.AddRange(poolNonPaintableChest.GetAllChildren().Where(obj => obj.gameObject.activeSelf).ToList());

        foreach (Transform t in allChests)
        {
            Chest _chestComponent = t.GetComponent<Chest>();
            if (_chestComponent != null && IsInRange(pl.WorldPosition, _chestComponent.WorldPosition, nearbyDistance))
            {
                _output.Add(_chestComponent);
            }
        }
        return _output;
    }

    private static bool IsInRange(Vector3 position1, Vector3 position2, float distanceThreshold)
    {
        float distance = Vector3.Distance(position1, position2);
        return distance < distanceThreshold;
    }

    #region Harmony
    [HarmonyPrefix, HarmonyPatch(typeof(CraftingHandler), "HasMaterialsInCraftingInventoryToCraftRecipe", new Type[] { typeof(CraftingHandler.RecipeInfo), typeof(bool), typeof(List<Chest>), typeof(bool), typeof(int) })]
    public static void HasMaterialsInCraftingInventoryToCraftRecipePrefix(CraftingHandler.RecipeInfo recipeInfo, bool checkPlayerInventoryToo, ref List<Chest> nearbyChestsToTakeMaterialsFrom, bool useRequiredObjectsSetInRecipeInfo, int multiplier)
    {
        nearbyChestsToTakeMaterialsFrom = chestsToAdd;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(CraftingHandler), "GetAmountOfAvailableMaterialsToUse")]
    public static void GetAmountOfAvailableMaterialsToUsePrefix(ObjectID material, ref List<Chest> nearbyChestsToTakeMaterialsFrom)
    {
        nearbyChestsToTakeMaterialsFrom = chestsToAdd;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(CraftingHandler), "GetCraftingMaterialInfosForRecipe", new Type[] {typeof(CraftingHandler.RecipeInfo), typeof(List<Chest>), typeof(bool), typeof(bool) })]
    public static void GetCraftingMaterialInfosForRecipePrefix(CraftingHandler.RecipeInfo recipeInfo, ref List<Chest> nearbyChestsToTakeMaterialsFrom, bool isRepairing, bool isReinforcing)
    {
        nearbyChestsToTakeMaterialsFrom = chestsToAdd;
    }
    
    [HarmonyPrefix, HarmonyPatch(typeof(CraftingHandler), "GetCraftingMaterialInfosForUpgrade")]
    public static void GetCraftingMaterialInfosForUpgradePrefix(int level, ref List<Chest> nearbyChestsToTakeMaterialsFrom)
    {
        nearbyChestsToTakeMaterialsFrom = chestsToAdd;
    }
    
    [HarmonyPrefix, HarmonyPatch(typeof(CraftingHandler), "GetMaterialInfos")]
    public static void GetMaterialInfosPrefix(List<CraftingObject> objectsRequired, float costMultiplier, ref List<Chest> nearbyChestsToTakeMaterialsFrom)
    {
        nearbyChestsToTakeMaterialsFrom = chestsToAdd;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(CraftingHandler), "GetNearbyChestWithMaterial")]
    public static void GetNearbyChestWithMaterialPrefix(ObjectID materialId, ref List<Chest> nearbyChestsToTakeMaterialsFrom, int amountInChests)
    {
        nearbyChestsToTakeMaterialsFrom = chestsToAdd;
    }
    
    [HarmonyPrefix, HarmonyPatch(typeof(CraftingHandler), "GetAnyNearbyChests")]
    public static bool GetAnyNearbyChestsPrefix(ref List<Chest> __result)
    {
        __result = chestsToAdd;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(UIManager), "OnPlayerInventoryOpen")]
    public static void OnPlayerInventoryOpenPrefix(LoadingScene __instance)
    {
        if (Manager.main.player.activeCraftingHandler != null)
        {
            chestsToAdd = SearchForChests();
        }
    }
    #endregion Harmony
}
