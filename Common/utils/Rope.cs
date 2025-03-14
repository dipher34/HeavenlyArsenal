using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace HeavenlyArsenal.Common.utils;

public class Rope
{
    public class RopeSegment
    {
        public RopeSegment(Vector2 position)
        {
            this.position = position;
            this.oldPosition = position;
        }

        public Vector2 position;
        public Vector2 oldPosition;
        public Vector2 velocity;
        public bool pinned;
    }

    public Rope(Vector2 startPos, Vector2 endPos, int segmentCount, float segmentLength, Vector2 gravity, int accuracy = 10)
    {
        segments = new RopeSegment[segmentCount];
        for (int i = 0; i < segmentCount; i++)
        {
            Vector2 segmentPos = Vector2.Lerp(startPos, endPos, i / (segmentCount - 1f));
            segments[i] = new RopeSegment(segmentPos);
        }
        segments[0].pinned = true;
        segments[^1].pinned = true;

        this.segmentLength = segmentLength;
        this.gravity = gravity;
        this.accuracy = accuracy;
    }

    public void Settle()
    {
        float oldDamp = damping;
        damping = 0.67f;
        for (int a = 0; a < segments.Length; a++)
            Update();

        damping = oldDamp;
    }

    public RopeSegment[] segments;
    public float segmentLength;
    public Vector2 gravity;

    public bool tileCollide;
    public Vector2 colliderOrigin;
    public int colliderWidth;
    public int colliderHeight;
    public float damping;

    private int accuracy;

    public void Update()
    {
        for (int i = 0; i < segments.Length; i++)
        {
            segments[i].velocity = (segments[i].position - segments[i].oldPosition) * (1f - damping);
            if (segments[i].velocity.Length() < 0.015f)
                segments[i].velocity = Vector2.Zero;

            segments[i].oldPosition = segments[i].position;

            if (!segments[i].pinned)
                segments[i].position += TileCollision(segments[i].position, segments[i].velocity + gravity);
        }

        for (int a = 0; a < accuracy; a++)
            Constrain();
    }

    public void Constrain()
    {
        for (int i = 0; i < segments.Length - 1; i++)
        {
            float dist = segments[i].position.Distance(segments[i + 1].position);
            float error = dist - segmentLength;
            Vector2 correction = segments[i].position.DirectionFrom(segments[i + 1].position) * error;

            bool pinned = segments[i].pinned;
            bool nextPinned = segments[i + 1].pinned;
            float multiplier = pinned || nextPinned ? 1f : 0.5f;

            if (!pinned)
                segments[i].position -= TileCollision(segments[i].position, correction * multiplier);
            if (!nextPinned)
                segments[i + 1].position += TileCollision(segments[i + 1].position, correction * multiplier);
        }
    }

    private Vector2 TileCollision(Vector2 position, Vector2 velocity)
    {
        if (!tileCollide)
            return velocity;

        Vector2 newVelocity = Collision.noSlopeCollision(position + colliderOrigin, velocity, colliderWidth, colliderHeight + 2, true, true);
        newVelocity = Collision.noSlopeCollision(position + colliderOrigin, newVelocity, colliderWidth, colliderHeight, true, true);
        Vector2 result = velocity;
        if (Math.Abs(velocity.X) > Math.Abs(newVelocity.X))
            result.X = 0;
        if (Math.Abs(velocity.Y) > Math.Abs(newVelocity.Y))
            result.Y = 0;

        return result;
    }

    public Vector2[] GetPoints()
    {
        Vector2[] points = new Vector2[segments.Length];
        for (int i = 0; i < segments.Length; i++)
            points[i] = segments[i].position;

        return points;
    }

    public Rectangle GetCollisionRect(int i) => new Rectangle((int)(segments[i].position.Floor().X + colliderOrigin.X), (int)(segments[i].position.Floor().Y + colliderOrigin.Y), colliderWidth, colliderHeight);
}