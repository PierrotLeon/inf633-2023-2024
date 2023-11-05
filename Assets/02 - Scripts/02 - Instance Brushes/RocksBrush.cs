using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocksBrush : InstanceBrush {

    public override void draw(float x, float z) {
        for (int i = 0; i < radius * radius / 2; ++i) {
            terrain.changeRock();
            float r1 = Random.Range(-1.0f, 1.0f);
            float r2 = Random.Range(-1.0f, 1.0f);
            spawnObject(x + r1 * radius, z + r2 * radius);
        }
    }
}