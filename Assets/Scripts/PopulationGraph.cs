/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;
using UnityEngine;
using UnityEngine.UI;

public class PopulationGraph : MonoBehaviour {

    [SerializeField] private Sprite circleSprite;
    private RectTransform graphContainer;
    
    
    [Header("Food")]
    [SerializeField] private Color dotColourFood;
    [SerializeField] private Color lineColorFood;
    [Header("Herbivore")]
    [SerializeField] private Color dotColourHerb;
    [SerializeField] private Color lineColorHerb;
    [Header("Carnivore")]
    [SerializeField] private Color dotColourCarn;
    [SerializeField] private Color lineColorCarn;
    [Header("Omnivore")]
    [SerializeField] private Color dotColourOmni;
    [SerializeField] private Color lineColorOmni;
    
    private Color dotColour;
    private Color lineColor;
    
    [SerializeField] private float yMaximum = 100f;
    [SerializeField] private float xSize = 5f;
    [SerializeField] private TestManager Enviroment = null;
    [SerializeField] private List<int> popFood;
    [SerializeField] private List<int> popHerb;
    [SerializeField] private List<int> popCarn;
    [SerializeField] private List<int> popOmni;
    [SerializeField] private int curGeneration = 0;

    private void Awake() 
    {
        graphContainer = transform.Find("Graph Container").GetComponent<RectTransform>();

        popFood = new List<int>();
        popHerb = new List<int>();
        popCarn = new List<int>();
        popOmni = new List<int>();

        ShowGraph(popFood);
        ShowGraph(popHerb);
        ShowGraph(popCarn);
        ShowGraph(popOmni);
    }

    private void UpdateColor(Color dot, Color line)
    {
        dotColour = dot;
        lineColor = line;
    }
    private void Update()
    {
        if (curGeneration != Enviroment.GetGeneration())
        {
            AddToList(Entity.EntityType.food);
            AddToList(Entity.EntityType.herbivore);
            AddToList(Entity.EntityType.carnivore);
            AddToList(Entity.EntityType.omnivore);
            curGeneration++;
            
            UpdateColor(dotColourFood, lineColorFood);
            ShowGraph(popFood);
            
            UpdateColor(dotColourHerb, lineColorHerb);
            ShowGraph(popHerb);
            
            UpdateColor(dotColourCarn, lineColorCarn);
            ShowGraph(popCarn);
            
            UpdateColor(dotColourOmni, lineColorOmni);
            ShowGraph(popOmni);
        }
    }

    private void AddToList(Entity.EntityType type)
    {
        switch (type)
        {
            case Entity.EntityType.food:
                popFood.Add(Enviroment.GetPopulation(type));
                break;
            case Entity.EntityType.herbivore:
                popHerb.Add(Enviroment.GetPopulation(type));
                break;
            case Entity.EntityType.carnivore:
                popCarn.Add(Enviroment.GetPopulation(type));
                break;
            case Entity.EntityType.omnivore:
                popOmni.Add(Enviroment.GetPopulation(type));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private GameObject CreateCircle(Vector2 anchoredPosition) 
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        gameObject.GetComponent<Image>().color = dotColour;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(11, 11);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        return gameObject;
    }

    private void ShowGraph(List<int> valueList) 
    {
        float graphHeight = graphContainer.sizeDelta.y;

        GameObject lastCircleGameObject = null;
        for (int i = 0; i < valueList.Count; i++) {
            float xPosition = i * xSize;
            float yPosition = (valueList[i] / yMaximum) * graphHeight;
            GameObject circleGameObject = CreateCircle(new Vector2(xPosition, yPosition));
            if (lastCircleGameObject != null) {
                CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition,
                    circleGameObject.GetComponent<RectTransform>().anchoredPosition);
            }
            lastCircleGameObject = circleGameObject;
        }
    }

    private void CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB) 
    {
        GameObject gameObject = new GameObject("dotConnection", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().color = lineColor;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, 3f);
        rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * 180 / Mathf.PI);
    }

}
