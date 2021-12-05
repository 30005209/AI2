using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Herbivore : Entity
{
    public Herbivore()
    {
        this.food = new EventWeight(manager.random.Next(10, 100),
            manager.random.Next(10, 100),
            manager.random.Next(10, 100));
        this.food.UpdateTotal();
        
        this.herbivore = new EventWeight(manager.random.Next(10, 100),
            manager.random.Next(10, 100),
            manager.random.Next(10, 100));
        this.herbivore.UpdateTotal();
        
        this.carnivore = new EventWeight(manager.random.Next(10, 100),
            manager.random.Next(10, 100),
            manager.random.Next(10, 100));
        this.carnivore.UpdateTotal();
        
        this.omnivore = new EventWeight(manager.random.Next(10, 100),
            manager.random.Next(10, 100),
            manager.random.Next(10, 100));
        this.omnivore.UpdateTotal();
    }

    private void Start()
    {
        this.type = EntityType.herbivore;
        this.damage = 20;
        this.energyMax = 1000;
        this.energyCur = this.energyMax;
        this.isAlive = true;
        manager = this.GetComponentInParent<TestManager>();
    }

    private new void Eat(Entity targetEntity)
    {
        if (targetEntity.GetEntType() == EntityType.food && targetEntity.IsAlive())
        {
            targetEntity.SetAlive(false);
            targetEntity.causeOfDeath = "Eaten";
            ChangeEnergyLevel(targetEntity.GetEnergyMax()/2);
        }
        else
        {
            print("Herbivore tried to eat but was unable to do so");
        }
    }

    private new void Fight(Entity targetEntity)
    {
        ChangeEnergyLevel(-100);
        if (targetEntity.IsAlive() && targetEntity.GetEntType() != EntityType.food)
        {
            ChangeEnergyLevel(-150);
            targetEntity.SetAlive(manager.random.Next(100) < this.damage);

            if (targetEntity.IsAlive())
            {
                this.SetAlive(manager.random.Next(100) < targetEntity.GetDamage());

                if (targetEntity.GetEntType() == EntityType.herbivore)
                {
                    herbivore.RemoveWeight(ActionType.fight);
                }
                if (targetEntity.GetEntType() == EntityType.carnivore)
                {
                    carnivore.RemoveWeight(ActionType.fight);
                }
                if (targetEntity.GetEntType() == EntityType.omnivore)
                {
                    omnivore.RemoveWeight(ActionType.fight);
                }
            }
            else
            {
                print(this.name + " tried to fight " + targetEntity.name + " and killed it");

                if (targetEntity.GetEntType() == EntityType.herbivore)
                {
                    herbivore.AddWeight(ActionType.fight);
                }
                if (targetEntity.GetEntType() == EntityType.carnivore)
                {
                    carnivore.AddWeight(ActionType.fight);
                }
                if (targetEntity.GetEntType() == EntityType.omnivore)
                {
                    omnivore.AddWeight(ActionType.fight);
                }
            }
        }
        else
        {
            MakeChoice(targetEntity);
        }
    }

    private new void Reproduce(Entity targetEntity)
    {
        if(targetEntity.GetEntType() == EntityType.herbivore)
        {
            Herbivore child = gameObject.AddComponent<Herbivore>(); 
            child.InheritInfo((Herbivore)this, (Herbivore)targetEntity);
            child.energyCur = 0;
            manager.entities.Add(child);
            child.transform.parent = manager.transform;
        }
    }

    private new void Hide(Entity targetEntity)
    {
        ChangeEnergyLevel(-100);
        //print(this.name + " tried to get hide from " + targetEntity.name);
    }

}
