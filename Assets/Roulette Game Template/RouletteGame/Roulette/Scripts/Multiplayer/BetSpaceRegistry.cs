using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Extension data for BetSpace to support networked betting with indexing
/// This enables the NetworkGameState to reference bet spaces by index
/// </summary>
public static class BetSpaceRegistry
{
    private static List<BetSpace> allBetSpaces = new List<BetSpace>();
    private static Dictionary<int, BetSpace> betSpaceByIndex = new Dictionary<int, BetSpace>();

    public static bool IsInitialized { get; private set; } = false;

    /// <summary>
    /// Register a bet space with a unique index
    /// </summary>
    public static void RegisterBetSpace(BetSpace betSpace, int index)
    {
        if (!allBetSpaces.Contains(betSpace))
        {
            allBetSpaces.Add(betSpace);
            
            if (!betSpaceByIndex.ContainsKey(index))
            {
                betSpaceByIndex[index] = betSpace;
            }
        }
    }

    /// <summary>
    /// Get a bet space by its index
    /// </summary>
    public static BetSpace GetBetSpaceByIndex(int index)
    {
        if (betSpaceByIndex.TryGetValue(index, out BetSpace betSpace))
        {
            return betSpace;
        }
        return null;
    }

    /// <summary>
    /// Get the index of a bet space
    /// </summary>
    public static int GetIndexOfBetSpace(BetSpace betSpace)
    {
        foreach (var kvp in betSpaceByIndex)
        {
            if (kvp.Value == betSpace)
                return kvp.Key;
        }
        return -1;
    }

    /// <summary>
    /// Get all registered bet spaces
    /// </summary>
    public static List<BetSpace> GetAllBetSpaces()
    {
        return allBetSpaces;
    }

    /// <summary>
    /// Clear the registry (for scene changes)
    /// </summary>
    public static void Clear()
    {
        allBetSpaces.Clear();
        betSpaceByIndex.Clear();
        IsInitialized = false;
    }

    /// <summary>
    /// Initialize the registry by finding all bet spaces in the scene
    /// </summary>
    public static void Initialize()
    {
        Clear();
        
        BetSpace[] spaces = Object.FindObjectsOfType<BetSpace>();
        
        for (int i = 0; i < spaces.Length; i++)
        {
            RegisterBetSpace(spaces[i], i);
        }
        
        IsInitialized = true;
        
        Debug.Log($"[BetSpaceRegistry] Initialized with {allBetSpaces.Count} bet spaces");
    }
}

/// <summary>
/// Extension methods for ResultManager to support networked operations
/// </summary>
public static class ResultManagerExtensions
{
    /// <summary>
    /// Get a bet space by its registered index
    /// </summary>
    public static BetSpace GetBetSpaceByIndex(int index)
    {
        return BetSpaceRegistry.GetBetSpaceByIndex(index);
    }

    /// <summary>
    /// Get the index of a bet space
    /// </summary>
    public static int GetIndexOfBetSpace(BetSpace betSpace)
    {
        return BetSpaceRegistry.GetIndexOfBetSpace(betSpace);
    }
}
