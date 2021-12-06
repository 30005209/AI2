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
    [SerializeField] private int numOfHerb = 50;
    [SerializeField] private int deadHerb = 0;
    [SerializeField] public int numOfCarn = 10;
    [SerializeField] private int numOfOmni = 10;
    [SerializeField] private int numOfFood = 50;
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
    [SerializeField] public List<Entity> reproHerb;

    [SerializeField] private float updateTimer = 10;
    public System.Random random;
    private string pathFInfo;
    private string pathFStat;
    private string pathHInfo;
    private string pathHStat;
    
    int averageFood = 0;
    int averageHerb = 0;
    
    Entity.EventWeight foodEWfood = new Entity.EventWeight(0, 0, 0);
    Entity.EventWeight foodEWHerb = new Entity.EventWeight(0, 0, 0);
    Entity.EventWeight herbEWHerb = new Entity.EventWeight(0, 0, 0);
    Entity.EventWeight herbEWfood = new Entity.EventWeight(0, 0, 0);

    private int HerbivoreIntroCount = 0;
    private bool HerbivoreIntro = false;
    
    
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

        pathFInfo = Application.dataPath + "/fSurvivalInfo.txt";
        pathFStat = Application.dataPath + "/fStatInfo.txt";
        pathHStat = Application.dataPath + "/hSurvivalInfo.txt";
        pathHInfo = Application.dataPath + "/hStatInfo.txt";

        CreateText(pathFInfo);
        CreateText(pathFStat);
        CreateText(pathHStat);
        CreateText(pathHInfo);
    }

    void ReintroducePopulation(Entity.EntityType type)
    {
        for (int i = 0; i < 100; i++)
        {
            switch (type)
            {
                case Entity.EntityType.food:
                    entities.Add(gameObject.AddComponent<Food>());
                    break;
                case Entity.EntityType.herbivore:
                    entities.Add(gameObject.AddComponent<Herbivore>());
                    break;
                case Entity.EntityType.carnivore:
                    break;
                case Entity.EntityType.omnivore:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            
        }
    }
    
    
    void Update()
    {
        List<Entity> toRemove = entities.FindAll(e => !e.IsAlive());

        entities.RemoveAll(e => !e.IsAlive());

        foreach (Entity e in toRemove)
        {
            print(e.GetEntType().ToString() + " - " + e.causeOfDeath);
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

    private void ResetInfo()
    {
        if (groundEnergy < 0) groundEnergy = 0;
        if (groundEnergy > 10000) groundEnergy = 10000;
        
        eliteFood.Clear();
        reproFood.Clear();
        reproHerb.Clear();
        weakFood.Clear();
    }

    private void UpdateElites()
    {
        // Get Elites
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
                

            File.AppendAllText(pathFStat, "\nElite (Pop: " + eliteFood.Count.ToString()
                                                           + ") Reaching " + eliteFoodAmount.ToString().ToString());
        }
            
        // Elite Reproduction Food
        foreach (Food f in eliteFood)
        {
            if (f.GetEnergyCur() > 500 & this != null)
            {
                Entity e = null;

                while (e == null)
                {
                    e = eliteFood[random.Next(eliteFood.Count)];
                }

                groundEnergy -= 250;
                
                f.NightReproduce(e);
            }
        }
        

        // Elite Reproduction Herbivore
        foreach (Herbivore h in eliteHerb)
        {
            if (h.GetEnergyCur() > 500 && this != null)
            {
                Entity e = null;

                while (e == null)
                {
                    e = eliteHerb[random.Next(eliteHerb.Count)];
                }

                h.ChangeEnergyLevel(-250);
                h.NightReproduce(e);
            }
        }

    }

    private void UpdateEntityInfo()
    {
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

        if (numOfFood != 0) { averageFood /= numOfFood; }
        if (numOfHerb != 0) { averageHerb /= numOfHerb; }


        File.AppendAllText(pathFStat, "\nNewGeneration (Died:" + deadFood.ToString() + ")(New Total Pop: "
                                      + numOfFood.ToString() + "):\n" + "Average: " + averageFood.ToString());

        File.AppendAllText(pathHInfo, "\nNewGeneration (Died:" + deadHerb.ToString() + ")(New Total Pop: "
                                      + numOfHerb.ToString() + "):\n" + "Average: " + averageHerb.ToString());
        
    }

    private void ReproduceSuccessful()
    {
        // Reproduction Successful Food
        foreach (Food f in reproFood)
        {
            if (f.GetEnergyCur() > f.GetEnergyStart() && reproFood.Count < 800)
            {
                groundEnergy -= 250;

                f.NightReproduce(reproFood[random.Next(reproFood.Count - 1)]);
            }
        }

        // Reproduction Successful Herb
        foreach (Herbivore h in reproHerb)
        {
            if (h.GetEnergyCur() > h.GetEnergyStart() && reproHerb.Count < 800)
            {
                groundEnergy -= 250;
                h.NightReproduce(reproHerb[random.Next(reproHerb.Count - 1)]);
            }
        }
    }

    private void UpdateWeak()
    {
        foreach (Food f in weakFood)
        {
            f.SetAlive(false);
            f.causeOfDeath = "Weak";
        }
        
        File.AppendAllText(pathFStat, "\nWeak (Pop: " + weakFood.Count.ToString() 
                                                      + ") Reaching " + weakFoodAmount.ToString().ToString() + "\n");
    }

    private void UpdateAverageStats()
    {
        foreach (Entity e in entities)
        {
            if( e.GetEntType() == Entity.EntityType.food)
            {
                foodEWfood += e.GetEW(Entity.EntityType.food);
                foodEWHerb += e.GetEW(Entity.EntityType.herbivore);
            }
                
            if( e.GetEntType() == Entity.EntityType.herbivore)
            {
                herbEWfood += e.GetEW(Entity.EntityType.food);
                herbEWHerb += e.GetEW(Entity.EntityType.herbivore);
            }
        }

        if (numOfFood != 0)
        {
            foodEWfood /= numOfFood;
            foodEWHerb /= numOfFood;
        }

        if (numOfHerb != 0)
        {
            herbEWfood /= numOfHerb;
            herbEWHerb /= numOfHerb;
        }

        File.AppendAllText(pathFInfo, "Average Food Stats (Food): " + foodEWfood.OutputEWStats());
        File.AppendAllText(pathFInfo, "Average Food Stats (Herb): " + foodEWHerb.OutputEWStats());
        File.AppendAllText(pathHStat, "Average Herbivore Stats (Food): " + herbEWfood.OutputEWStats());
        File.AppendAllText(pathHStat, "Average Herbivore Stats (Herb): " + herbEWHerb.OutputEWStats());
    }
    
    private void FixedUpdate()
    {
        updateTimer -= Time.deltaTime;

        if (updateTimer < 0)
        {
            ResetInfo();
            
            entities.RemoveAll(e => !e.IsAlive());
            updateTimer = 5;
            
            UpdateEntityInfo();
            UpdateAverageStats();
            UpdateWeak();
            UpdateElites();
            ReproduceSuccessful();

            foreach (Entity e in entities)
            {
                e.ResetMoves();   
            }
            numOfFood = 0;
            numOfHerb = 0;
            deadFood = 0;
            deadHerb = 0;

            if (entities.FindAll(e => e.GetEntType() == Entity.EntityType.food).Count < 50)
            {
                print("Introduce Food");
                ReintroducePopulation(Entity.EntityType.food);
            }

            if (entities.FindAll(e => e.GetEntType() == Entity.EntityType.food).Count > 200)
            {
                HerbivoreIntroCount++;
            }
            else
            {
                HerbivoreIntroCount = 0;
            }

            if (HerbivoreIntroCount > 3) HerbivoreIntro = true;

            if (entities.FindAll(e => e.GetEntType() == Entity.EntityType.herbivore).Count < 50
            && HerbivoreIntro)
            {
                print("Introduce Herbivore");
                ReintroducePopulation(Entity.EntityType.herbivore);
            }
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