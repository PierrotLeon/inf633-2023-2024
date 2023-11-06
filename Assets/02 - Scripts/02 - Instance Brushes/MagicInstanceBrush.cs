using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class MagicInstanceBrush : InstanceBrush {

    public enum BrushType {
        Create,
        CopyPaste,
        Rocks
    }

    public BrushType type;

    public bool Altitude = true;
    public bool Steepness = true;
    public bool RandomPlacement = true;
    public bool RandomTree = true;
    public bool MinDistance = true;
    public float minDistanceValue = 5;
    public float maxHeightValue = 50;
    public float maxSteepnessValue = 100;

    private List<float> xpos = new List<float> { 0 };
    private List<float> zpos = new List<float> { 0 };
    public float MinimalDistance = 1.0f;

    public override void draw(float x, float z) {
        switch(type) {
            case BrushType.Create:
                create(x, z);
                return;
            
            case BrushType.CopyPaste:
                copypaste(x, z);
                return;

            case BrushType.Rocks:
                putrocks(x, z);
                return;
        }
    }

    private void create(float x, float z) {
        float dx = 0;
        float dz = 0;
        float scaler = 1;
        System.Random rand = new System.Random();

        if (RandomPlacement) {
            dx = (float)((rand.NextDouble() - 0.5) * 2 * radius);
            dz = (float)((rand.NextDouble() - 0.5) * 2 * radius);
        }

        if (RandomTree) {
            terrain.changeTree();
        }

        // don't spawn if too close to other tree
        if (MinDistance) {
            for (int i = 0; i < terrain.getObjectCount(); i++)
            {
                Vector3 loc = terrain.getObjectLoc(i);
                float xloc = loc.x;
                float zloc = loc.z;
                if (dist(x + dx, z + dz, xloc, zloc) < minDistanceValue)
                {
                    return;
                }
            }
        }

        // don't spawn if terrain is too steep
        if (Steepness) {
            float steepness = terrain.getSteepness(x + dx, z + dz);
            if (steepness > maxSteepnessValue) {
                return;
            }
        }

        if (Altitude) {
            float h = terrain.get(x + dx, z + dz);

            float hChoice = 1f / (1f + (float)Math.Exp(-h / maxHeightValue));
            float rdChoice = (float)rand.NextDouble();

            scaler = 1f - h / 2f / maxHeightValue;
            if (hChoice >= rdChoice)
            {
                return;
            }
        }

        spawnObjectScaled(x + dx, z + dz, scaler);
    }

    // left shift + click to copy, then click to paste
    private void copypaste(float x, float z) {
        update(x, z, radius);
        for (int i = 0; i < xpos.Count; i++) {

            bool spwn = true;
            for (int k = 0; k < terrain.getObjectCount(); k++) {
                Vector3 loc = terrain.getObjectLoc(k);
                float xloc = loc.x;
                float zloc = loc.z;
                if (dist(x + xpos[i], z + zpos[i], xloc, zloc) < MinimalDistance) {
                    spwn = false;
                }

            }

            if (spwn) {
                spawnObject(x + xpos[i], z + zpos[i]);
            }
        }
    }

    void update(float x, float z, float radius) {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            // Left Shift is currently pressed down (and we are clicking)
            xpos.Clear();
            zpos.Clear();
            Vector3 loc;
            float xloc;
            float zloc;
            for (int i = 0; i < terrain.getObjectCount(); i++)
            {
                loc = terrain.getObjectLoc(i);
                xloc = loc.x;
                zloc = loc.z;
                if ((xloc < x + radius) & (xloc > x - radius) & (zloc > z - radius) & (zloc < z + radius))
                {
                    xpos.Add(xloc - x);
                    zpos.Add(zloc - z);
                }
            }
        }
    }
    
    private static float dist(float x1, float z1, float x2, float z2)
    {
        return (float)Math.Pow(Math.Pow(x1 - x2, 2) + Math.Pow(z1 - z2, 2), 0.5f);
    }

    private void putrocks(float x, float z) {
        for (int i = 0; i < radius * radius / 2; ++i) {
            terrain.changeRock();
            float r1 = UnityEngine.Random.Range(-1.0f, 1.0f);
            float r2 = UnityEngine.Random.Range(-1.0f, 1.0f);
            spawnObject(x + r1 * 2 * radius, z + r2 * 2 * radius);
        }
    }
}
