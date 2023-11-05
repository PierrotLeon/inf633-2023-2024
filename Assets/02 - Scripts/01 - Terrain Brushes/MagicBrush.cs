using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MagicBrush : TerrainBrush {

    public enum BrushType
    {
        ERASE,
        SIMPLE,
        INCREMENTAL,
        NORMAL,
        NOISE,
        SMOOTH,
        EROSION,
        PERLIN_FIX_HEIGHT,
        PERLIN,
        TEXTURE,

    };

    public enum BrushShape
    {
        SQUARE,
        CIRCLE,
    };

    public float height = 5;
    public int max_fractal_level = 5;
    public int min_fractal_level = 0;
    public float detailScale = 100f;
    public int fractal_level = 5;
    public float erosionFactor = 0.0001f;
    public float hColorChange = 50f;

    public BrushType type;
    public BrushShape shape;

    public override void draw(int x, int z) {
        float actualHeight;

        for (int zi = -radius; zi <= radius; zi++) {
            for (int xi = -radius; xi <= radius; xi++) {
                if (shape == BrushShape.CIRCLE && xi * xi + zi * zi > radius * radius) {
                    continue;
                }

                actualHeight = terrain.get(x + xi, z + zi);
                switch (type)
                {
                    case BrushType.SIMPLE:
                        terrain.set(x + xi, z + zi, height);
                        break;

                    case BrushType.INCREMENTAL:
                        terrain.set(x + xi, z + zi, height + actualHeight);
                        break;

                    case BrushType.NORMAL:
                        float standardDeviation = radius/2;
                        float distanceSquared = xi * xi + zi * zi;
                        float gaussianValue = Mathf.Exp(-distanceSquared / (2 * standardDeviation * standardDeviation));
                        float newheight = gaussianValue * height + actualHeight;
                        terrain.set(x + xi, z + zi, newheight);
                        break;

                    case BrushType.NOISE:
                        float randomFloat = UnityEngine.Random.Range(0.0f, height) + actualHeight;
                        terrain.set(x + xi, z + zi, randomFloat);
                        break;

                    case BrushType.SMOOTH:
                        float hlocprevx = terrain.get(x + xi - 1, z + zi);
                        float hlocnextx = terrain.get(x + xi + 1, z + zi);
                        float hlocprevz = terrain.get(x + xi, z + zi - 1);
                        float hlocnextz = terrain.get(x + xi, z + zi + 1);
                        float smoothheight = (hlocprevx + hlocnextx + hlocprevz + hlocnextz) / 4;
                        terrain.set(x + xi, z + zi, smoothheight);
                        break;

                    case BrushType.ERASE:
                        terrain.set(x + xi, z + zi, 0);
                        break;

                    case BrushType.EROSION:                        
                        hlocprevx = terrain.get(x + xi - 1, z + zi);
                        hlocnextx = terrain.get(x + xi + 1, z + zi);
                        hlocprevz = terrain.get(x + xi, z + zi - 1);
                        hlocnextz = terrain.get(x + xi, z + zi + 1);
                        float h = (hlocprevx + hlocnextx + hlocprevz + hlocnextz) / 4;
                        float steepness = (terrain.getSteepness(x + xi + 1, z + zi) + terrain.getSteepness(x + xi - 1, z + zi) + terrain.getSteepness(x + xi, z + zi + 1) + terrain.getSteepness(x + xi, z + zi - 1)) / 4.0f;
                        terrain.set(x + xi, z + zi, h - erosionFactor * steepness * steepness);
                        break;

                    case BrushType.PERLIN_FIX_HEIGHT:
                        float deltaH = 0;
                        for (int k = 0; k < fractal_level; k++)
                        {
                            deltaH += height * (float)Math.Pow(1.5, -k) * Mathf.PerlinNoise((float)Math.Pow(2, k) * (float)(x + xi) / detailScale, (float)Math.Pow(2, k) * (float)(z + zi) / detailScale);
                        }
                        terrain.set(x + xi, z + zi, deltaH);
                        break;

                    case BrushType.PERLIN:
                        deltaH = 0;
                        for (int k = min_fractal_level; k < max_fractal_level; k++)
                        {
                            deltaH += height * (float)Math.Pow(1.5, -k) * Mathf.PerlinNoise((float)Math.Pow(2, k) * (float)(x + xi) / detailScale, (float)Math.Pow(2, k) * (float)(z + zi) / detailScale);
                        }
                        deltaH *= bell(xi, zi, radius);
                        terrain.set(x + xi, z + zi, actualHeight + deltaH);
                        break;

                    case BrushType.TEXTURE:
                        // this brush is set for use with four terrain layers, rocks, grass, cliff and water
                        float angle = terrain.getSteepness(x + xi, z + zi);
                        float afrac = (float)Math.Pow(angle / 90.0f, 2f);
                        float hfrac = Math.Min(actualHeight, hColorChange) / hColorChange;
                        float[] valeurs = { hfrac * (1f - afrac), (1f - hfrac) * (1f - afrac), afrac, 0f };
                        if (actualHeight == 0f)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                valeurs[i] = 0f;
                            }
                            valeurs[3] = 1f;
                        }
                        terrain.setTextures(x + xi, z + zi, valeurs);
                        break;
                }
            }
        }
    }

    private static float bell(float x, float z, int radius)
    {
        return 1.0f / ((float)Math.Exp(((float)Math.Pow(x, 2) + (float)Math.Pow(z, 2)) / (float)Math.Pow(radius, 2)));
    }
}
