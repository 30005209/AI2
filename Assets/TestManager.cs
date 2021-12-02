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
    [SerializeField] public int numOfCarn = 10;
    [SerializeField] private int numOfOmni = 10;
    [SerializeField] private int numOfFood = 100;
    [SerializeField] public List<Entity> entities;
    public System.Random random;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        random = new System.Random();
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
            Destroy(e);
        }
        
    }

    private void FixedUpdate()
    {
        numOfHerb = 0;
        int x = entities.Count;

        foreach (Entity e in entities)
        {
            e.Update();

            if (e.GetEntType() == Entity.EntityType.herbivore)
            {
                numOfHerb++;
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
