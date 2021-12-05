using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using TMPro;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;

public class TestManager : MonoBehaviour
{
    [SerializeField] private int numOfHerb = 10;
    [SerializeField] private int deadHerb = 0;
    [SerializeField] public int numOfCarn = 10;
    [SerializeField] private int numOfOmni = 10;
    [SerializeField] private int numOfFood = 100;
    [SerializeField] private int deadFood = 0;
    [SerializeField] public int groundEnergy = 1000;
    [SerializeField] public List<Entity> entities;
    [SerializeField] public List<Entity> eliteFood;
    [SerializeField] public List<Entity> weakFood;
    [SerializeField] public float eliteFoodAmount = 1000;
    [SerializeField] public float weakFoodAmount = 100;
    [SerializeField] public List<Entity> eliteHerb;
    [SerializeField] private float updateTimer = 10;
    public System.Random random;
    private string path;

    // Start is called before the first frame update
    void Start()
    {
        random = new System.Random();
        for (int i = 0; i < numOfFood; i++)
        {
            entities.Add(gameObject.AddComponent<Food>());
        }

        for (int i = 0; i < numOfHerb; i++)
        {
            Herbivore x;

            entities.Add(gameObject.AddComponent<Herbivore>());
        }
        
        CreateText();
    }

    void Update()
    {
        List<Entity> toRemove = entities.FindAll(e => !e.IsAlive());

        entities.RemoveAll(e => !e.IsAlive());

        foreach (Entity e in toRemove)
        {
            print(e.GetEntType() + " cause of death: " + e.causeOfDeath);

            if (e.GetEntType() == Entity.EntityType.food) deadFood++;
            if (e.GetEntType() == Entity.EntityType.herbivore) deadHerb++;

            Destroy(e);
        }
    }

    private void FixedUpdate()
    {
        if (groundEnergy < 0) groundEnergy = 0;
        if (groundEnergy > 10000) groundEnergy = 10000;

        updateTimer -= Time.deltaTime;

        eliteFood.Clear();
        weakFood.Clear();

        if (updateTimer < 0)
        {
            updateTimer = 5;

            int averageFood = 0;
            foreach (Entity e in entities)
            {
                if (e.GetEntType() == Entity.EntityType.food)
                {
                    eliteFoodAmount = e.GetEnergyCur() * 0.8f;
                    weakFoodAmount = e.GetEnergyCur() * 0.2f;
                    averageFood += e.GetEnergyCur();
                    numOfFood++;
                }
            }

            averageFood /= numOfFood;
            File.AppendAllText(path, "\nNewGeneration (Died:" + deadFood.ToString() + ")(New Total Pop: "
                                     + numOfFood.ToString() + "):\n" + "Average: " + averageFood.ToString());

            foreach (Entity e in entities)
            {
                if (e.GetEntType() == Entity.EntityType.food)
                {
                    if (e.GetEnergyCur() >= eliteFoodAmount)
                    {
                        eliteFood.Add(e);
                    }
                    else if (e.GetEnergyCur() <= weakFoodAmount)
                    {
                        weakFood.Add(e);
                    }
                }

                e.ResetMoves();
            }

            foreach (Food f in eliteFood)
            {
                if (f.GetEnergyCur() > 500)
                {
                    groundEnergy -= 250;
                    f.NightReproduce(eliteFood[random.Next(eliteFood.Count - 1)]);
                    f.NightReproduce(eliteFood[random.Next(eliteFood.Count - 1)]);
                }
            }

            foreach (Food f in weakFood)
            {
                //f.SetAlive(false);
            }

            File.AppendAllText(path, "\nElite (Pop: " + eliteFood.Count.ToString()
                                                      + ") Reaching " + eliteFoodAmount.ToString().ToString());

           //File.AppendAllText(path, "\nWeak (Pop: " + weakFood.Count.ToString()
           //                                          + ") Reaching " + weakFoodAmount.ToString().ToString() + "\n");

            numOfFood = 0;
            deadFood = 0;
            deadHerb = 0;
        }
    }
    void CreateText()
    {
        path = Application.dataPath + "/Log01.txt";

        if (!File.Exists(path))
        {
            File.WriteAllText(path, "Test:\n\n");
        }
    }
    public List<Entity> GetAllEnts()
    {
        return entities;
    }
}