using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;
using NoxusBoss.Core.World.GameScenes.AvatarUniverseExploration;
using HeavenlyArsenal.Content.Items.Armor;
using HeavenlyArsenal.Content.Items.Misc;
using NoxusBoss.Assets;
using CalamityMod.Items.Accessories;

namespace HeavenlyArsenal.Common.Scenes
{
    // Define an enum to specify where the output items should appear.
    public enum ItemReturnType
    {
        None,       // Spawned on top of (at) the player.
        ShadowHand, // Delivered to the player's Shadow Hand (not implemented).
        SpatOut     // Spawned from below the bottom of the screen.
    }

    // Data structure that defines a trade.
    public class TradeDefinition
    {
        public int InputItemType { get; set; }
        public float MinDistance { get; set; }
        public List<(int itemType, int quantity)> OutputItems { get; set; }
        public ItemReturnType ReturnType { get; set; }

        public TradeDefinition(int inputItemType, float minDistance, ItemReturnType returnType, params int[] outputItemPairs)
        {
            InputItemType = inputItemType;
            MinDistance = minDistance;
            ReturnType = returnType;
            OutputItems = new List<(int, int)>();

            if (outputItemPairs.Length % 2 != 0)
            {
                throw new ArgumentException("Output item pairs must be in itemType, quantity format.");
            }

            for (int i = 0; i < outputItemPairs.Length; i += 2)
            {
                OutputItems.Add((outputItemPairs[i], outputItemPairs[i + 1]));
            }
        }
    }


    // The main ModSystem that processes trades.
    class VoidTradingSystem : ModSystem
    {
        // A list of all possible trade definitions.
        private List<TradeDefinition> tradeDefinitions = new List<TradeDefinition>();

        public override void PostSetupContent()
        {
            // Initialize trade definitions.
            tradeDefinitions.Add(new TradeDefinition(
               //item to trade
               ModContent.ItemType<ChaliceOfFun>(),
               1000f,
               ItemReturnType.None,
               //Items to get back

               ModContent.ItemType<ChaliceOfTheBloodGod>(), 1,
               ModContent.ItemType<AncientCoin>(), 1
                ));

            
               tradeDefinitions.Add(new TradeDefinition(
               //item to trade
               ModContent.ItemType<ChaliceOfTheBloodGod>(),
               1000f,
               ItemReturnType.None,
               //Items to get back
               
               ModContent.ItemType<ChaliceOfFun>(), 1,
               ModContent.ItemType<ShintoArmorLeggings>(), 1,
               ModContent.ItemType<ShintoArmorBreastplate>(), 1,
               ModContent.ItemType<ShintoArmorHelmetAll>(), 1
                ));


            tradeDefinitions.Add(new TradeDefinition(
               //item to trade
               ModContent.ItemType<CoinofDeceit>(),
               1000f,
               ItemReturnType.None,
               //Items to get back

               ModContent.ItemType<AncientCoin>(), 1
                ));
        }

        public override void PostUpdateEverything()
        {
           
            // Only process trades when in the Avatar Universe.
            if (AvatarUniverseExplorationSystem.InAvatarUniverse)
            {
                Player player = Main.LocalPlayer;

                // Check each trade definition.
                foreach (TradeDefinition trade in tradeDefinitions)
                {
                    // For each trade, iterate over all world items to look for the required input item.
                    for (int i = 0; i < Main.maxItems; i++)
                    {
                        Item worldItem = Main.item[i];

                        if (worldItem.active && worldItem.type == trade.InputItemType)
                        {
                            // Check that the found item meets the minimum distance requirement.
                            if (Vector2.Distance(worldItem.Center, player.Center) > trade.MinDistance)
                            {
                                // Log the deletion for debugging.
                                Main.NewText($"Deleting trade input item: {worldItem.Name}", Color.AntiqueWhite);

                                // Remove the input item.
                                worldItem.TurnToAir();

                                // Play a sound to indicate successful trade execution.
                                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Clap with { PitchVariance = 0.2f });
                                // Process each output item defined in this trade.
                                foreach ((int outputItemType, int quantity) in trade.OutputItems)
                                {
                                    Main.NewText($"Prepairing to create:{outputItemType}", Color.AntiqueWhite);
                                    for (int r = 0; r < quantity; r++)
                                    {
                                        Vector2 spawnPosition = GetSpawnPosition(player, trade.ReturnType);

                                        int index = Item.NewItem(new EntitySource_Misc("VoidTradingSystem"),
                                            (int)spawnPosition.X, (int)spawnPosition.Y,
                                            player.width, player.height, outputItemType); // Ensure outputItemType is correctly used here.
                                        Main.NewText($"Created item: {Main.item[index].Name} (Type: {outputItemType}), Index: {index}", Color.AntiqueWhite);
                                        if (index >= 0 && index < Main.maxItems)
                                        {
                                            // Optional: additional properties
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
       

        /// <summary>
        /// Determines the spawn position for an output item based on the specified return type.
        /// </summary>
        /// <param name="player">Reference to the player.</param>
        /// <param name="returnType">The chosen return type for the spawn location.</param>
        /// <returns>A Vector2 representing the position where the item will spawn.</returns>
        private Vector2 GetSpawnPosition(Player player, ItemReturnType returnType)
        {
            switch (returnType)
            {
                case ItemReturnType.None:
                    // Spawn directly at the player's center.
                    return player.Center;
                case ItemReturnType.ShadowHand:
                    // ShadowHand is not implemented; fallback to player's center.
                    return player.Center;
                case ItemReturnType.SpatOut:
                    // Calculate a position below the bottom of the visible screen.
                    float bottomY = Main.screenPosition.Y + Main.screenHeight;
                    return new Vector2(player.Center.X, bottomY + 20f);
                default:
                    return player.Center;
            }
        }
    }
}
