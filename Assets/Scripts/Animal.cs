using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Layers
{
    plants = 8,
    animals = 9
}
public class Animal : MonoBehaviour
{
    [SerializeField] private int size = 1;
    [SerializeField] private int nutrition = 1;
    [SerializeField] private int maxEnergy = 10;
    [SerializeField] private float energy = 10;
    [SerializeField] private float sightRange = 1;
    [SerializeField] private float plantTarg = 0.5f;
    //[SerializeField] private Planmt curTarg = null;
    [SerializeField] private Collider curTarg = null;
    private Collider[] targets;
    
    private GeneticAlgorithm<int[]> ga;
    private System.Random random;


    void Start()
    {
        random = new System.Random();
    }

    void Update()
    {
        nutrition = (int)energy + (size * 50);
                
        if (curTarg == null) SeekTarget();
    }

    void GainEnergy(float amount)
    {
        if (amount < 0) Debug.LogError(this.name + " tried to gain negative energy");
        else energy += amount;

        if (energy > maxEnergy) energy = maxEnergy;

    }

    void UseEnergy(float amount)
    {
        if (amount < 0) Debug.LogError(this.name + " tried to use negative energy");
        else energy -= amount;

        if (energy < 0) Die();

    }

    void Die()
    {
        Debug.Log(this.name + " died");

        this.GetComponentInParent<GameObject>().SetActive(false);
    }

    void SeekTarget()
    {
        //float desire = (float)random.NextDouble();
        //Debug.Log(desire);
        if (true)
        {                       
            targets = Physics.OverlapSphere(this.transform.position, this.sightRange);

            float targDist = this.sightRange;

            foreach (Collider collider in targets)
            {
                if (collider.GetComponentInParent<Animal>() != null &&
                    collider != this.GetComponentInParent<Collider>())
                {
                    if (Vector3.Distance(collider.transform.position, this.transform.position) < targDist)
                    {
                        curTarg = collider;
                    }
                }
            }    
            
        }
    }


}
