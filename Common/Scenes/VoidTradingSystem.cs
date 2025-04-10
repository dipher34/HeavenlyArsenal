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
        // The item type required to trigger this trade.
        public int InputItemType { get; set; }
        // The minimum distance (in pixels) from the player that the input item must be.
        public float MinDistance { get; set; }
        // The output items to be spawned and their quantities.
        public Dictionary<int, int> OutputItems { get; set; }
        // The placement style of the spawned output items.
        public ItemReturnType ReturnType { get; set; }

        public TradeDefinition(int inputItemType, float minDistance, Dictionary<int, int> outputItems, ItemReturnType returnType)
        {
            InputItemType = inputItemType;
            MinDistance = minDistance;
            OutputItems = outputItems;
            ReturnType = returnType;
        }
    }

    // The main ModSystem that processes trades.
    class VoidTradingSystem : ModSystem
    {
        // A list of all possible trade definitions.
        private List<TradeDefinition> tradeDefinitions = new List<TradeDefinition>();

        public override void Load()
        {
            
            tradeDefinitions.Add(new TradeDefinition(
                // Input: AncientCoin
                ModContent.ItemType<AncientCoin>(),
                // Minimum distance required between the coin and the player.
                1000f,
                // Output: Armor trade – one of each armor piece.
                new Dictionary<int, int>
                {
                    { ModContent.ItemType<ShintoArmorLeggings>(), 1 },
                    { ModContent.ItemType<ShintoArmorBreastplate>(), 1 },
                    { ModContent.ItemType<ShintoArmorHelmetAll>(), 1 }
                },
                // Return type determines the spawn location of the output items.
                ItemReturnType.None
            ));


            
            tradeDefinitions.Add(new TradeDefinition(
                // Input: Chalice of the blood god
                ModContent.ItemType<ChaliceOfTheBloodGod>(),
                // Minimum distance required between the coin and the player.
                1000f,
                
                new Dictionary<int, int>
                {
                    { ModContent.ItemType<ChaliceOfFun>(), 1 }
                },
                
                ItemReturnType.None
            ));

            
            // reference
            /*
            tradeDefinitions.Add(new TradeDefinition(
                ModContent.ItemType<ExampleInputItem>(), 
                800f, 
                new Dictionary<int, int>
                {
                    { ModContent.ItemType<ExampleOutputItem1>(), 2 },
                    { ModContent.ItemType<ExampleOutputItem2>(), 1 }
                },
                ItemReturnType.SpatOut
            ));
            */
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
                                // Remove the input item.
                                worldItem.TurnToAir();
                                // Play a sound to indicate successful trade execution.
                                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Clap);

                                // Process each output item defined in this trade.
                                foreach (var outputPair in trade.OutputItems)
                                {
                                    int outputItemType = outputPair.Key;
                                    int quantity = outputPair.Value;
                                    for (int q = 0; q < quantity; q++)
                                    {
                                        // Determine the spawn position based on the specified return type.
                                        Vector2 spawnPosition = GetSpawnPosition(player, trade.ReturnType);

                                        // Spawn the output item.
                                        int index = Item.NewItem(new EntitySource_Misc("VoidTradingSystem"),
                                            (int)spawnPosition.X, (int)spawnPosition.Y, player.width, player.height, outputItemType);

                                        //weeee
                                        if (index >= 0 && index < Main.maxItems)
                                        {
                                            Main.item[index].velocity.Y++;
                                        }
                                    }
                                }
                                // Once we process this trade for one valid input item, exit the inner loop.
                                break;
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
