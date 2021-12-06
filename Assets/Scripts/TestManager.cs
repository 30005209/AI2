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
    [SerializeField] private float updateTimer = 0;
    [SerializeField] private int herbivoreIntroCount = 0;
    [SerializeField] private bool herbivoreIntro = false;
    [SerializeField] private int carnivoreIntroCount = 0;
    [SerializeField] private bool carnivoreIntro = false;
    [SerializeField] private int numOfFood = 00;
    [SerializeField] private int numOfHerb = 0;
    [SerializeField] public int numOfCarn = 0;
    [SerializeField] private int numOfOmni = 0;
    [SerializeField] private int deadFood = 0;
    [SerializeField] private int deadHerb = 0;
    [SerializeField] private int deadCarn = 0;
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
    
    // Carnivore
    [SerializeField] public List<Entity> eliteCarn;
    [SerializeField] public List<Entity> weakCarn;
    [SerializeField] public float eliteCarnAmount = 1000;
    [SerializeField] public float weakCarnAmount = 100;
    [SerializeField] public List<Entity> reproCarn;

    public System.Random random;
    private string pathFInfo;
    private string pathFStat;
    private string pathHInfo;
    private string pathHStat;
    private string pathCInfo;
    private string pathCStat;
    
    int averageFood = 0;
    int averageHerb = 0;
    int averageCarn = 0;
    
    Entity.EventWeight foodEWfood = new Entity.EventWeight(0, 0, 0);
    Entity.EventWeight foodEWHerb = new Entity.EventWeight(0, 0, 0);
    Entity.EventWeight foodEWCarn = new Entity.EventWeight(0, 0, 0);
    
    Entity.EventWeight herbEWfood = new Entity.EventWeight(0, 0, 0);
    Entity.EventWeight herbEWHerb = new Entity.EventWeight(0, 0, 0);
    Entity.EventWeight herbEWCarn = new Entity.EventWeight(0, 0, 0);
    
    Entity.EventWeight carnEWfood = new Entity.EventWeight(0, 0, 0);
    Entity.EventWeight carnEWHerb = new Entity.EventWeight(0, 0, 0);
    Entity.EventWeight carnEWCarn = new Entity.EventWeight(0, 0, 0);

    
    
    // Start is called before the first frame update
    void Start()
    {
        random = new System.Random();
        
        pathFInfo = Application.dataPath + "/fSurvivalInfo.txt";
        pathFStat = Application.dataPath + "/fStatInfo.txt";
        pathHStat = Application.dataPath + "/hSurvivalInfo.txt";
        pathHInfo = Application.dataPath + "/hStatInfo.txt";
        pathCStat = Application.dataPath + "/cSurvivalInfo.txt";
        pathCInfo = Application.dataPath + "/cStatInfo.txt";

        CreateText(pathFInfo);
        CreateText(pathFStat);
        CreateText(pathHStat);
        CreateText(pathHInfo);
        CreateText(pathCStat);
        CreateText(pathCInfo);
    }

    void ReintroducePopulation(Entity.EntityType type)
    {
        int amountToAdd = 0;

        if (type == Entity.EntityType.food) amountToAdd = 100;
        if (type == Entity.EntityType.herbivore) amountToAdd = 50;
        if (type == Entity.EntityType.carnivore) amountToAdd = 20;
        if (type == Entity.EntityType.herbivore) amountToAdd = 10;
        
        for (int i = 0; i < amountToAdd; i++)
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
                    entities.Add(gameObject.AddComponent<Carnivore>());
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
            if (e.GetEntType() == Entity.EntityType.carnivore) deadCarn++;

            Destroy(e);
        }

        numOfFood = 0;
        numOfHerb = 0;
        numOfCarn = 0;
        
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
                    numOfCarn++;
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
        eliteHerb.Clear();
        eliteCarn.Clear();
        
        reproFood.Clear();
        reproHerb.Clear();
        reproCarn.Clear();
        
        weakFood.Clear();
        weakHerb.Clear();
        weakCarn.Clear();
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
            
            if (e.GetEntType() == Entity.EntityType.carnivore)
            {
                if (e.GetEnergyCur() >= eliteCarnAmount && e.IsAlive())
                {
                    eliteCarn.Add(e);
                }
                else if (e.GetEnergyCur() <= weakCarnAmount && e.IsAlive())
                {
                    weakCarn.Add(e);
                }
            }
                

            //File.AppendAllText(pathFStat, "\nElite (Pop: " + eliteFood.Count.ToString()
            //                                               + ") Reaching " + eliteFoodAmount.ToString().ToString());
        }
            
        // Elite Reproduction Food
        if (entities.FindAll(e => e.GetEntType() == Entity.EntityType.food).Count < 400)
        {
            foreach (Food f in eliteFood)
            {
                if (f.GetEnergyCur() > 500 & this != null)
                {
                    Entity E = null;

                    while (E == null)
                    {
                        E = eliteFood[random.Next(eliteFood.Count)];
                    }

                    groundEnergy -= 250;

                    f.NightReproduce(E);
                }
            }
        }


        // Elite Reproduction Herbivore
        if (entities.FindAll(e => e.GetEntType() == Entity.EntityType.herbivore).Count < 400)
        {
            foreach (Herbivore h in eliteHerb)
            {
                if (h.GetEnergyCur() > 500 && this != null)
                {
                    Entity E = null;

                    while (E == null)
                    {
                        E = eliteHerb[random.Next(eliteHerb.Count)];
                    }

                    h.ChangeEnergyLevel(-250);
                    h.NightReproduce(E);
                }
            }
        }

        // Elite Reproduction Herbivore
        if (entities.FindAll(e => e.GetEntType() == Entity.EntityType.carnivore).Count < 400)
        {
            foreach (Carnivore c in eliteCarn)
            {
                if (c.GetEnergyCur() > 500 && this != null)
                {
                    Entity E = null;

                    while (E == null)
                    {
                        E = eliteCarn[random.Next(eliteCarn.Count)];
                    }

                    c.ChangeEnergyLevel(-250);
                    c.NightReproduce(E);
                }
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
            
            if (e.GetEntType() == Entity.EntityType.carnivore)
            {
                eliteCarnAmount = e.GetEnergyCur() * 0.8f;
                weakCarnAmount = e.GetEnergyCur() * 0.2f;
                averageCarn += e.GetEnergyCur();
                numOfCarn++;
            }
            
        }

        if (numOfFood != 0) { averageFood /= numOfFood; }
        if (numOfHerb != 0) { averageHerb /= numOfHerb; }
        if (numOfCarn != 0) { averageCarn /= numOfCarn; }


       //File.AppendAllText(pathFStat, "\nNewGeneration (Died:" + deadFood.ToString() + ")(New Total Pop: "
       //                              + numOfFood.ToString() + "):\n" + "Average: " + averageFood.ToString());

       //File.AppendAllText(pathHInfo, "\nNewGeneration (Died:" + deadHerb.ToString() + ")(New Total Pop: "
       //                              + numOfHerb.ToString() + "):\n" + "Average: " + averageHerb.ToString());
        
    }

    private void ReproduceSuccessful()
    {
        // Reproduction Successful Food
        foreach (Food f in reproFood)
        {
            if (f.GetEnergyCur() > f.GetEnergyStart() && reproFood.Count < 300)
            {
                groundEnergy -= 250;

                f.NightReproduce(reproFood[random.Next(reproFood.Count - 1)]);
            }
        }

        // Reproduction Successful Herb
        foreach (Herbivore h in reproHerb)
        {
            if (h.GetEnergyCur() > h.GetEnergyStart() && reproHerb.Count < 200)
            {
                groundEnergy -= 250;
                h.NightReproduce(reproHerb[random.Next(reproHerb.Count - 1)]);
            }
        }
        
        // Reproduction Successful Carn
        foreach (Carnivore c in reproHerb)
        {
            if (c.GetEnergyCur() > c.GetEnergyStart() && reproCarn.Count < 200)
            {
                groundEnergy -= 250;
                c.NightReproduce(reproCarn[random.Next(reproCarn.Count - 1)]);
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
        
        foreach (Herbivore h in weakHerb)
        {
            h.SetAlive(false);
            h.causeOfDeath = "Weak";
        }
        
        
        //File.AppendAllText(pathFStat, "\nWeak (Pop: " + weakFood.Count.ToString() 
        //                                              + ") Reaching " + weakFoodAmount.ToString().ToString() + "\n");
    }

    private void UpdateAverageStats()
    {
        foreach (Entity e in entities)
        {
            if( e.GetEntType() == Entity.EntityType.food)
            {
                foodEWfood += e.GetEW(Entity.EntityType.food);
                foodEWHerb += e.GetEW(Entity.EntityType.herbivore);
                foodEWCarn += e.GetEW(Entity.EntityType.carnivore);
            }
                
            if( e.GetEntType() == Entity.EntityType.herbivore)
            {
                herbEWfood += e.GetEW(Entity.EntityType.food);
                herbEWHerb += e.GetEW(Entity.EntityType.herbivore);
                herbEWCarn += e.GetEW(Entity.EntityType.herbivore);
            }
            
            if( e.GetEntType() == Entity.EntityType.carnivore)
            {
                carnEWfood += e.GetEW(Entity.EntityType.food);
                carnEWHerb += e.GetEW(Entity.EntityType.herbivore);
                carnEWCarn += e.GetEW(Entity.EntityType.carnivore);
            }
            
        }

        if (numOfFood != 0)
        {
            foodEWfood /= numOfFood;
            foodEWHerb /= numOfFood;
            foodEWCarn /= numOfFood;
        }

        if (numOfHerb != 0)
        {
            herbEWfood /= numOfHerb;
            herbEWHerb /= numOfHerb;
            herbEWCarn /= numOfHerb;
        }
        
        if (numOfCarn != 0)
        {
            carnEWfood /= numOfCarn;
            carnEWHerb /= numOfCarn;
            carnEWCarn /= numOfCarn;
        }
        

        File.AppendAllText(pathFInfo, $"Average Food Stats (Food): {foodEWfood.OutputEWStats()}");
        File.AppendAllText(pathFInfo, $"Average Food Stats (Herb): {foodEWHerb.OutputEWStats()}");
        File.AppendAllText(pathFInfo, $"Average Food Stats (Carn): {foodEWCarn.OutputEWStats()}\n");
        
        File.AppendAllText(pathHStat, $"Average Herbivore Stats (Food): {herbEWfood.OutputEWStats()}");
        File.AppendAllText(pathHStat, $"Average Herbivore Stats (Herb): {herbEWHerb.OutputEWStats()}");
        File.AppendAllText(pathHStat, $"Average Herbivore Stats (Carn): {herbEWCarn.OutputEWStats()}\n");
        
        File.AppendAllText(pathCStat, $"Average Herbivore Stats (Food): {carnEWfood.OutputEWStats()}");
        File.AppendAllText(pathCStat, $"Average Herbivore Stats (Herb): {carnEWHerb.OutputEWStats()}");
        File.AppendAllText(pathCStat, $"Average Herbivore Stats (Carn): {carnEWCarn.OutputEWStats()}\n");
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
                File.AppendAllText(pathFStat, "Reintroduction of Food\n");
            }

            if (entities.FindAll(e => e.GetEntType() == Entity.EntityType.food).Count > 200)
            {
                herbivoreIntroCount++;
            }
            else
            {
                herbivoreIntroCount = 0;
                herbivoreIntro = false;
            }

            if (herbivoreIntroCount > 3) herbivoreIntro = true;
            

            if (entities.FindAll(e => e.GetEntType() == Entity.EntityType.herbivore).Count < 50
                    && herbivoreIntro)
            {
                print("Introduce Herbivore");
                ReintroducePopulation(Entity.EntityType.herbivore);
                File.AppendAllText(pathHStat, "Reintroduction of Herbivore\n");
                
            }
            
            if (entities.FindAll(e => e.GetEntType() == Entity.EntityType.herbivore).Count > 100)
            {
                carnivoreIntroCount++;
            }
            else
            {
                carnivoreIntroCount = 0;
                carnivoreIntro = false;
            }

            if (carnivoreIntroCount > 3) carnivoreIntro = true;

            if (entities.FindAll(e => e.GetEntType() == Entity.EntityType.carnivore).Count < 20
                && carnivoreIntro)
            {
                print("Introduce Carnivore");
                ReintroducePopulation(Entity.EntityType.carnivore);
                File.AppendAllText(pathCStat, "Reintroduction of Carnivore\n");
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