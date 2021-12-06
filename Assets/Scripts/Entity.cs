using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class Entity : MonoBehaviour
{
    [System.Serializable]
    public enum EntityType
    {
        food = 0,
        herbivore = 1,
        carnivore = 2,
        omnivore = 3
    }

    [System.Serializable]
    public enum ActionType
    {
        eat = 0,
        fight = 1,
        hide = 2
    }

    [System.Serializable]
    public struct EventWeight
    {
        public List<int> actions;
        public int curTotal;

        public static EventWeight operator +(EventWeight a, EventWeight b)
            => new EventWeight(
                a.actions[(int)ActionType.eat] + b.actions[(int)ActionType.eat], 
                a.actions[(int)ActionType.fight] + b.actions[(int)ActionType.fight],
                a.actions[(int)ActionType.hide] + b.actions[(int)ActionType.hide]);

        public static EventWeight operator /(EventWeight a, int b)
            => new EventWeight(a.actions[(int) ActionType.eat] / b,
                a.actions[(int) ActionType.fight] / b,
                a.actions[(int) ActionType.hide] / b);

        
        public EventWeight(int eat, int fight, int hide)
        {
            actions = new List<int> {eat, fight, hide};

            this.curTotal = 10000;
        }
        
        public void UpdateTotal()
        {
            this.curTotal = 0;
            foreach (int i in actions)
            {
                this.curTotal += i;
            }

            if (curTotal > 120)
            {
                actions[(int) ActionType.eat] -= 10;
                actions[(int) ActionType.fight] -= 10;
                actions[(int) ActionType.hide] -= 10;
            }
        }

        public int GetTotal()
        {
            UpdateTotal();
            return curTotal;
        }

        public ActionType GetChoice(int input)
        {
            ActionType choiceMade = ActionType.hide;

            if (input > 0) choiceMade = ActionType.eat;
            input -= actions[(int) ActionType.eat];

            if (input > 0) choiceMade = ActionType.fight;
            input -= actions[(int) ActionType.fight];
          
            if (input > 0) choiceMade = ActionType.hide;

            return choiceMade;
        }

        public void AddWeight(ActionType givenType)
        {
            actions[(int) givenType] += 10;
            if (actions[(int) givenType] > 100) actions[(int) givenType] = 100;
            UpdateTotal();
        }

        public void RemoveWeight(ActionType givenType)
        {
            actions[(int)givenType] -= 10;
            if (actions[(int) givenType] < 10) actions[(int) givenType] = 10;
            UpdateTotal();
        }
        
        public string OutputEWStats()
        {
            string output = null;
            
            // Eat
            output += actions[(int) ActionType.eat] + " | ";

            // Fight
            output += actions[(int) ActionType.fight] + " | ";

            // Sleep
            output += actions[(int) ActionType.hide] + "\n";
                
            return output;
        }
    }

    [SerializeField] protected int energyMax;
    [SerializeField] protected int energyCur;
    protected int energyStart;
    [SerializeField] protected int damage;

    [SerializeField] protected EventWeight herbivore;
    [SerializeField] protected EventWeight carnivore;
    [SerializeField] protected EventWeight omnivore;
    [SerializeField] protected EventWeight food;

    [SerializeField] protected EntityType type;

    [SerializeField] protected bool isAlive = true;
    [SerializeField] protected bool mustAct = true;

    protected int moveNumMax = 3;
    protected int moveNumCur = 2;
    
    protected TestManager manager;

    public string causeOfDeath = "";
    
    public EntityType GetEntType()
    {
        return this.type;
    }

    public ref EventWeight GetTargEW(Entity target)
    {
        switch (target.GetEntType())
        {
            case EntityType.food:
                return ref food;

            case EntityType.herbivore:
                return ref herbivore;

            case EntityType.carnivore:
                return ref carnivore;

            case EntityType.omnivore:
                return ref omnivore;
            
            default:
                return ref food;
        }
    }

    public EventWeight GetEW(EntityType target)
    {
        switch (target)
        {
            case EntityType.food:
                return food;
            
            case EntityType.herbivore:
                return herbivore;
            
            case EntityType.carnivore:
                return carnivore;
                
            case EntityType.omnivore:
                return omnivore;
            
            default:
                return food;
        }
    }
    
    public bool IsAlive()
    {
        return isAlive;
    }
    public void SetAlive(bool newState)
    {
        this.isAlive = newState;
    }

    public int GetDamage()
    {
        return this.damage;
    }

    public int GetEnergyStart()
    {
        return energyStart;
    }

    public int GetEnergyCur()
    {
        return this.energyCur;
    }

    public int GetEnergyMax()
    {
        return this.energyMax;
    }

    public void ChangeEnergyLevel(int amount)
    {
        this.energyCur += amount;

        if (this.energyCur < 0) this.energyCur = 0;
        if (energyCur > energyMax) energyCur = energyMax;
    }

    public bool MustAct()
    {
        return this.mustAct;
    }

    public void SetMustAct(bool newVal)
    {
        this.mustAct = newVal;
    }

    protected ref EventWeight GetEw(EntityType eType)
    {
        while (true)
        {
            switch (eType)
            {
                case EntityType.food:
                    return ref food;
                case EntityType.herbivore:
                    return ref herbivore;
                case EntityType.carnivore:
                    return ref carnivore;
                case EntityType.omnivore:
                    return ref omnivore;
                default:
                    eType = this.GetEntType();
                    break;
            }
        }
    }
    
    protected void InheritInfo<T>(T parent0, T parent1) where T : Entity
    {
        for (int i = (int) EntityType.food; i <= (int) EntityType.omnivore; i++)
        {
            List<int> p0Action = parent0.GetEw((EntityType)i).actions;
            List<int> p1Action = parent1.GetEw((EntityType)i).actions;

            GetEw((EntityType) i) = new EventWeight(
                (p0Action[(int) ActionType.eat] + p1Action[(int) ActionType.eat]) / 2,
                (p0Action[(int) ActionType.fight] + p1Action[(int) ActionType.fight]) / 2,
                (p0Action[(int) ActionType.hide] + p1Action[(int) ActionType.hide]) / 2);
            GetEw((EntityType)i).UpdateTotal();
            
        }
    }

    protected void MakeChoice<T>(T target) where T : Entity
    {
        if (energyCur > 0 && target != this)
        {
            ActOnTarget(target, GetTargEW(target).GetChoice(
                manager.random.Next(GetTargEW(target).GetTotal())));
        }
    }

    protected void ActOnTarget<T>(T target, ActionType action) where T : Entity
    {
        if (this != null && target != null)
        {
            GetTargEW(target).RemoveWeight(ActionType.eat);
            GetTargEW(target).RemoveWeight(ActionType.fight);
            GetTargEW(target).RemoveWeight(ActionType.hide);
            switch (action)
            {
                case ActionType.eat:
                    Eat(target);
                    break;
                case ActionType.fight:
                    Fight(target);
                    break;
                case ActionType.hide:
                    Hide(target);
                    break;
                default:
                    Hide(target);
                    break;
            }

            GetTargEW(target).AddWeight(action);
            GetTargEW(target).AddWeight(action);
        }
    }
    
    protected virtual void Eat(Entity target)
    {
        
    }

    protected virtual void Fight(Entity target)
    {
        
    }

    protected virtual void Reproduce(Entity target)
    {
        
    }

    protected virtual void Hide(Entity target)
    {
        
    }

    public void ResetMoves()
    {
        moveNumCur = moveNumMax;
    }

    public void NightReproduce(Entity target)
    {
        Reproduce(target);
    }

    public void MakeFight(Entity target)
    {
        Fight(target);
    }
    
    protected void Day()
    {
        if (manager.entities.Count > 1 && moveNumCur > 0)
        {
            ChangeEnergyLevel(50);
            MakeChoice(manager.entities[manager.random.Next(manager.entities.Count)]);
            moveNumCur--;
        }
    }
    
    public virtual void Update()
    {
        mustAct = moveNumCur > 0;

        if (moveNumCur == moveNumMax)
        {
            energyStart = energyCur;
        }
        
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

    
}