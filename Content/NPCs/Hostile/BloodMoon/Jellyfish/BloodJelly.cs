using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Particles;
using HeavenlyArsenal.Core;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Luminance.Common.Utilities.Utilities;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Jellyfish
{
    partial class BloodJelly : BloodMoonBaseNPC
    {
        #region Setup
        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Jellyfish/BloodJelly";

        private Dictionary<int, (Vector2[], Vector2[])> Tendrils = new Dictionary<int, (Vector2[], Vector2[])>(2);
        public int CosmeticTime
        {
            get => (int)NPC.localAI[0];
            set => NPC.localAI[0] = value;
        }
        private readonly int tendrilCount = 4;

        #endregion
        public override void SetDefaults()
        {
            NPC.lifeMax = 40000;
            NPC.damage = 300;
            NPC.defense = 300;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.Size = new Vector2(40, 80);
            NPC.aiStyle = -1;
        }
        public override void OnSpawn(IEntitySource source)
        {
          
            for (int i = 0; i < tendrilCount; i++)
            {
                Tendrils.Add(i, (new Vector2[tendrilCount], new Vector2[tendrilCount]));
            }
        }

        public override void AI()
        {
            if (NPC.ai[1] != 0)
            {
                NPC.Center = Main.MouseWorld;
            }
            
            Time++;
        }

        public override void PostAI()
        {


            CosmeticTime++;
        }
    }
}
