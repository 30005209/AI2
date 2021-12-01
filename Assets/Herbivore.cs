using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Herbivore : Entity
{
    private System.Random random;
    TestManager manager;
    
    public void Start()
    {
        random = new System.Random();
        herbivore = new EventWeight(100, 100, 100, 100, 100);
        herbivore.UpdateTotal();
        
        carnivore = new EventWeight(100, 100, 100, 100, 100);
        carnivore.UpdateTotal();
        
        omnivore = new EventWeight(100, 100, 100, 100, 100);
        omnivore.UpdateTotal();
        
        food = new EventWeight(100, 100, 100, 100, 100);
        food.UpdateTotal();

        this.type = EntityType.herbivore;

        this.isAlive = true;
        this.damage = 20;
        this.sight = 5;
        this.energyMax = 10000;
        this.energyCur = energyMax;
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
    }

    private void Day()
    {
        List<Entity> dailyList = manager.entities;
        for (int i = 0; i < this.sight; i++)
        {
            MakeChoice(dailyList[random.Next(dailyList.Count)]);
        }
        mustAct = false;
    }

    private void MakeChoice(Entity targetEntity)
    {
        if (targetEntity == this)
        {
            
        }
        else if (energyCur > 0)
        {
            switch (targetEntity.GetEntType())
            {
                case EntityType.food:
                    switch (food.GetChoice(random.Next(food.GetTotal())))
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
                    switch (herbivore.GetChoice(random.Next(herbivore.GetTotal())))
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
            }
        }
    }

    private void Eat(Entity targetEntity)
    {
        if (targetEntity.GetEntType() == EntityType.food && targetEntity.IsAlive())
        {
            targetEntity.SetAlive(false);
            GainEnergy(targetEntity.GetNutValue());
            print(this.name + " is eating and gained some energy");
        }
        else
        {
            print(this.name + " tried to eat but was unable to do so");
        }
    }

    private void Fight(Entity targetEntity)
    {
        if (targetEntity.IsAlive())
        {
            energyCur -= 100;
            targetEntity.SetAlive(random.Next(100) < this.damage);
            
            if (targetEntity.IsAlive())
            {
                this.SetAlive(random.Next(100) < targetEntity.GetDamage());
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
        print(this.name + " found a place to nap near " + targetEntity.name);
        energyCur += 50;
    }

    private void Reproduce(Entity targetEntity)
    {
        print(this.name + " tried to get jiggy with " + targetEntity.name);
    }

    private void Hide(Entity targetEntity)
    {
        print(this.name + " tried to get hide from " + targetEntity.name);
    }

}
