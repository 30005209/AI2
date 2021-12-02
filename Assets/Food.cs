using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Food : Entity
{
    TestManager manager;

    public Food()
    {
        herbivore = new EventWeight(10, 10, 10, 10, 10);
        herbivore.UpdateTotal();

        carnivore = new EventWeight(10, 10, 10, 10, 10);
        carnivore.UpdateTotal();

        omnivore = new EventWeight(10, 10, 10, 10, 10);
        omnivore.UpdateTotal();

        food = new EventWeight(10, 10, 10, 10, 10);
        food.UpdateTotal();


        this.energyCur = energyMax;
    }

    private void Start()
    {
        this.type = EntityType.food;
        this.energyMax = 1000;
        this.energyCur = this.energyMax;
        this.isAlive = true;
        manager = this.GetComponentInParent<TestManager>();
    }

    new public void Update()
    {
        if (mustAct)
        {
            Day();
        }

        if (energyCur <= 0)
        {
            SetAlive(false);
            causeOfDeath = "Starved";
            manager.groundEnergy += GetEnergyMax()/2;
        }
    }

    private void Day()
    {
        if(manager.entities.Count > 1)
        {
            MakeChoice(manager.entities[manager.random.Next(manager.entities.Count)]);
        }

        mustAct = false;
    }

    public EventWeight GetHerbivore()
    {
        return herbivore;
    }

    public EventWeight GetFood()
    {
        return food;
    }

    void InheritInfo(Food parent0, Food parent1)
    {
        List<int> p0HerbAction = parent0.GetHerbivore().actions;
        List<int> p1HerbAction = parent1.GetHerbivore().actions;

        herbivore = new EventWeight(
            (p0HerbAction[(int)ActionType.eat] + p1HerbAction[(int)ActionType.eat]) / 2,
            (p0HerbAction[(int)ActionType.fight] + p1HerbAction[(int)ActionType.fight]) / 2,
            (p0HerbAction[(int)ActionType.sleep] + p1HerbAction[(int)ActionType.sleep]) / 2,
            (p0HerbAction[(int)ActionType.reproduce] + p1HerbAction[(int)ActionType.reproduce]) / 2,
            (p0HerbAction[(int)ActionType.hide] + p1HerbAction[(int)ActionType.hide]) / 2);
        herbivore.UpdateTotal();

        carnivore = new EventWeight(100, 100, 100, 100, 100);
        carnivore.UpdateTotal();

        omnivore = new EventWeight(100, 100, 100, 100, 100);
        omnivore.UpdateTotal();


        List<int> p0FoodAction = parent0.GetHerbivore().actions;
        List<int> p1FoodAction = parent1.GetHerbivore().actions;

        food = new EventWeight(
            (p0FoodAction[(int)ActionType.eat] + p1FoodAction[(int)ActionType.eat]) / 2,
            (p0FoodAction[(int)ActionType.fight] + p1FoodAction[(int)ActionType.fight]) / 2,
            (p0FoodAction[(int)ActionType.sleep] + p1FoodAction[(int)ActionType.sleep]) / 2,
            (p0FoodAction[(int)ActionType.reproduce] + p1FoodAction[(int)ActionType.reproduce]) / 2,
            (p0FoodAction[(int)ActionType.hide] + p1FoodAction[(int)ActionType.hide]) / 2);
        food.UpdateTotal();

    }

    private void MakeChoice(Entity targetEntity)
    {
        print("Herbivore has chosen a " + targetEntity.GetEntType());
        if (energyCur > 0 && targetEntity != this)
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
        if (manager.groundEnergy > 0)
        {
            manager.groundEnergy -= 200;
            ChangeEnergyLevel(100);
        }
        else
        {
            ChangeEnergyLevel(-100);
        }
    }

    private void Fight(Entity targetEntity)
    {
        ChangeEnergyLevel(-100);
        MakeChoice(targetEntity);
    }

    private void Sleep(Entity targetEntity)
    {
        ChangeEnergyLevel(-100);
        MakeChoice(targetEntity);
    }

    private void Reproduce(Entity targetEntity)
    {
        if(targetEntity.GetEntType() == EntityType.food)
        {
            ChangeEnergyLevel(-500);
            Food child = gameObject.AddComponent<Food>();
            child.InheritInfo((Food)this, (Food)targetEntity);
            manager.entities.Add(child);
            child.energyCur = 500;
            child.transform.parent = manager.transform;
            //print(this.name + " tried to get jiggy with " + targetEntity.name);
        }
        else
        {
            ChangeEnergyLevel(-100);
            MakeChoice(targetEntity);
        }
    }

    private void Hide(Entity targetEntity)
    {
        ChangeEnergyLevel(-100);
     
        //print(this.name + " tried to get hide from " + targetEntity.name);
    }

}
