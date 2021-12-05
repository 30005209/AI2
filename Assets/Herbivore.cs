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
        
        this.type = EntityType.herbivore;
        this.damage = 20;
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
            targetEntity.ChangeEnergyLevel(-1000);
        }
    }

    protected override void Fight(Entity targetEntity)
    {
        ChangeEnergyLevel(-100);
        if (targetEntity.GetEntType() != EntityType.food)
        {
            ChangeEnergyLevel(-100);

            targetEntity.SetAlive(manager.random.Next(100) < this.damage);

            if (targetEntity.IsAlive()) targetEntity.MakeFight(this);
        }
    }

    protected override void Reproduce(Entity targetEntity)
    {
        if(targetEntity.GetEntType() == EntityType.herbivore)
        {
            ChangeEnergyLevel(-250);
            Herbivore child = gameObject.AddComponent<Herbivore>();
            if (this != null && targetEntity != null)
            {
                child.InheritInfo((Herbivore) this, (Herbivore) targetEntity);
            }
            manager.entities.Add(child);
            child.energyCur = 500;
            child.transform.parent = manager.transform;
        }
    }

    protected override void Hide(Entity targetEntity)
    {
        if (targetEntity.GetEntType() != EntityType.food)
        {
            ChangeEnergyLevel(-100);

            if (manager.random.Next(100) > damage)
            {
                targetEntity.MakeFight(this);
            }
        }
    }

    public override void Update()
    {
        base.Update();
    }

}
