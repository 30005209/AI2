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
    TestManager manager;

    public Herbivore()
    {
        herbivore = new EventWeight(100, 100, 100, 100, 100);
        herbivore.UpdateTotal();
        
        carnivore = new EventWeight(100, 100, 100, 100, 100);
        carnivore.UpdateTotal();
        
        omnivore = new EventWeight(100, 100, 100, 100, 100);
        omnivore.UpdateTotal();
        
        food = new EventWeight(100, 100, 100, 100, 100);
        food.UpdateTotal();
    }

    public void Start()
    {
        this.type = EntityType.herbivore;

        this.isAlive = true;
        this.damage = 20;
        this.sight = 5;
        this.energyMax = 1000;
        this.energyCur = energyMax;
        this.nutritionalValue = 300;
        this.size = 1;
        this.speed = 5;

        manager = this.GetComponentInParent<TestManager>();
    }

    public void Update()
    {
        if (mustAct)
        {
            Day();
        }

        if (energyCur <= 0)
        {
            SetAlive(false);
        }
    }

    private void Day()
    {
        List<Entity> dailyList = manager.entities;
        for (int i = 0; i < this.sight; i++)
        {
            MakeChoice(dailyList[manager.random.Next(dailyList.Count)]);
        }
        mustAct = false;
    }

    public EventWeight GetHerbivore()
    {
        return herbivore;
    }
    
    void InheritInfo(Herbivore parent0, Herbivore parent1)
    {
        List<int> p0HerbAction = parent0.GetHerbivore().actions;
        List<int> p1HerbAction = parent1.GetHerbivore().actions;
        
        herbivore = new EventWeight(
            (p0HerbAction[(int) ActionType.eat] + p1HerbAction[(int) ActionType.eat]) / 2,
            (p0HerbAction[(int) ActionType.fight] + p1HerbAction[(int) ActionType.fight]) / 2,
            (p0HerbAction[(int) ActionType.sleep] + p1HerbAction[(int) ActionType.sleep]) / 2,
            (p0HerbAction[(int) ActionType.reproduce] + p1HerbAction[(int) ActionType.reproduce]) / 2,
            (p0HerbAction[(int) ActionType.hide] + p1HerbAction[(int) ActionType.hide]) / 2);
        herbivore.UpdateTotal();
        
        carnivore = new EventWeight(100, 100, 100, 100, 100);
        carnivore.UpdateTotal();
        
        omnivore = new EventWeight(100, 100, 100, 100, 100);
        omnivore.UpdateTotal();
        
        food = new EventWeight(100, 100, 100, 100, 100);
        food.UpdateTotal();
        
    }

    private void MakeChoice(Entity targetEntity)
    {
        if (targetEntity == this) Sleep(this);
        if (energyCur > 0)
        {
            switch (targetEntity.GetEntType())
            {
                case EntityType.food:
                    switch (food.GetChoice(manager.random.Next(food.GetTotal())))
                    {
                        case ActionType.eat:
                            Eat(targetEntity);
                            food.AddWeight(ActionType.eat);
                            break;

                        case ActionType.fight:
                            Fight(targetEntity);
                            food.AddWeight(ActionType.fight);
                            break;

                        case ActionType.sleep:
                            Sleep(targetEntity);
                            food.AddWeight(ActionType.sleep);
                            break;

                        case ActionType.reproduce:
                            Reproduce(targetEntity);
                            food.AddWeight(ActionType.reproduce);
                            break;

                        case ActionType.hide:
                            Hide(targetEntity);
                            food.AddWeight(ActionType.hide);
                            break;
                    }

                    break;

                case EntityType.herbivore:
                    switch (herbivore.GetChoice(manager.random.Next(herbivore.GetTotal())))
                    {
                        case ActionType.eat:
                            Eat(targetEntity);
                            herbivore.AddWeight(ActionType.eat);
                            break;

                        case ActionType.fight:
                            Fight(targetEntity);
                            herbivore.AddWeight(ActionType.fight);
                            break;

                        case ActionType.sleep:
                            Sleep(targetEntity);
                            herbivore.AddWeight(ActionType.sleep);
                            break;

                        case ActionType.reproduce:
                            Reproduce(targetEntity);
                            herbivore.AddWeight(ActionType.reproduce);
                            break;

                        case ActionType.hide:
                            Hide(targetEntity);
                            herbivore.AddWeight(ActionType.hide);
                            break;
                    }

                    break;
                case EntityType.carnivore:
                    break;
                case EntityType.omnivore:
                    break;
            }
        }
    }

    private void Eat(Entity targetEntity)
    {
        if (targetEntity.GetEntType() == EntityType.food && targetEntity.IsAlive())
        {
            targetEntity.SetAlive(false);
            GainEnergy(targetEntity.GetNutValue());
            //print(this.name + " is eating and gained some energy");
        }
        else
        {
            //print(this.name + " tried to eat but was unable to do so");
        }
    }

    private void Fight(Entity targetEntity)
    {
        if (targetEntity.IsAlive())
        {
            energyCur -= 400;
            targetEntity.SetAlive(manager.random.Next(100) < this.damage);
            
            if (targetEntity.IsAlive())
            {
                this.SetAlive(manager.random.Next(100) < targetEntity.GetDamage());
                print(this.name + " tried to fight " + targetEntity.name + " and it fought back");
            }
            else
            {
                print(this.name + " tried to fight " + targetEntity.name + " and killed it");
            }
        }
        else
        {
            print(this.name + " tried to find a fight but was unable to do so");
        }
    }

    private void Sleep(Entity targetEntity)
    {
        //print(this.name + " found a place to nap near " + targetEntity.name);
        energyCur += 100;
    }

    private void Reproduce(Entity targetEntity)
    {        
        energyCur -= 300;
        if (targetEntity.GetEntType() == this.GetEntType())
        {
            Herbivore child = gameObject.AddComponent<Herbivore>();
            child.InheritInfo((Herbivore)this, (Herbivore)targetEntity);
            manager.entities.Add(child);
            child.transform.parent = manager.transform;
            //print(this.name + " tried to get jiggy with " + targetEntity.name);
        }
    }

    private void Hide(Entity targetEntity)
    {
        energyCur -= 100;
        //print(this.name + " tried to get hide from " + targetEntity.name);
    }

}
