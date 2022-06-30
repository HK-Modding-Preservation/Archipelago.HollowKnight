﻿using System;
using System.Collections.Generic;
using ItemChanger;
using UnityEngine.SceneManagement;

namespace Archipelago.HollowKnight
{
    public enum GoalsLookup
    {
        Any = 0,
        HollowKnight = 1,
        SealedSiblings = 2,
        Radiance = 3,
        Godhome = 4,
        MAX = Godhome
    }

    public abstract class Goal
    {
        public abstract string Name { get; }
        public abstract string Description { get; }

        private static readonly Dictionary<GoalsLookup, Goal> Lookup = new Dictionary<GoalsLookup, Goal>()
        {
            [GoalsLookup.Any] = new AnyGoal(),
            [GoalsLookup.HollowKnight] = new HollowKnightGoal(),
            [GoalsLookup.SealedSiblings] = new SealedSiblingsGoal(),
            [GoalsLookup.Radiance] = new RadianceGoal(),
            [GoalsLookup.Godhome] = new GodhomeGoal()
        };

        protected void FountainPlaqueTopEdit(ref string s) => s = "Your goal is";
        protected void FountainPlaqueNameEdit(ref string s) => s = Name;
        protected void FountainPlaqueDescEdit(ref string s) => s = Description;

        // Helpers for subclasses.
        protected bool AcquiredVoidHeart => PlayerData.instance.GetInt(nameof(PlayerData.royalCharmState)) == 4;
        protected bool EquippedVoidHeart => AcquiredVoidHeart && PlayerData.instance.GetBool(nameof(PlayerData.equippedCharm_36));
        protected bool HasThreeDreamers => PlayerData.instance.GetInt(nameof(PlayerData.guardiansDefeated)) >= 3;

        public abstract bool VictoryCondition(string sceneName);

        public static Goal GetGoal(GoalsLookup key)
        {
            Goal value;
            if (Lookup.TryGetValue(key, out value))
            {
                return value;
            }
            Archipelago.Instance.LogError($"Listed goal is {key}, which is greater than {GoalsLookup.MAX}.  Is this an outdated client?");
            throw new ArgumentOutOfRangeException($"Unrecognized goal condition {key} (are you running an outdated client?)");
        }

        public void CheckForVictory(Scene scene)
        {
            Archipelago.Instance.LogDebug($"Checking for victory; goal is {this.Name}; scene {scene.name}");
            if (VictoryCondition(scene.name))
            {
                Archipelago.Instance.LogDebug($"Victory detected, declaring!");
                Archipelago.Instance.DeclareVictory();
            }
        }

        public void Select()
        {
            ItemChanger.Events.AddLanguageEdit(new ItemChanger.LanguageKey("Prompts", "FOUNTAIN_PLAQUE_TOP"), FountainPlaqueTopEdit);
            ItemChanger.Events.AddLanguageEdit(new ItemChanger.LanguageKey("Prompts", "FOUNTAIN_PLAQUE_MAIN"), FountainPlaqueNameEdit);
            ItemChanger.Events.AddLanguageEdit(new ItemChanger.LanguageKey("Prompts", "FOUNTAIN_PLAQUE_DESC"), FountainPlaqueDescEdit);
            ItemChanger.Events.OnSceneChange += CheckForVictory;
        }

        public void Unselect()
        {
            ItemChanger.Events.RemoveLanguageEdit(new ItemChanger.LanguageKey("Prompts", "FOUNTAIN_PLAQUE_TOP"), FountainPlaqueTopEdit);
            ItemChanger.Events.RemoveLanguageEdit(new ItemChanger.LanguageKey("Prompts", "FOUNTAIN_PLAQUE_MAIN"), FountainPlaqueNameEdit);
            ItemChanger.Events.RemoveLanguageEdit(new ItemChanger.LanguageKey("Prompts", "FOUNTAIN_PLAQUE_DESC"), FountainPlaqueDescEdit);
            ItemChanger.Events.OnSceneChange -= CheckForVictory;
        }
    }

    public class AnyGoal : Goal
    {
        private static List<string> VictoryScenes = new()
        {
            SceneNames.Cinematic_Ending_A,   // THK
            SceneNames.Cinematic_Ending_B,   // Sealed Siblings
            SceneNames.Cinematic_Ending_C,   // Radiance
            "Cinematic_Ending_D",            // Godhome no flower quest(?)
            SceneNames.Cinematic_Ending_E    // Godhome w/ flower quest
        };

        public override string Name => "Beat the Game";
        public override string Description => "Complete Hollow Knight with any ending.";

        public override bool VictoryCondition(string sceneName)
        {
            return VictoryScenes.Contains(sceneName);
        }
    }

    public class HollowKnightGoal : AnyGoal
    {
        public override string Name => "The Hollow Knight";
        public override string Description => "Defeat The Hollow Knight<br>or any other ending with 3 dreamers.";

        public override bool VictoryCondition(string sceneName)
        {
            return HasThreeDreamers && base.VictoryCondition(sceneName);
        }
    }

    public class SealedSiblingsGoal : Goal
    {
        private static List<string> VictoryScenes = new()
        {
            SceneNames.Cinematic_Ending_B,   // Sealed Siblings
            SceneNames.Cinematic_Ending_C,   // Radiance
            "Cinematic_Ending_D",            // Godhome no flower quest(?)
            SceneNames.Cinematic_Ending_E    // Godhome w/ flower quest
        };

        public override string Name => "Sealed Siblings";
        public override string Description => "Complete the Sealed Siblings ending<br>or any other ending with 3 dreamers and Void Heart equipped.";

        public override bool VictoryCondition(string sceneName)
        {
            return HasThreeDreamers && EquippedVoidHeart && VictoryScenes.Contains(sceneName);
        }
    }

    public class RadianceGoal : Goal
    {
        private static List<string> VictoryScenes = new()
        {
            SceneNames.Cinematic_Ending_C,   // Radiance
            "Cinematic_Ending_D",            // Godhome no flower quest(?)
            SceneNames.Cinematic_Ending_E    // Godhome w/ flower quest
        };

        public override string Name => "Dream No More";
        public override string Description => "Defeat The Radiance or Absolute Radiance<br>after obtaining Void Heart and 3 dreamers.";

        public override bool VictoryCondition(string sceneName)
        {
            return HasThreeDreamers && EquippedVoidHeart && VictoryScenes.Contains(sceneName);
        }
    }
    public class GodhomeGoal : Goal
    {
        private static List<string> VictoryScenes = new()
        {
            "Cinematic_Ending_D",            // Godhome no flower quest(?)
            SceneNames.Cinematic_Ending_E    // Godhome w/ flower quest
        };

        public override string Name => "Embrace the Void";
        public override string Description => "Defeat Absolute Radiance<br>at the end of Pantheon 5.";

        public override bool VictoryCondition(string sceneName)
        {
            return VictoryScenes.Contains(sceneName);
        }
    }
}
