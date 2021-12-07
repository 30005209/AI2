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
using System.Linq;
using System.Security;
using UnityEngine.SubsystemsImplementation;

public class TestManager : MonoBehaviour
{
    [Header("Timer Info")]
    [SerializeField] private float timerCur = 0;
    [SerializeField] private float timerMax = 5.0f;
    
    [Header("Active Entities")] 
    [Tooltip("Herbivores will spawn")] [SerializeField] private bool allowHerbivores = true;
    [Tooltip("Carnivores will spawn")] [SerializeField] private bool allowCarnivores = true;
    [Tooltip("Omnivores will spawn")] [SerializeField] private bool allowOmnivores = true;
    
    [Header("Populations")]
    [SerializeField] private int numOfFood = 0;
    [SerializeField] private int numOfHerb = 0;
    [SerializeField] public int numOfCarn = 0;
    [SerializeField] private int numOfOmni = 0;
    
    [Header("Deaths")]
    [SerializeField] private int deadFood = 0;
    [SerializeField] private int deadHerb = 0;
    [SerializeField] private int deadCarn = 0;
    [SerializeField] private int deadOmni = 0;
    
    [Header("Starting Scene")]
    [Tooltip("How much energy the land has at the start, lower amounts will make it harder for plants to grow")]
    [SerializeField] public int groundEnergy = 1000;
    
    [SerializeField] public List<Entity> entities;
    
    [Header("Food Information")]
    [SerializeField] public List<Entity> eliteFood;
    [SerializeField] public List<Entity> weakFood;
    [SerializeField] public float eliteFoodAmount = 1000;
    [SerializeField] public float weakFoodAmount = 100;
    [SerializeField] public List<Entity> reproFood;
    
    [Header("Herbivore Information")]
    [SerializeField] public List<Entity> eliteHerb;
    [SerializeField] public List<Entity> weakHerb;
    [SerializeField] public float eliteHerbAmount = 1000;
    [SerializeField] public float weakHerbAmount = 100;
    [SerializeField] public List<Entity> reproHerb;
    
    [Header("Carnivore Information")]
    [SerializeField] public List<Entity> eliteCarn;
    [SerializeField] public List<Entity> weakCarn;
    [SerializeField] public float eliteCarnAmount = 1000;
    [SerializeField] public float weakCarnAmount = 100;
    [SerializeField] public List<Entity> reproCarn;
    
    [Header("Omnivore Information")]
    [SerializeField] public List<Entity> eliteOmni;
    [SerializeField] public List<Entity> weakOmni;
    [SerializeField] public float eliteOmniAmount = 1000;
    [SerializeField] public float weakOmniAmount = 100;
    [SerializeField] public List<Entity> reproOmni;

    public System.Random random;
    
    private string pathFInfo;
    private string pathFStat;
    private string pathHInfo;
    private string pathHStat;
    private string pathCInfo;
    private string pathCStat;
    private string pathOInfo;
    private string pathOStat;
    
    // Average amount of energy found in each type of entity
    private int averageFood = 0;
    private int averageHerb = 0;
    private int averageCarn = 0;
    private int averageOmni = 0;
    
    // Event weights used to keep track of average action dispositions
    private Entity.EventWeight foodEWfood = new Entity.EventWeight(0, 0, 0);
    private Entity.EventWeight foodEWHerb = new Entity.EventWeight(0, 0, 0);
    private Entity.EventWeight foodEWCarn = new Entity.EventWeight(0, 0, 0);
    private Entity.EventWeight foodEWOmni = new Entity.EventWeight(0, 0, 0);

    private Entity.EventWeight herbEWfood = new Entity.EventWeight(0, 0, 0);
    private Entity.EventWeight herbEWHerb = new Entity.EventWeight(0, 0, 0);
    private Entity.EventWeight herbEWCarn = new Entity.EventWeight(0, 0, 0);
    private Entity.EventWeight herbEWOmni = new Entity.EventWeight(0, 0, 0);

    private Entity.EventWeight carnEWfood = new Entity.EventWeight(0, 0, 0);
    private Entity.EventWeight carnEWHerb = new Entity.EventWeight(0, 0, 0);
    private Entity.EventWeight carnEWCarn = new Entity.EventWeight(0, 0, 0);
    private Entity.EventWeight carnEWOmni = new Entity.EventWeight(0, 0, 0);

    private Entity.EventWeight omniEWfood = new Entity.EventWeight(0, 0, 0);
    private Entity.EventWeight omniEWHerb = new Entity.EventWeight(0, 0, 0);
    private Entity.EventWeight omniEWCarn = new Entity.EventWeight(0, 0, 0);
    private Entity.EventWeight omniEWOmni = new Entity.EventWeight(0, 0, 0);
    
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
        pathOStat = Application.dataPath + "/oSurvivalInfo.txt";
        pathOInfo = Application.dataPath + "/oStatInfo.txt";

        //CreateText(pathFInfo);
        //CreateText(pathHInfo);
        //CreateText(pathCInfo);
        //CreateText(pathOInfo);
        
        CreateText(pathFStat);
        CreateText(pathHStat);
        CreateText(pathCStat);
        CreateText(pathOStat);
    }

    void ReintroducePopulation(Entity.EntityType type)
    {
        if (entities.FindAll(e => e.GetEntType() == type).Count == 0)
        {
            for (int i = 0; i < 50; i++)
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
                        entities.Add(gameObject.AddComponent<Omnivore>());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            
            }
        }
        else
        {
            ReproSuccessfulType(type);
            UpdateElites();
            EliteRepro(type);
        }
        
        
    }
    
    
    void Update()
    {
        List<Entity> toRemove = entities.FindAll(e => !e.IsAlive());

        entities.RemoveAll(e => !e.IsAlive());

        foreach (Entity e in toRemove)
        {
            //print(e.GetEntType().ToString() + " - " + e.causeOfDeath);
            if (e.GetEntType() == Entity.EntityType.food) deadFood++;
            if (e.GetEntType() == Entity.EntityType.herbivore) deadHerb++;
            if (e.GetEntType() == Entity.EntityType.carnivore) deadCarn++;
            if (e.GetEntType() == Entity.EntityType.omnivore) deadOmni++;

            Destroy(e);
        }

        numOfFood = 0;
        numOfHerb = 0;
        numOfCarn = 0;
        numOfOmni = 0;
        
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
                    numOfOmni++;
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
        eliteOmni.Clear();
        
        reproFood.Clear();
        reproHerb.Clear();
        reproCarn.Clear();
        reproOmni.Clear();
        
        weakFood.Clear();
        weakHerb.Clear();
        weakCarn.Clear();
        weakOmni.Clear();
    }

    private void UpdateElites()
    {
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
                
            if (e.GetEntType() == Entity.EntityType.omnivore)
            {
                if (e.GetEnergyCur() >= eliteOmniAmount && e.IsAlive())
                {
                    eliteOmni.Add(e);
                }
                else if (e.GetEnergyCur() <= weakOmniAmount && e.IsAlive())
                {
                    weakOmni.Add(e);
                }
            }

            //File.AppendAllText(pathFStat, "\nElite (Pop: " + eliteFood.Count.ToString()
            //                                               + ") Reaching " + eliteFoodAmount.ToString().ToString());
        }
    }

    private void EliteRepro(Entity.EntityType type)
    {
        int count;
        if (entities.FindAll(e => e.GetEntType() == type) != null)
        {
            count = entities.FindAll(e => e.GetEntType() == type).Count;
            UpdateElites();

            switch (type)
            {

                case Entity.EntityType.food:
                    // Elite Reproduction Food

                    if (count < 600)
                    {
                        foreach (var f in eliteFood)
                        {
                            if (f is Food && f.GetEnergyCur() > 500 & this != null)
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
                    break;

                case Entity.EntityType.herbivore:
                    // Elite Reproduction Herbivore
                    if (count < 200)
                    {
                        foreach (var h in eliteHerb)
                        {
                            if (h is Herbivore && h.GetEnergyCur() > 500 && this != null)
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
                    break;

                case Entity.EntityType.carnivore:
                    // Elite Reproduction Carnivore
                    if (count < 200)
                    {
                        foreach (var c in eliteCarn)
                        {
                            if (c is Carnivore && c.GetEnergyCur() > 500 && this != null)
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
                    break;

                case Entity.EntityType.omnivore:
                    // Elite Reproduction Omnivore
                    if (count < 80)
                    {
                        foreach (var o in eliteOmni)
                        {
                            if (o is Omnivore && o.GetEnergyCur() > 500 && this != null)
                            {
                                Entity E = null;

                                while (E == null)
                                {
                                    E = eliteOmni[random.Next(eliteOmni.Count)];
                                }

                                o.ChangeEnergyLevel(-250);
                                o.NightReproduce(E);
                            }
                        }
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
    
    private void EliteReproAll()
    {
        EliteRepro(Entity.EntityType.food);
        EliteRepro(Entity.EntityType.herbivore);
        EliteRepro(Entity.EntityType.carnivore);
        EliteRepro(Entity.EntityType.omnivore);
    }

    private void UpdateEntityInfo()
    {
        foreach (Entity e in entities)
        {
            if (e.GetEntType() == Entity.EntityType.food)
            {
                eliteFoodAmount = e.GetEnergyCur() * 0.7f;
                weakFoodAmount = e.GetEnergyCur() * 0.3f;
                averageFood += e.GetEnergyCur();
                numOfFood++;
            }

            if (e.GetEntType() == Entity.EntityType.herbivore)
            {
                eliteHerbAmount = e.GetEnergyCur() * 0.7f;
                weakHerbAmount = e.GetEnergyCur() * 0.3f;
                averageHerb += e.GetEnergyCur();
                numOfHerb++;
            }
            
            if (e.GetEntType() == Entity.EntityType.carnivore)
            {
                eliteCarnAmount = e.GetEnergyCur() * 0.7f;
                weakCarnAmount = e.GetEnergyCur() * 0.3f;
                averageCarn += e.GetEnergyCur();
                numOfCarn++;
            }
            
            if (e.GetEntType() == Entity.EntityType.omnivore)
            {
                eliteOmniAmount = e.GetEnergyCur() * 0.7f;
                weakOmniAmount = e.GetEnergyCur() * 0.3f;
                averageOmni += e.GetEnergyCur();
                numOfOmni++;
            }
            
        }

        if (numOfFood != 0) { averageFood /= numOfFood; }
        if (numOfHerb != 0) { averageHerb /= numOfHerb; }
        if (numOfCarn != 0) { averageCarn /= numOfCarn; }
        if (numOfOmni != 0) { averageOmni /= numOfOmni; }


       //File.AppendAllText(pathFStat, "\nNewGeneration (Died:" + deadFood.ToString() + ")(New Total Pop: "
       //                              + numOfFood.ToString() + "):\n" + "Average: " + averageFood.ToString());

       //File.AppendAllText(pathHInfo, "\nNewGeneration (Died:" + deadHerb.ToString() + ")(New Total Pop: "
       //                              + numOfHerb.ToString() + "):\n" + "Average: " + averageHerb.ToString());
        
    }

    private void ReproduceSuccessful()
    {
        ReproSuccessfulType(Entity.EntityType.food);
        ReproSuccessfulType(Entity.EntityType.herbivore);
        ReproSuccessfulType(Entity.EntityType.carnivore);
        ReproSuccessfulType(Entity.EntityType.omnivore);
    }

    void ReproSuccessfulType(Entity.EntityType type)
    {
        switch (type)
        {
            case Entity.EntityType.food:
                // Reproduction Successful Food
                foreach (Food f in reproFood)
                {
                    if (f.GetEnergyCur() > f.GetEnergyStart() && reproFood.Count < 400)
                    {
                        groundEnergy -= 250;

                        f.NightReproduce(reproFood[random.Next(reproFood.Count - 1)]);
                    }
                }
                break;
            
            case Entity.EntityType.herbivore:
                // Reproduction Successful Herb
                foreach (Herbivore h in reproHerb)
                {
                    if (h.GetEnergyCur() > h.GetEnergyStart() && reproHerb.Count < 100)
                    {
                        groundEnergy -= 250;
                        h.NightReproduce(reproHerb[random.Next(reproHerb.Count - 1)]);
                    }
                }
                break;
            
            case Entity.EntityType.carnivore:
                // Reproduction Successful Carn
                foreach (Carnivore c in reproHerb)
                {
                    if (c.GetEnergyCur() > c.GetEnergyStart() && reproCarn.Count < 100)
                    {
                        groundEnergy -= 250;
                        c.NightReproduce(reproCarn[random.Next(reproCarn.Count - 1)]);
                    }
                }
                break;
            
            case Entity.EntityType.omnivore:
                // Reproduction Successful Carn
                foreach (Omnivore o in reproOmni)
                {
                    if (o.GetEnergyCur() > o.GetEnergyStart() && reproOmni.Count < 20)
                    {
                        groundEnergy -= 250;
                        o.NightReproduce(reproOmni[random.Next(reproOmni.Count - 1)]);
                    }
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
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

        foreach (Carnivore c in weakCarn)
        {
            c.SetAlive(false);
            c.causeOfDeath = "Weak";
        }
        
        foreach (Omnivore o in weakOmni)
        {
            o.SetAlive(false);
            o.causeOfDeath = "Weak";
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
                foodEWOmni += e.GetEW(Entity.EntityType.omnivore);
            }
                
            if( e.GetEntType() == Entity.EntityType.herbivore)
            {
                herbEWfood += e.GetEW(Entity.EntityType.food);
                herbEWHerb += e.GetEW(Entity.EntityType.herbivore);
                herbEWCarn += e.GetEW(Entity.EntityType.carnivore);
                herbEWOmni += e.GetEW(Entity.EntityType.omnivore);
            }
            
            if( e.GetEntType() == Entity.EntityType.carnivore)
            {
                carnEWfood += e.GetEW(Entity.EntityType.food);
                carnEWHerb += e.GetEW(Entity.EntityType.herbivore);
                carnEWCarn += e.GetEW(Entity.EntityType.carnivore);
                carnEWOmni += e.GetEW(Entity.EntityType.omnivore);
            }
            
            if( e.GetEntType() == Entity.EntityType.omnivore)
            {
                omniEWfood += e.GetEW(Entity.EntityType.food);
                omniEWHerb += e.GetEW(Entity.EntityType.herbivore);
                omniEWCarn += e.GetEW(Entity.EntityType.carnivore);
                omniEWOmni += e.GetEW(Entity.EntityType.omnivore);
            }
            
        }

        if (numOfFood != 0)
        {
            foodEWfood /= numOfFood;
            foodEWHerb /= numOfFood;
            foodEWCarn /= numOfFood;
            foodEWOmni /= numOfFood;
        }

        if (numOfHerb != 0)
        {
            herbEWfood /= numOfHerb;
            herbEWHerb /= numOfHerb;
            herbEWCarn /= numOfHerb;
            herbEWOmni /= numOfHerb;
        }
        
        if (numOfCarn != 0)
        {
            carnEWfood /= numOfCarn;
            carnEWHerb /= numOfCarn;
            carnEWCarn /= numOfCarn;
            carnEWOmni /= numOfCarn;
        }
        
        if (numOfOmni != 0)
        {
            omniEWfood /= numOfOmni;
            omniEWHerb /= numOfOmni;
            omniEWCarn /= numOfOmni;
            omniEWOmni /= numOfOmni;
        }
        

        File.AppendAllText(pathFStat, $"Average Food Stats (Food): {foodEWfood.OutputEWStats()}");
        File.AppendAllText(pathFStat, $"Average Food Stats (Herb): {foodEWHerb.OutputEWStats()}");
        File.AppendAllText(pathFStat, $"Average Food Stats (Carn): {foodEWCarn.OutputEWStats()}");
        File.AppendAllText(pathFStat, $"Average Food Stats (Omni): {foodEWOmni.OutputEWStats()}\n");
        
        File.AppendAllText(pathHStat, $"Average Herbivore Stats (Food): {herbEWfood.OutputEWStats()}");
        File.AppendAllText(pathHStat, $"Average Herbivore Stats (Herb): {herbEWHerb.OutputEWStats()}");
        File.AppendAllText(pathHStat, $"Average Herbivore Stats (Carn): {herbEWCarn.OutputEWStats()}");
        File.AppendAllText(pathHStat, $"Average Herbivore Stats (Omni): {herbEWOmni.OutputEWStats()}\n");
        
        File.AppendAllText(pathCStat, $"Average Carnivore Stats (Food): {carnEWfood.OutputEWStats()}");
        File.AppendAllText(pathCStat, $"Average Carnivore Stats (Herb): {carnEWHerb.OutputEWStats()}");
        File.AppendAllText(pathCStat, $"Average Carnivore Stats (Carn): {carnEWCarn.OutputEWStats()}");
        File.AppendAllText(pathCStat, $"Average Carnivore Stats (Omni): {carnEWOmni.OutputEWStats()}\n");
        
        File.AppendAllText(pathOStat, $"Average Carnivore Stats (Food): {omniEWfood.OutputEWStats()}");
        File.AppendAllText(pathOStat, $"Average Carnivore Stats (Herb): {omniEWHerb.OutputEWStats()}");
        File.AppendAllText(pathOStat, $"Average Carnivore Stats (Carn): {omniEWCarn.OutputEWStats()}");
        File.AppendAllText(pathOStat, $"Average Carnivore Stats (Omni): {omniEWOmni.OutputEWStats()}\n");
    }
    
    private void FixedUpdate()
    {
        timerCur -= Time.deltaTime;

        if (timerCur < 0)
        {
            ResetInfo();
            
            entities.RemoveAll(e => !e.IsAlive());
            timerCur = timerMax;
            
            UpdateEntityInfo();
            UpdateAverageStats();
            UpdateWeak();
            UpdateElites();
            EliteReproAll();
            ReproduceSuccessful();

            foreach (Entity e in entities)
            {
                e.ResetMoves();   
            }

            // If less than 100 bits off food - Reintroduce food
            if (numOfFood < 100)
            {
                print("Introduce Food");
                ReintroducePopulation(Entity.EntityType.food);
                File.AppendAllText(pathFStat, "\nReintroduction of Food\n");
            }

            // If there is more than 200 food and  more food than herbivores - Reintroduce herbivores
            if (allowHerbivores && numOfFood > 500 && numOfFood > numOfHerb)
            {
                print("Introduce Herbivores");
                ReintroducePopulation(Entity.EntityType.herbivore);
                File.AppendAllText(pathHStat, "\nReintroduction of Herbivores\n");
            }
            
            // If there is more than 100 herbivores and more herbivores than carnivores - Reintroduce carnivores
            if (allowCarnivores && numOfHerb > 400 && numOfHerb > numOfCarn)
            {
                print("Introduce Carnivores");
                ReintroducePopulation(Entity.EntityType.carnivore);
                File.AppendAllText(pathCStat, "\nReintroduction of Carnivores\n");
            }
            
            // If there is more than 50 carnivores and more than 50 and more than 50 food and more
            // (carnivores + herbivores + food ) /2 than omnivores - Reintroduce omnivores
            if (allowOmnivores && numOfFood > 100 && numOfHerb > 100 && numOfCarn > 100 && 
                (numOfFood + numOfHerb + numOfCarn)/2 > numOfOmni)
            {
                print("Introduce Omnivores");
                ReintroducePopulation(Entity.EntityType.omnivore);
                File.AppendAllText(pathOStat, "\nReintroduction of Omnivores\n");
            }

            numOfFood = 0;
            numOfHerb = 0;
            numOfCarn = 0;
            numOfOmni = 0;
            deadFood = 0;
            deadHerb = 0;
            deadCarn = 0;
            deadOmni = 0;
        }
    }
    void CreateText(string givenPath)
    {
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