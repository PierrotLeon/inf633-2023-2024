using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MagicBrush : TerrainBrush {

    public enum BrushType
    {
        SQUARE,
        CIRCLE,
        NORMAL,
        NOISE,
        SMOOTH
    };

    public float height = 5;

    public BrushType type;

    public override void draw(int x, int z) {
        float actualHeight;

        for (int zi = -radius; zi <= radius; zi++) {
            for (int xi = -radius; xi <= radius; xi++) {
                actualHeight = terrain.get(x + xi, z + zi);
                switch (type)
                {
                    case BrushType.SQUARE:
                        terrain.set(x + xi, z + zi, height + actualHeight);
                        break;

                    case BrushType.CIRCLE:
                        if (xi * xi + zi * zi <= radius * radius)
                        {
                            terrain.set(x + xi, z + zi, height + actualHeight);
                        }
                        break;

                    case BrushType.NORMAL:
                        float standardDeviation = radius/2;
                        float distanceSquared = xi * xi + zi * zi;
                        float gaussianValue = Mathf.Exp(-distanceSquared / (2 * standardDeviation * standardDeviation));
                        float newheight = gaussianValue * height + actualHeight;
                        terrain.set(x + xi, z + zi, (int)newheight);
                        break;

                    case BrushType.NOISE:
                        float randomFloat = UnityEngine.Random.Range(0.0f, height) + actualHeight;
                        terrain.set(x + xi, z + zi, (int)randomFloat);
                        break;

                    case BrushType.SMOOTH:
                        float hlocprevx = terrain.get(x + xi - 1, z + zi);
                        float hlocnextx = terrain.get(x + xi + 1, z + zi);
                        float hlocprevz = terrain.get(x + xi, z + zi - 1);
                        float hlocnextz = terrain.get(x + xi, z + zi + 1);
                        float smoothheight = (hlocprevx + hlocnextx + hlocprevz + hlocnextz) / 4;
                        terrain.set(x + xi, z + zi, smoothheight);
                        break;

                }
                
            }
        }
    }
}
