using System;
using System.Collections;
using System.Collections.Concurrent;
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
using System.Threading;
using JetBrains.Annotations;
using UnityEngine.SubsystemsImplementation;
using Debug = UnityEngine.Debug;
using System.Threading.Tasks;
using UnityEngine.PlayerLoop;

public class TestManager : MonoBehaviour
{
    [Header("Timer Info")] [SerializeField]
    private float timerCur = 0;

    [SerializeField] private float timerMax = 5.0f;

    [Header("Active Entities")] [Tooltip("Herbivores will spawn")] [SerializeField]
    private bool allowHerbivores = true;

    [Tooltip("Carnivores will spawn")] [SerializeField]
    private bool allowCarnivores = true;

    [Tooltip("Omnivores will spawn")] [SerializeField]
    private bool allowOmnivores = true;

    [Header("Populations")] [SerializeField]
    private int currentGeneration = 0;

    [SerializeField] private int numOfFood = 0;
    [SerializeField] private int numOfHerb = 0;
    [SerializeField] private int numOfCarn = 0;
    [SerializeField] private int numOfOmni = 0;

    [Header("Deaths")] [SerializeField] private int deadFood = 0;
    [SerializeField] private int deadHerb = 0;
    [SerializeField] private int deadCarn = 0;
    [SerializeField] private int deadOmni = 0;

    [Header("Food Information")] [SerializeField]
    private float eliteFoodAmount = 1000;

    [SerializeField] private float weakFoodAmount = 100;
    [HideInInspector] [SerializeField] private ConcurrentBag<Entity> eliteFood = new ConcurrentBag<Entity>();
    [HideInInspector] [SerializeField] private ConcurrentBag<Entity> weakFood = new ConcurrentBag<Entity>();
    [HideInInspector] [SerializeField] private ConcurrentBag<Entity> reproFood = new ConcurrentBag<Entity>();
    [HideInInspector] [SerializeField] private ConcurrentBag<Entity> curFood = new ConcurrentBag<Entity>();


    [Header("Herbivore Information")] [SerializeField]
    private float eliteHerbAmount = 1000;

    [SerializeField] private float weakHerbAmount = 100;
    [HideInInspector] [SerializeField] private ConcurrentBag<Entity> eliteHerb = new ConcurrentBag<Entity>();
    [HideInInspector] [SerializeField] private ConcurrentBag<Entity> weakHerb = new ConcurrentBag<Entity>();
    [HideInInspector] [SerializeField] private ConcurrentBag<Entity> reproHerb = new ConcurrentBag<Entity>();
    [HideInInspector] [SerializeField] private ConcurrentBag<Entity> curHerb = new ConcurrentBag<Entity>();

    [Header("Carnivore Information")] [SerializeField]
    private float eliteCarnAmount = 1000;

    [SerializeField] private float weakCarnAmount = 100;
    [HideInInspector] [SerializeField] private ConcurrentBag<Entity> eliteCarn = new ConcurrentBag<Entity>();
    [HideInInspector] [SerializeField] private ConcurrentBag<Entity> weakCarn = new ConcurrentBag<Entity>();
    [HideInInspector] [SerializeField] private ConcurrentBag<Entity> reproCarn = new ConcurrentBag<Entity>();
    [HideInInspector] [SerializeField] private ConcurrentBag<Entity> curCarn = new ConcurrentBag<Entity>();

    [Header("Omnivore Information")] [SerializeField]
    private float eliteOmniAmount = 1000;

    [SerializeField] private float weakOmniAmount = 100;
    [HideInInspector] [SerializeField] private ConcurrentBag<Entity> eliteOmni = new ConcurrentBag<Entity> ();
    [HideInInspector] [SerializeField] private ConcurrentBag<Entity> weakOmni = new ConcurrentBag<Entity> ();
    [HideInInspector] [SerializeField] private ConcurrentBag<Entity> reproOmni = new ConcurrentBag<Entity> ();
    [HideInInspector] [SerializeField] private ConcurrentBag<Entity> curOmni = new ConcurrentBag<Entity> ();

    [Header("General Info")]
    [Tooltip("How much energy the land has at the start, lower amounts will make it harder for plants to grow")]
    [SerializeField]
    public int groundEnergy = 1000;

    [SerializeField] public List<Entity> entities;
    [NonSerialized] public System.Random random;

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

        eliteFood = new ConcurrentBag<Entity>();
        eliteHerb = new ConcurrentBag<Entity>();
        eliteCarn = new ConcurrentBag<Entity>();
        eliteOmni = new ConcurrentBag<Entity>();

        reproFood = new ConcurrentBag<Entity>();
        reproHerb = new ConcurrentBag<Entity>();
        reproCarn = new ConcurrentBag<Entity>();
        reproOmni = new ConcurrentBag<Entity>();

        weakFood = new ConcurrentBag<Entity>();
        weakHerb = new ConcurrentBag<Entity>();
        weakCarn = new ConcurrentBag<Entity>();
        weakOmni = new ConcurrentBag<Entity>();
    }

    private void UpdateElites()
    {
        int sum = 0;
        Parallel.ForEach(
            entities, // source collection
            () => 0, // thread local initializer
            (e, loopState, localSum) => // body
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

                return localSum;
            },
            (localSum) => Interlocked.Add(ref sum, localSum) // thread local aggregator
        );
    }

    private void EliteRepro(Entity.EntityType type)
    {
        int count;
        if (entities.FindAll(e => e.GetEntType() == type) != null)
        {
            count = entities.FindAll(e => e.GetEntType() == type).Count;
            UpdateElites();

            int sum = 0;
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
                                    E = eliteFood.ToArray()[random.Next((eliteFood.Count))];
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
                                    E = eliteHerb.ToArray()[random.Next(eliteHerb.Count)];
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
                                    E = eliteCarn.ToArray()[random.Next(eliteCarn.Count)];
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
                                    E = eliteOmni.ToArray()[random.Next(eliteOmni.Count)];
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
        Parallel.ForEach(
            curFood, // source collection
            () => 0, // thread local initializer
            (e, loopState, localSum) => // body
            {
                localSum += e.GetEnergyCur();
                numOfFood++;
                return localSum;
            },
            (localSum) => Interlocked.Add(ref averageFood, localSum) // thread local aggregator
        );

        Parallel.ForEach(
            curHerb, // source collection
            () => 0, // thread local initializer
            (e, loopState, localSum) => // body
            {
                localSum += e.GetEnergyCur();
                numOfHerb++;
                return localSum;
            },
            (localSum) => Interlocked.Add(ref averageHerb, localSum) // thread local aggregator
        );

        Parallel.ForEach(
            curCarn, // source collection
            () => 0, // thread local initializer
            (e, loopState, localSum) => // body
            {
                localSum += e.GetEnergyCur();
                numOfCarn++;
                return localSum;
            },
            (localSum) => Interlocked.Add(ref averageCarn, localSum) // thread local aggregator
        );

        Parallel.ForEach(
            curOmni, // source collection
            () => 0, // thread local initializer
            (e, loopState, localSum) => // body
            {
                localSum += e.GetEnergyCur();
                numOfOmni++;
                return localSum;
            },
            (localSum) => Interlocked.Add(ref averageOmni, localSum) // thread local aggregator
        );

        if (numOfFood != 0)
        {
            averageFood /= numOfFood;
            eliteFoodAmount = averageFood * 0.7f;
            weakFoodAmount = averageFood * 0.3f;
        }

        if (numOfHerb != 0)
        {
            averageHerb /= numOfHerb;
            eliteHerbAmount = averageHerb * 0.7f;
            weakHerbAmount = averageHerb * 0.3f;
        }

        if (numOfCarn != 0)
        {
            averageCarn /= numOfCarn;
            eliteCarnAmount = averageCarn * 0.7f;
            weakCarnAmount = averageCarn * 0.3f;
        }

        if (numOfOmni != 0)
        {
            averageOmni /= numOfOmni;
            eliteOmniAmount = averageOmni * 0.7f;
            weakOmniAmount = averageOmni * 0.3f;
        }


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
        int sum = 0;
        switch (type)
        {
            case Entity.EntityType.food:
                Parallel.ForEach(
                    reproFood, // source collection
                    () => 0, // thread local initializer
                    (e, loopState, localSum) => // body
                    {
                        if (e.GetEnergyCur() > e.GetEnergyStart() && reproFood.Count < 400)
                        {
                            groundEnergy -= 250;

                            e.NightReproduce(reproFood.ToArray()[random.Next(reproFood.Count - 1)]);
                        }

                        return localSum;
                    },
                    (localSum) => Interlocked.Add(ref sum, localSum) // thread local aggregator
                );
                break;

            case Entity.EntityType.herbivore:
                // Reproduction Successful Herb
                Parallel.ForEach(
                    reproFood, // source collection
                    () => 0, // thread local initializer
                    (e, loopState, localSum) => // body
                    {
                        if (e.GetEnergyCur() > e.GetEnergyStart() && reproHerb.Count < 100)
                        {
                            groundEnergy -= 250;
                            e.NightReproduce(reproHerb.ToArray()[random.Next(reproHerb.Count - 1)]);
                        }

                        return localSum;
                    },
                    (localSum) => Interlocked.Add(ref sum, localSum) // thread local aggregator
                );
                break;

            case Entity.EntityType.carnivore:
                // Reproduction Successful Carn
                Parallel.ForEach(
                    reproCarn, // source collection
                    () => 0, // thread local initializer
                    (e, loopState, localSum) => // body
                    {
                        if (e.GetEnergyCur() > e.GetEnergyStart() && reproCarn.Count < 100)
                        {
                            groundEnergy -= 250;
                            e.NightReproduce(reproCarn.ToArray()[random.Next(reproCarn.Count - 1)]);
                        }

                        return localSum;
                    },
                    (localSum) => Interlocked.Add(ref sum, localSum) // thread local aggregator
                );
                break;

            case Entity.EntityType.omnivore:
                // Reproduction Successful Carn
                Parallel.ForEach(
                    reproOmni, // source collection
                    () => 0, // thread local initializer
                    (e, loopState, localSum) => // body
                    {
                        if (e.GetEnergyCur() > e.GetEnergyStart() && reproOmni.Count < 20)
                        {
                            groundEnergy -= 250;
                            e.NightReproduce(reproOmni.ToArray()[random.Next(reproOmni.Count - 1)]);
                        }

                        return localSum;
                    },
                    (localSum) => Interlocked.Add(ref sum, localSum) // thread local aggregator
                );
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private void UpdateWeak()
    {
        int sum = 0;
        Parallel.ForEach(
            weakFood, // source collection
            () => 0, // thread local initializer
            (f, loopState, localSum) => // body
            {
                f.SetAlive(false);
                f.causeOfDeath = "Weak";
                return localSum;
            },
            (localSum) => Interlocked.Add(ref sum, localSum) // thread local aggregator
        );

        Parallel.ForEach(
            weakHerb, // source collection
            () => 0, // thread local initializer
            (e, loopState, localSum) => // body
            {
                e.SetAlive(false);
                e.causeOfDeath = "Weak";
                return localSum;
            },
            (localSum) => Interlocked.Add(ref sum, localSum) // thread local aggregator
        );

        Parallel.ForEach(
            weakCarn, // source collection
            () => 0, // thread local initializer
            (e, loopState, localSum) => // body
            {
                e.SetAlive(false);
                e.causeOfDeath = "Weak";
                return localSum;
            },
            (localSum) => Interlocked.Add(ref sum, localSum) // thread local aggregator
        );

        Parallel.ForEach(
            weakOmni, // source collection
            () => 0, // thread local initializer
            (e, loopState, localSum) => // body
            {
                e.SetAlive(false);
                e.causeOfDeath = "Weak";
                return localSum;
            },
            (localSum) => Interlocked.Add(ref sum, localSum) // thread local aggregator
        );


        //File.AppendAllText(pathFStat, "\nWeak (Pop: " + weakFood.Count.ToString() 
        //                                              + ") Reaching " + weakFoodAmount.ToString().ToString() + "\n");
    }

    private void UpdateAverageStats()
    {
        Parallel.ForEach(
            curFood, // source collection
            () => 0, // thread local initializer
            (e, loopState, localSum) => // body
            {
                foodEWfood += e.GetEW(Entity.EntityType.food);
                foodEWHerb += e.GetEW(Entity.EntityType.herbivore);
                foodEWCarn += e.GetEW(Entity.EntityType.carnivore);
                foodEWOmni += e.GetEW(Entity.EntityType.omnivore);
                localSum++;
                return localSum;
            },
            (localSum) => Interlocked.Add(ref numOfFood, localSum) // thread local aggregator
        );

        Parallel.ForEach(
            curHerb, // source collection
            () => 0, // thread local initializer
            (e, loopState, localSum) => // body
            {
                herbEWfood += e.GetEW(Entity.EntityType.food);
                herbEWHerb += e.GetEW(Entity.EntityType.herbivore);
                herbEWCarn += e.GetEW(Entity.EntityType.carnivore);
                herbEWOmni += e.GetEW(Entity.EntityType.omnivore);
                localSum++;
                return localSum;
            },
            (localSum) => Interlocked.Add(ref numOfHerb, localSum) // thread local aggregator
        );

        Parallel.ForEach(
            curCarn, // source collection
            () => 0, // thread local initializer
            (e, loopState, localSum) => // body
            {
                carnEWfood += e.GetEW(Entity.EntityType.food);
                carnEWHerb += e.GetEW(Entity.EntityType.herbivore);
                carnEWCarn += e.GetEW(Entity.EntityType.carnivore);
                carnEWOmni += e.GetEW(Entity.EntityType.omnivore);
                localSum++;
                return localSum;
            },
            (localSum) => Interlocked.Add(ref numOfCarn, localSum) // thread local aggregator
        );

        Parallel.ForEach(
            curOmni, // source collection
            () => 0, // thread local initializer
            (e, loopState, localSum) => // body
            {
                omniEWfood += e.GetEW(Entity.EntityType.food);
                omniEWHerb += e.GetEW(Entity.EntityType.herbivore);
                omniEWCarn += e.GetEW(Entity.EntityType.carnivore);
                omniEWOmni += e.GetEW(Entity.EntityType.omnivore);
                localSum++;
                return localSum;
            },
            (localSum) => Interlocked.Add(ref numOfOmni, localSum) // thread local aggregator
        );

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

            entities?.RemoveAll(e => !e.IsAlive());
            timerCur = timerMax;

            curFood = new ConcurrentBag<Entity>();
            curHerb = new ConcurrentBag<Entity>();
            curCarn = new ConcurrentBag<Entity>();
            curOmni = new ConcurrentBag<Entity>();

            numOfFood = 0;
            numOfHerb = 0;
            numOfCarn = 0;
            numOfOmni = 0;
            deadFood = 0;
            deadHerb = 0;
            deadCarn = 0;
            deadOmni = 0;

            if (entities != null)
            {
                foreach (Entity e in entities)
                {
                    switch (e.GetEntType())
                    {
                        case Entity.EntityType.food:
                            curFood.Add(e);
                            break;

                        case Entity.EntityType.herbivore:
                            curHerb.Add(e);
                            break;
                        case Entity.EntityType.carnivore:
                            curCarn.Add(e);
                            break;

                        case Entity.EntityType.omnivore:
                            curOmni.Add(e);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

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
                (numOfFood + numOfHerb + numOfCarn) / 2 > numOfOmni)
            {
                print("Introduce Omnivores");
                ReintroducePopulation(Entity.EntityType.omnivore);
                File.AppendAllText(pathOStat, "\nReintroduction of Omnivores\n");
            }

            currentGeneration++;


        }
    }

    void CreateText(string givenPath)
    {
        if (!File.Exists(givenPath))
        {
            File.WriteAllText(givenPath, "Test:\n\n");
        }
    }

    public ref List<Entity> GetAllEnts()
    {
        return ref entities;
    }

    public int GetGeneration()
    {
        return currentGeneration;
    }

    public int GetPopulation(Entity.EntityType type)
    {
        switch (type)
        {
            case Entity.EntityType.food:
                return numOfFood;

            case Entity.EntityType.herbivore:
                return numOfHerb;

            case Entity.EntityType.carnivore:
                return numOfCarn;

            case Entity.EntityType.omnivore:
                return numOfOmni;

            default:
                Debug.LogError("Failed to obtain population");
                return -1;
        }
    }

    public Entity.EventWeight GetEW(Entity.EntityType source, Entity.EntityType target)
    {
        switch (source)
        {
            case Entity.EntityType.food:
                switch (target)
                {
                    case Entity.EntityType.food:
                        return foodEWfood;
                    
                    case Entity.EntityType.herbivore:
                        return foodEWHerb;
                    
                    case Entity.EntityType.carnivore:
                        return foodEWCarn;
                    
                    case Entity.EntityType.omnivore:
                        return foodEWOmni;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(target), target, null);
                }
                
            case Entity.EntityType.herbivore:
                switch (target)
                {
                    case Entity.EntityType.food:
                        return herbEWfood;
                    
                    case Entity.EntityType.herbivore:
                        return herbEWHerb;
                    
                    case Entity.EntityType.carnivore:
                        return herbEWCarn;
                    
                    case Entity.EntityType.omnivore:
                        return herbEWOmni;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(target), target, null);
                }
            case Entity.EntityType.carnivore:
                switch (target)
                {
                    case Entity.EntityType.food:
                        return carnEWfood;
                    
                    case Entity.EntityType.herbivore:
                        return carnEWHerb;
                    
                    case Entity.EntityType.carnivore:
                        return carnEWCarn;
                    
                    case Entity.EntityType.omnivore:
                        return carnEWOmni;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(target), target, null);
                }
            case Entity.EntityType.omnivore:
                switch (target)
                {
                    case Entity.EntityType.food:
                        return omniEWfood;
                    
                    case Entity.EntityType.herbivore:
                        return omniEWHerb;
                    
                    case Entity.EntityType.carnivore:
                        return omniEWCarn;
                    
                    case Entity.EntityType.omnivore:
                        return omniEWOmni;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(target), target, null);
                }
            default:
                throw new ArgumentOutOfRangeException(nameof(source), source, null);
        }
    }
}