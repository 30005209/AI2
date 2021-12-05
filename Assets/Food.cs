using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Debug = UnityEngine.Debug;

public class Food : Entity
{
    private void Start()
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
        
        this.type = EntityType.food;
        this.energyMax = 1000;
        this.energyCur = 500;
        this.isAlive = true;
    }

    protected override void Eat(Entity targetEntity)
    {
        if (manager.groundEnergy > 0)
        {
            manager.groundEnergy -= 100;
            ChangeEnergyLevel(100);
        }
    }

    protected override void Fight(Entity targetEntity)
    {
        ChangeEnergyLevel(-100);
    }

    protected override void Reproduce(Entity targetEntity)
    {
        if(targetEntity.GetEntType() == EntityType.food)
        {
            ChangeEnergyLevel(-250);
            Food child = gameObject.AddComponent<Food>();
            child.InheritInfo((Food)this, (Food)targetEntity);
            manager.entities.Add(child);
            child.energyCur = 500;
            child.transform.parent = manager.transform;
        }
    }

    protected override void Hide(Entity targetEntity)
    {
        ChangeEnergyLevel(-100);
    }

    public override void Update()
    {
        base.Update();
    }
}
