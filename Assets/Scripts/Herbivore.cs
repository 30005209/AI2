using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

public class Herbivore : Entity
{
    public void Start()
    {
        manager = this.GetComponentInParent<TestManager>();

        do
        {

            this.food = new EventWeight(manager.random.Next(10, 100),
                manager.random.Next(10, 100),
                manager.random.Next(10, 100));
            this.food.UpdateTotal();
        } while (food.GetTotal() < 80);

        do
        {
            this.herbivore = new EventWeight(manager.random.Next(10, 100),
                manager.random.Next(10, 100),
                manager.random.Next(10, 100));
            this.herbivore.UpdateTotal();
        } while (herbivore.GetTotal() < 80);

        do
        {
            this.carnivore = new EventWeight(manager.random.Next(10, 100),
                manager.random.Next(10, 100),
                manager.random.Next(10, 100));
            this.carnivore.UpdateTotal();
        } while (carnivore.GetTotal() < 80);

        do
        {
            this.omnivore = new EventWeight(manager.random.Next(10, 100),
                manager.random.Next(10, 100),
                manager.random.Next(10, 100));
            this.omnivore.UpdateTotal();
        } while (omnivore.GetTotal() < 80);

        this.type = EntityType.herbivore;
        this.damage = 10;
        this.energyMax = 1000;
        this.energyCur = 500;
        this.isAlive = true;
    }

    protected override void Eat(Entity targetEntity)
    {
        ChangeEnergyLevel(-100);
        if (targetEntity.GetEntType() == EntityType.food)
        {
            ChangeEnergyLevel(targetEntity.GetEnergyCur());
            targetEntity.SetAlive(false);
            targetEntity.causeOfDeath = "Eaten by Herbivore";
            targetEntity.ChangeEnergyLevel(-1000);
        }
    }

    protected override void Fight(Entity targetEntity)
    {
        ChangeEnergyLevel(-100);
        
        // If it's not food fight it - triggering a fight
        if (targetEntity.GetEntType() != EntityType.food)
        {
            ChangeEnergyLevel(-100);

            targetEntity.SetAlive(manager.random.Next(200) < this.damage);

            if (!targetEntity.IsAlive())
            {
                targetEntity.causeOfDeath = "Killed by Herbivore";
            }
        }
        else
        {
            ChangeEnergyLevel(-200);
        }
    }

    protected override void Reproduce(Entity targetEntity)
    {
        if(targetEntity.GetEntType() == EntityType.herbivore)
        {
            if (this != null && targetEntity != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    ChangeEnergyLevel(-50);
                    Herbivore child = gameObject.AddComponent<Herbivore>();
                    child.InheritInfo(this, (Herbivore) targetEntity);
                    manager.entities.Add(child);
                    child.energyCur = 500;
                    child.transform.parent = manager.transform;
                }
            }
        }
    }

    protected override void Hide(Entity targetEntity)
    {
        if (targetEntity.GetEntType() != EntityType.food  ||
            targetEntity.GetEntType() != EntityType.herbivore)
        {
            ChangeEnergyLevel(-100);
            if (manager.random.Next(100) > damage)
            {
                targetEntity.MakeFight(this);
            }
        }
        else
        {
            ChangeEnergyLevel(-300);
        }
    }

    public override void Update()
    {
        base.Update();
    }

}
