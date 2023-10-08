using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class IncrementalPerlinBrush : TerrainBrush {

    public float detailScale = 100f;
    public float noiseHeight = 1f;
    public override void draw(int x, int z) {
        for (int zi = -radius; zi <= radius; zi++) {
            for (int xi = -radius; xi <= radius; xi++) {
                float hloc = terrain.get(x + xi, z + zi);
                float deltaH = noiseHeight * (Mathf.PerlinNoise((float)Math.Cos(z) + (float)xi / radius * detailScale, (float)Math.Cos(x) + (float)zi / radius * detailScale));
                deltaH *= sigmoid(xi + radius -5.0f) * sigmoid(zi + radius - 5.0f) * sigmoid(-xi + radius - 5.0f) * sigmoid(-zi + radius - 5.0f);
                terrain.set(x + xi, z + zi, hloc + deltaH);
            }
        }
    }
    private static float sigmoid(float x){
        return 1.0f / (1.0f + (float)Math.Exp(-x));
    }
}
