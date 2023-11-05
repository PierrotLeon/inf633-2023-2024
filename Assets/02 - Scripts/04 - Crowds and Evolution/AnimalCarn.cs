using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class AnimalCarn : MonoBehaviour
{

    [Header("Animal parameters")]
    public float swapRate = 0.01f;
    public float mutateRate = 0.01f;
    public float swapStrength = 10.0f;
    public float mutateStrength = 0.5f;
    public float maxAngle = 10.0f;

    [Header("Energy parameters")]
    public float maxEnergy = 100.0f;
    public float lossEnergy = 0.1f;
    public float gainEnergy = 10.0f;
    private float energy;

    [Header("Sensor - Vision")]
    public float maxVision = 20.0f;
    public float stepAngle = 10.0f;
    public int nEyes = 5;

    private int[] networkStruct;
    private SimpleNeuralNet brain = null;

    // Terrain.
    private CustomTerrain terrain = null;
    private int[,] details = null;
    private Vector2 detailSize;
    private Vector2 terrainSize;

    // Animal.
    private Transform tfm;
    private float[] vision;

    // Genetic alg.
    private GeneticCarn genetic_carn = null;
    private GeneticAlgo genetic_algo = null;

    // Renderer.
    private Material mat = null;

    private int isHunting = 0;
    public Transform target;
    private List<GameObject> childPrefabs = new List<GameObject>();

    void Start()
    {
        // Network: 1 input per receptor, 1 output per actuator.
        vision = new float[nEyes];
        networkStruct = new int[] { nEyes, 5, 1 };
        energy = maxEnergy;
        tfm = transform;

        // Renderer used to update animal color.
        // It needs to be updated for more complex models.
        MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();
        if (renderer != null)
            mat = renderer.material;
    }

    void Update()
    {
        if (target == null)
        {
            isHunting = 1; // Deactivate hunting
        }

        // In case something is not initialized...
        if (brain == null)
            brain = new SimpleNeuralNet(networkStruct);
        if (terrain == null)
            return;
        UpdateSetup();

        // Retrieve animal location in the heighmap
        int dx = (int)((tfm.position.x / terrainSize.x) * detailSize.x);
        int dy = (int)((tfm.position.z / terrainSize.y) * detailSize.y);

        // For each frame, we lose lossEnergy
        energy -= lossEnergy;


        // 1. Update receptor.
        UpdateVision();

        DrawVisionLines();
        float[] output = new float[0];
        // 2. Use brain or hunt.

        if (isHunting == 2)
        {

            // Look for the direction of the target game object
            Vector3 targetDirection = target.transform.position - transform.position;
            targetDirection.y = 0.0f; // Assuming you want to ignore vertical component
            targetDirection.Normalize();
            output = new float[] { Mathf.Atan2(targetDirection.z, targetDirection.x) / (2 * Mathf.PI) };
            float anglemax = Vector3.Angle(transform.forward, targetDirection);

            // Convert the angle to a value between -1 and 1
            output = new float[] { anglemax / 30};

            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
            float range = 5.0f; // Adjust this value to set your desired range
            GeneticAlgo geneticalgoComponent = terrain.GetComponent<GeneticAlgo>();
            Animal animalComponent = target.GetComponent<Animal>();
            if (distanceToTarget <= range)
            {
                target = null;
                isHunting = 0;
                geneticalgoComponent.removeAnimal(animalComponent);
                UnityEngine.Debug.Log("cumeu");

                energy += gainEnergy;
                if (energy > maxEnergy)
                    energy = maxEnergy;

                genetic_carn.addOffspring(this);
            }
        }
        else if (isHunting == 0)
        {
            // If the animal is located in the dimensions of the terrain and over a grass position (details[dy, dx] > 0), it eats it, gain energy and spawn an offspring.
            if (UnityEngine.Random.Range(1, 100) < 10)
            {
                isHunting = 1;
            }
            if (energy < 0)
            {
                energy = 0.0f;
                genetic_carn.removeAnimal(this);
            }

            output = brain.getOutput(vision);
        }

        else 
        {
            output = brain.getOutput(vision);
        }


            // 3. Act using actuators.
            float angle = (output[0] * 2.0f - 1.0f) * maxAngle;
        tfm.Rotate(0.0f, angle, 0.0f);

    }

    /// <summary>
    /// Calculate distance to the nearest food resource, if there is any.
    /// </summary>
    private void UpdateVision()
    {
        float startingAngle = -((float)nEyes / 2.0f) * stepAngle;
        Vector2 ratio = detailSize / terrainSize;

        for (int i = 0; i < nEyes; i++)
        {
            Quaternion rotAnimal = tfm.rotation * Quaternion.Euler(0.0f, startingAngle + (stepAngle * i), 0.0f);
            Vector3 forwardAnimal = rotAnimal * Vector3.forward;
            float sx = tfm.position.x * ratio.x;
            float sy = tfm.position.z * ratio.y;
            vision[i] = 1.0f;

            RaycastHit hit;
            Vector3 rayDirection = forwardAnimal.normalized;
            if(isHunting == 1)
            {
                bool found = false;
                foreach (GameObject prefab in childPrefabs)
                {
                   
                    Vector3 position1 = new Vector3(transform.position.x, 0, transform.position.z); // Projected position of current object
                    Vector3 position2 = new Vector3(prefab.transform.position.x, 0, prefab.transform.position.z); // Projected position of prefab

                    float distance = Vector3.Distance(position1, position2);

                    if (distance <= 50.0f && prefab != null && prefab.name != "Animal - Carnivorous(Clone)")
                    {
                        target = prefab.transform;
                        isHunting = 2;
                        found = true;
                    }
                }
                if (!found)
                    isHunting = 0;
            }

            // Interate over vision length.
            for (float distance = 1.0f; distance < maxVision; distance += 0.5f)
            {
                // Position where we are looking at.
                float px = (sx + (distance * forwardAnimal.x * ratio.x));
                float py = (sy + (distance * forwardAnimal.z * ratio.y));

                if (px < 0)
                    px += detailSize.x;
                else if (px >= detailSize.x)
                    px -= detailSize.x;
                if (py < 0)
                    py += detailSize.y;
                else if (py >= detailSize.y)
                    py -= detailSize.y;

                if ((int)px >= 0 && (int)px < details.GetLength(1) && (int)py >= 0 && (int)py < details.GetLength(0) && details[(int)py, (int)px] > 0)
                {
                    vision[i] = distance / maxVision;
                    break;
                }
            }

           
        }
    }


    private void DrawVisionLines()
    {
        float startingAngle = -((float)nEyes / 2.0f) * stepAngle;
        for (int i = 0; i < nEyes; i++)
        {
            Quaternion rotAnimal = tfm.rotation * Quaternion.Euler(0.0f, startingAngle + (stepAngle * i), 0.0f);
            Vector3 forwardAnimal = rotAnimal * Vector3.forward;
            UnityEngine.Debug.DrawRay(tfm.position, forwardAnimal * maxVision, Color.green);
        }
    }

    public void Setup(CustomTerrain ct, GeneticCarn ga)
    {
        terrain = ct;
        genetic_carn = ga;
        UpdateSetup();
    }

    private void UpdateSetup()
    {
        detailSize = terrain.detailSize();
        Vector3 gsz = terrain.terrainSize();
        terrainSize = new Vector2(gsz.x, gsz.z);
        details = terrain.getDetails();

        int childCount = terrain.transform.childCount;
        childPrefabs.Clear(); // Clear the list before populating it again

        for (int i = 0; i < childCount; i++)
        {
            Transform child = terrain.transform.GetChild(i);
            if (child != null && child.gameObject != null)
            {
                childPrefabs.Add(child.gameObject);
            }
        }
    }

    public void InheritBrain(SimpleNeuralNet other, bool mutate)
    {
        brain = new SimpleNeuralNet(other);
        if (mutate)
            brain.mutate(swapRate, mutateRate, swapStrength, mutateStrength);
    }
    public SimpleNeuralNet GetBrain()
    {
        return brain;
    }
    public float GetHealth()
    {
        return energy / maxEnergy;
    }



}
