using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    //Genetic algorithmn
    [SerializeField] private int size;
    [SerializeField] private int energyMax;
    
    
    //Derived stats
    [SerializeField] private float energy;
    [SerializeField] private float nutrition;


}

public enum AnimalStats
{
    size = 0,
    energyMax = 1,
    speed = 2,
    damage = 3,

}

public class Animal : Entity
{
    private int numberOfStats = 4;
}

public class AnimalTest : MonoBehaviour
{
    [Header("Genetic Algorithm")]
    [SerializeField] int populationSize = 200;
    [SerializeField] float mutationRate = 0.01f;
    [SerializeField] int elitism = 5;

    [Header("Other")]
    [SerializeField] int x = 0;
    struct AnimalStats
    {
        int size;
        int energyMax;
        int speed;
        int damage;
    }

    GeneticAlgorithm<Animal> ga;
    private System.Random random;

    private void Start()
    {

        random = new System.Random();

        //public GeneticAlgorithm(int populationSize, int dnaSize, Random random, Func<T> getRandomGene, Func<int, float> fitnessFunction,
          //  int elitism, float mutationRate = 0.01f)
        ga = new GeneticAlgorithm<int[]>(populationSize, 4, random, SetAnimalInfo, FitnessFunction, elitism, mutationRate);

    }


    AnimalStats SetAnimalInfo()
    {
        AnimalStats x;

        return x;
    }

    int FitnessFunction()
    {
        return 0;
    }


}