using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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
    public System.Random random;
    
    
    
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

        if (groundEnergy < 0) groundEnergy = 0;
        if (groundEnergy > 10000) groundEnergy = 10000;

    }

    private void FixedUpdate()
    {
        numOfHerb = 0;
        numOfFood = 0;
        int x = entities.Count;

        groundEnergy += 10;

        foreach (Entity e in entities)
        {
            e.Update();

            if (e.GetEntType() == Entity.EntityType.herbivore)
            {
                numOfHerb++;
            }

            if(e.GetEntType() == Entity.EntityType.food)
            {
                numOfFood++;
            }
        }

        if (entities.FindAll(e => e.MustAct()).Count == 0)
        {
            foreach (Entity e in entities)
            {
                e.SetMustAct(true);
            }
        }

    }

    public List<Entity> GetAllEnts()
    {
        return entities;
    }
}
