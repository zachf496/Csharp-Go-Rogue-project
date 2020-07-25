﻿using GoRogue.GameFramework;
using GoRogue.MapViews;

namespace GoRogue.Debugger.Extensions
{
    public static class MapExtensions
    {
        public static ArrayMap<char> CharMap(this Map map)
        {
            ArrayMap<char> chars = new ArrayMap<char>(map.Width, map.Height);
            for (int i = 0; i < map.Width; i++)
            {
                for (int j = 0; j < map.Height; j++)
                {
                    IGameObject? obj = map.GetTerrainAt((i, j));
                    if (obj != null)
                        chars[obj.Position] = obj.IsTransparent ?
                            obj.IsWalkable ? '.' : '+' : //transparent
                            obj.IsWalkable ? '#' : '"'; //opaque
                    else
                        chars[(i, j)] = ' ';
                }
            }
            // foreach (IGameObject gameObject in map.Terrain)
            // {
            //     chars[gameObject.Position] = gameObject.IsTransparent ? gameObject.IsWalkable ? '.' : '+' : '"';;
            // }

            return chars;
        }
    }
}
