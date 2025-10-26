using CalamityMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace HeavenlyArsenal.Core.Systems
{
    internal class LineAlgorithm
    {
        public static bool RaytraceTo(int x0, int y0, int x1, int y1, bool ignoreHalfTiles = false)
        {
            //Bresenham's algorithm
            int horizontalDistance = Math.Abs(x1 - x0); //Delta X
            int verticalDistance = Math.Abs(y1 - y0); //Delta Y
            int horizontalIncrement = (x1 > x0) ? 1 : -1; //S1
            int verticalIncrement = (y1 > y0) ? 1 : -1; //S2

            int x = x0;
            int y = y0;
            int E = horizontalDistance - verticalDistance;

            while (true)
            {
                if (Main.tile[x, y].IsTileSolid() && (!ignoreHalfTiles || !Main.tile[x, y].IsHalfBlock))
                    return false;

                if (x == x1 && y == y1)
                    return true;

                int E2 = E * 2;
                if (E2 >= -verticalDistance)
                {
                    if (x == x1)
                        return true;
                    E -= verticalDistance;
                    x += horizontalIncrement;
                }
                if (E2 <= horizontalDistance)
                {
                    if (y == y1)
                        return true;

                    E += horizontalDistance;
                    y += verticalIncrement;
                }
            }
        }
    }
}
