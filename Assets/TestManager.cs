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
    
    // Food
    [SerializeField] public List<Entity> eliteFood;
    [SerializeField] public List<Entity> weakFood;
    [SerializeField] public float eliteFoodAmount = 1000;
    [SerializeField] public float weakFoodAmount = 100;
    [SerializeField] public List<Entity> reproFood;
    
    // Herbivore
    [SerializeField] public List<Entity> eliteHerb;
    [SerializeField] public List<Entity> weakHerb;
    [SerializeField] public float eliteHerbAmount = 1000;
    [SerializeField] public float weakHerbAmount = 100;

    [SerializeField] private float updateTimer = 10;
    public System.Random random;
    private string path;
    private string path0;

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
            entities.Add(gameObject.AddComponent<Herbivore>());
        }

        path = Application.dataPath + "/survivalInfo.txt";
        path0 = Application.dataPath + "/statInfo.txt";
        
        CreateText(path);
        CreateText(path0);
    }

    void Update()
    {
        List<Entity> toRemove = entities.FindAll(e => !e.IsAlive());

        entities.RemoveAll(e => !e.IsAlive());

        foreach (Entity e in toRemove)
        {
            if (e.GetEntType() == Entity.EntityType.food) deadFood++;
            if (e.GetEntType() == Entity.EntityType.herbivore) deadHerb++;

            Destroy(e);
        }

        numOfFood = 0;
        numOfHerb = 0;
        foreach (Entity e in entities)
        {
            switch (e.GetEntType())
            {
                case Entity.EntityType.food:
                    numOfFood++;
                    break;
                case Entity.EntityType.herbivore:
                    numOfHerb++;
                    break;
                case Entity.EntityType.carnivore:
                    break;
                case Entity.EntityType.omnivore:
                    break;
            }
            
        }
        
    }

    private void FixedUpdate()
    {
        if (groundEnergy < 0) groundEnergy = 0;
        if (groundEnergy > 10000) groundEnergy = 10000;

        updateTimer -= Time.deltaTime;

        eliteFood.Clear();
        reproFood.Clear();
        weakFood.Clear();

        if (updateTimer < 0)
        {
            entities.RemoveAll(e => !e.IsAlive());
            updateTimer = 5;

            int averageFood = 0;
            int averageHerb = 0;
            foreach (Entity e in entities)
            {
                if (e.GetEntType() == Entity.EntityType.food)
                {
                    eliteFoodAmount = e.GetEnergyCur() * 0.8f;
                    weakFoodAmount = e.GetEnergyCur() * 0.2f;
                    averageFood += e.GetEnergyCur();
                    numOfFood++;
                }

                if (e.GetEntType() == Entity.EntityType.herbivore)
                {
                    eliteHerbAmount = e.GetEnergyCur() * 0.8f;
                    weakHerbAmount = e.GetEnergyCur() * 0.2f;
                    averageHerb += e.GetEnergyCur();
                    numOfHerb++;
                    
                }
            }

            averageFood /= numOfFood;
            //averageHerb /= numOfHerb;
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

                if (e.GetEntType() == Entity.EntityType.herbivore)
                {
                    if (e.GetEnergyCur() >= eliteFoodAmount && e.IsAlive())
                    {
                        eliteHerb.Add(e);
                    }
                    else if (e.GetEnergyCur() <= weakHerbAmount && e.IsAlive())
                    {
                        weakHerb.Add(e);
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
                }
            }

            foreach (Food f in reproFood)
            {
                if (f.GetEnergyCur() > f.GetEnergyStart())
                {
                    groundEnergy -= 250;
                    f.NightReproduce(reproFood[random.Next(reproFood.Count - 1)]);
                }
            }

            foreach (Herbivore h in eliteHerb)
            {
                if (h.GetEnergyCur() > 500)
                {
                    groundEnergy -= 250;
                    if (h != null)
                    {
                        h.NightReproduce(eliteHerb[random.Next(eliteHerb.Count - 1)]);
                        h.NightReproduce(eliteHerb[random.Next(eliteHerb.Count - 1)]);
                    }
                }
            }

            foreach (Food f in weakFood)
            {
                f.SetAlive(false);
            }

            File.AppendAllText(path, "\nElite (Pop: " + eliteFood.Count.ToString()
                                                      + ") Reaching " + eliteFoodAmount.ToString().ToString());

            File.AppendAllText(path, "\nWeak (Pop: " + weakFood.Count.ToString() 
                                                    + ") Reaching " + weakFoodAmount.ToString().ToString() + "\n");

            Entity.EventWeight averageEWFood = new Entity.EventWeight(0, 0, 0);
            foreach (Food f in entities)
            {
                averageEWFood.actions[(int)Entity.ActionType.eat] += 
                    f.GetTargEW(f).actions[(int)Entity.ActionType.eat];
                
                averageEWFood.actions[(int)Entity.ActionType.fight] += 
                    f.GetTargEW(f).actions[(int)Entity.ActionType.fight];
                
                averageEWFood.actions[(int)Entity.ActionType.hide] += 
                    f.GetTargEW(f).actions[(int)Entity.ActionType.hide];
            }

            averageEWFood.actions[(int) Entity.ActionType.eat] /= numOfFood;
            averageEWFood.actions[(int) Entity.ActionType.fight] /= numOfFood;
            averageEWFood.actions[(int) Entity.ActionType.hide] /= numOfFood;
            
            
            File.AppendAllText(path0, "Average Food stats: " 
                                      + averageEWFood.actions[(int)Entity.ActionType.eat].ToString() + " | "
                                      + averageEWFood.actions[(int)Entity.ActionType.fight].ToString() + " | "
                                      +  averageEWFood.actions[(int)Entity.ActionType.hide].ToString() + "\n");
            
            numOfFood = 0;
            numOfHerb = 0;
            deadFood = 0;
            deadHerb = 0;
        }
    }
    void CreateText(string givenPath)
    {
        givenPath = givenPath;

        if (!File.Exists(givenPath))
        {
            File.WriteAllText(givenPath, "Test:\n\n");
        }
    }
    public List<Entity> GetAllEnts()
    {
        return entities;
    }
}