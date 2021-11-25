using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Plant : MonoBehaviour
{
	struct PlantSection
    {
		public PlantSection(char Type, float Radius)
        {
			this.type = Type;
			this.radius = Radius;
        }

		char type;
		float radius;

		public void ChangeType(char newType) { this.type = newType; }

		public void ChangeRadius(float newRad) { this.radius = newRad; }

	}

	int size;
	int power;
	List<PlantSection> build;

	private void Start()
    {
        if(build.Count < 1)
        {
			build.Add(new PlantSection('g', 1.0f));
        }
    }

}

public class PlantTest : MonoBehaviour
{
	[Header("Genetic Algorithm")]
	[SerializeField] int populationSize = 200;
	[SerializeField] float mutationRate = 0.01f;
	[SerializeField] int elitism = 5;

	private GeneticAlgorithm<Plant> ga;
	private System.Random random;

	void Start()
	{
		int dnaSize = 0;
		random = new System.Random();
		ga = new GeneticAlgorithm<Plant>(populationSize, dnaSize, random,
			GetRandomGene, FitnessFunction, elitism, mutationRate);
	}
	void Update() 
	{
		ga.NewGeneration();
	}

	private Plant GetRandomGene()
	{
		return null;
	}

	private float FitnessFunction(int index)
	{
		float score = 0;

		DNA<Plant> dna = ga.Population[index];

		score = (Mathf.Pow(2, score) - 1) / (2 - 1);

		return score;
	}
}