using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Gores
{
    class BulletGore : ModGore
    {
        public override string Texture => base.Texture;


        public override void OnSpawn(Gore gore, IEntitySource source)
        {
            Dust.NewDustPerfect(gore.position, DustID.Sandnado, gore.velocity, 150, default);
            //base.OnSpawn(gore, source);
        }
    }
}
