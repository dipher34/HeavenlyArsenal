using HeavenlyArsenal.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Renderers;

namespace HeavenlyArsenal.Content.Particles
{
    internal class FusionReaction : BaseParticle
    {
        public static ParticlePool<FusionReaction> pool = new ParticlePool<FusionReaction>(500, GetNewParticle<FusionReaction>);

     
       
        public void Prepare()
        {
         
           
        }

        public override void FetchFromPool()
        {
            base.FetchFromPool();
            
        }

        public override void Update(ref ParticleRendererSettings settings)
        {
           
           
                ShouldBeRemovedFromRenderer = true;
        }

        public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
        {
          

        }

    }
}
