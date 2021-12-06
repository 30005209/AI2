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

public class Carnivore : Entity
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

        this.type = EntityType.carnivore;
        this.damage = 50;
        this.energyMax = 1000;
        this.energyCur = 500;
        this.isAlive = true;
    }

    protected override void Eat(Entity targetEntity)
    {
        // Finding a non food entity
        if (targetEntity.GetEntType() != EntityType.food)
        {
            if (targetEntity.IsAlive())
            {
                Fight(targetEntity);
            }

            if (!targetEntity.IsAlive())
            {
                if (targetEntity.GetEnergyCur() > 500)
                {
                    ChangeEnergyLevel(targetEntity.GetEnergyCur());
                }
                else
                {
                    ChangeEnergyLevel(500);
                }
            }
        }
        else
        {
            ChangeEnergyLevel(-500);
        }
    }

    protected override void Fight(Entity targetEntity)
    {
        // If it's not food fight it
        if (targetEntity.GetEntType() != EntityType.food &&
            targetEntity.GetEntType() != EntityType.carnivore)
        {
            ChangeEnergyLevel(-100);

            targetEntity.SetAlive(manager.random.Next(100) > this.damage);

            if (!targetEntity.IsAlive())
            {
                Eat(targetEntity);
                targetEntity.causeOfDeath = "Killed and Eaten by Carnivore";
            }
        }
        else
        {
            ChangeEnergyLevel(-300);
        }
    }

    protected override void Reproduce(Entity targetEntity)
    {
        if(targetEntity.GetEntType() == EntityType.carnivore)
        {
            if (this != null && targetEntity as Carnivore != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    ChangeEnergyLevel(-50);
                    Carnivore child = gameObject.AddComponent<Carnivore>();
                    child.InheritInfo(this, (Carnivore) targetEntity);
                    manager.entities.Add(child);
                    child.energyCur = 500;
                    child.transform.parent = manager.transform;
                }
            }
        }
    }

    protected override void Hide(Entity targetEntity)
    {
        if (targetEntity.GetEntType() != EntityType.food ||
            targetEntity.GetEntType() != EntityType.carnivore)
        {
            ChangeEnergyLevel(-100);
            if (manager.random.Next(100) < damage)
            {
                ChangeEnergyLevel(-100);
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
        if (!isAlive) print(causeOfDeath);
    }

}
