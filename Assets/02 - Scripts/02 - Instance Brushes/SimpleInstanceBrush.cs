using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleInstanceBrush : InstanceBrush {

    public override void draw(float x, float z) {
        // print(terrain.rocks.Length);
        // print(Mathf.Max(terrain.max_scale, terrain.min_scale));
        for (int i = 0; i < radius * radius; ++i) {
            float r1 = Random.Range(-1.0f, 1.0f);
            float r2 = Random.Range(-1.0f, 1.0f);
            int num = CustomTerrain.rnd.Next(0, terrain.rocks.Length - 1);
            terrain.object_prefab = terrain.rocks[num];
            spawnObject(x + r1 * radius, z + r2 * radius);
        }
    }
}
