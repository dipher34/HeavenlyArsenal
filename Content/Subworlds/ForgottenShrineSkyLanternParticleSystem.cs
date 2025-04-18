using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Core.Graphics.FastParticleSystems;
using System;
using System.Runtime.CompilerServices;
using Terraria.ModLoader;
using MatrixSIMD = System.Numerics.Matrix4x4;
using Vector2SIMD = System.Numerics.Vector2;

namespace HeavenlyArsenal.Content.Subworlds;

[Autoload(Side = ModSide.Client)]
public class ForgottenShrineSkyLanternParticleSystem : FastParticleSystem
{
    public ForgottenShrineSkyLanternParticleSystem(int maxParticles, Action renderPreparations, ParticleUpdateAction extraUpdates = null) :
        base(maxParticles, renderPreparations, extraUpdates)
    { }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "particles")]
    private extern static ref FastParticle[] GetParticles(FastParticleSystem system);

    protected override void PopulateVertexBufferIndex(VertexPosition2DColorTexture[] vertices, int particleIndex)
    {
        ref FastParticle particle = ref GetParticles(this)[particleIndex];

        Color color = particle.Active ? particle.Color : Color.Transparent;
        Vector2SIMD center = Unsafe.As<Vector2, Vector2SIMD>(ref particle.Position);
        Vector2SIMD size = Unsafe.As<Vector2, Vector2SIMD>(ref particle.Size);
        MatrixSIMD particleRotationMatrix = MatrixSIMD.CreateRotationX(particle.Rotation * 1.1f) *
                                            MatrixSIMD.CreateRotationY(particle.Rotation * 0.5f) *
                                            MatrixSIMD.CreateRotationZ(particle.Rotation);

        Vector2SIMD topLeftPosition = center + Vector2SIMD.Transform(topLeftOffset * size, particleRotationMatrix);
        Vector2SIMD topRightPosition = center + Vector2SIMD.Transform(topRightOffset * size, particleRotationMatrix);
        Vector2SIMD bottomLeftPosition = center + Vector2SIMD.Transform(bottomLeftOffset * size, particleRotationMatrix);
        Vector2SIMD bottomRightPosition = center + Vector2SIMD.Transform(bottomRightOffset * size, particleRotationMatrix);

        int totalFrames = 4;
        int frameY = particleIndex % totalFrames;
        float topY = frameY / (float)totalFrames;
        float bottomY = (frameY + 1f) / totalFrames;

        int vertexIndex = particleIndex * 4;
        vertices[vertexIndex] = new VertexPosition2DColorTexture(Unsafe.As<Vector2SIMD, Vector2>(ref topLeftPosition), color, Vector2.UnitY * topY, frameY);
        vertices[vertexIndex + 1] = new VertexPosition2DColorTexture(Unsafe.As<Vector2SIMD, Vector2>(ref topRightPosition), color, new Vector2(1f, topY), frameY);
        vertices[vertexIndex + 2] = new VertexPosition2DColorTexture(Unsafe.As<Vector2SIMD, Vector2>(ref bottomRightPosition), color, new Vector2(1f, bottomY), frameY);
        vertices[vertexIndex + 3] = new VertexPosition2DColorTexture(Unsafe.As<Vector2SIMD, Vector2>(ref bottomLeftPosition), color, Vector2.UnitY * bottomY, frameY);
    }
}
