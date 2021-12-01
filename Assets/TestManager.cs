using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TestManager : MonoBehaviour
{
    [SerializeField] private int numOfHerb = 10;
    [SerializeField] private int numOfCarn = 10;
    [SerializeField] private int numOfOmni = 10;
    [SerializeField] private int numOfFood = 100;
    [SerializeField] public List<Entity> entities;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numOfHerb; i++)
        {
            entities.Add(gameObject.AddComponent<Herbivore>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Entity e in entities)
        {
            e.Update();
        }
        
    }

    public List<Entity> GetAllEnts()
    {
        return entities;
    }
}
