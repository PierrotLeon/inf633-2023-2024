using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GaussianBrush : TerrainBrush {

    public float height = 5;
    public float standardDeviation = 1;
    public override void draw(int x, int z) {
        for (int zi = -radius; zi <= radius; zi++) {
            for (int xi = -radius; xi <= radius; xi++) {
                float hloc = terrain.get(x + xi, z + zi);
                float h = height * (float)Math.Pow((double)2.0, -(xi * xi + zi * zi) / standardDeviation);
                terrain.set(x + xi, z + zi, hloc + (int)h);
            }
        }
    }
}
