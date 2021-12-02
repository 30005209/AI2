using System;
using System.Collections;
using System.Collections.Generic;
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
        sleep = 2,
        reproduce = 3,
        hide = 4,
    }

    [System.Serializable]
    public struct EventWeight
    {
        public List<int> actions;
        public int curTotal;
        
        public EventWeight(int eat, int fight, int sleep, int repro, int hide)
        {
            actions = new List<int> {eat, fight, sleep, repro, hide};

            this.curTotal = 10000;
        }
        
        public void UpdateTotal()
        {
            this.curTotal = 0;
            foreach (int i in actions)
            {
                this.curTotal += i;
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

            if (input > 0) choiceMade = ActionType.sleep;
            input -= actions[(int) ActionType.sleep];

            if (input > 0) choiceMade = ActionType.fight;
            input -= actions[(int) ActionType.fight];

            if (input > 0) choiceMade = ActionType.reproduce;
            input -= actions[(int) ActionType.reproduce];
            
            if (input > 0) choiceMade = ActionType.hide;

            return choiceMade;
        }

        public void AddWeight(ActionType givenType)
        {
            actions[(int) givenType] += 10;
        }

        public void RemoveWeight(ActionType givenType)
        {
            actions[(int)givenType] -= 10;
        }
        
    }

    [SerializeField] protected int energyMax;
    [SerializeField] protected int damage;
    [SerializeField] protected int energyCur;

    [SerializeField] protected EventWeight herbivore;
    [SerializeField] protected EventWeight carnivore;
    [SerializeField] protected EventWeight omnivore;
    [SerializeField] protected EventWeight food;

    [SerializeField] protected EntityType type;

    [SerializeField] protected bool isAlive = true;
    [SerializeField] protected bool mustAct = true;

    public string causeOfDeath = "";
    
    public EntityType GetEntType()
    {
        return this.type;
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

    public void Update()
    {
        
    }
    
}