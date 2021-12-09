using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarGraph : MonoBehaviour
{
    [SerializeField] private Material radarMaterial;
    [SerializeField] private Texture2D radarTexture2D;
    [SerializeField] private Entity.EntityType source;
    [SerializeField] private Entity.EntityType target;
    [SerializeField] private Entity.EventWeight eventWeight = new Entity.EventWeight(0, 0, 0);
    [SerializeField] private int generation = 0;
    [SerializeField] private TestManager Enviroment;

    [SerializeField] private CanvasRenderer radarMeshCanvasRenderer;
    
    private void Update()
    {
        UpdateStatsVisual();
        if (generation != Enviroment.GetGeneration())
        {
            eventWeight = Enviroment.GetEW(source, target);
            generation++;
        }
    }
    
     private void UpdateStatsVisual() 
     {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[3];
        Vector2[] uv = new Vector2[3];
        int[] triangles = new int[3];

        float angleIncrement = 360f / 3;
        float radarChartSize = 100f;

        Vector3 eatVertex = 
            Quaternion.Euler(0, 0, -angleIncrement * 0) *
            Vector3.up * radarChartSize * (float) eventWeight.GetChoice((int)Entity.ActionType.eat);
      
        Vector3 fightVertex = 
            Quaternion.Euler(0, 0, -angleIncrement * 1) *
            Vector3.up * radarChartSize * (float) eventWeight.GetChoice((int)Entity.ActionType.fight);
        
        Vector3 hideVertex = 
            Quaternion.Euler(0, 0, -angleIncrement * 2) *
            Vector3.up * radarChartSize * (float) eventWeight.GetChoice((int)Entity.ActionType.hide);


        vertices[0] = Vector3.zero;
        vertices[1] = new Vector3(0, 100);
        vertices[2] = new Vector3(100, 0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        
        
        
        //vertices[0] = Vector3.zero;
        //vertices[(int)Entity.ActionType.eat]  = eatVertex;
        //vertices[(int)Entity.ActionType.fight] = fightVertex;
        //vertices[(int)Entity.ActionType.hide]   = hideVertex;

        //uv[0]                   = Vector2.zero;
        //uv[(int)Entity.ActionType.eat]   = Vector2.one;
        //uv[(int)Entity.ActionType.fight]  = Vector2.one;

        //triangles[0] = 0;
        //triangles[1] = (int)Entity.ActionType.eat;
        //triangles[2] = (int)Entity.ActionType.fight;

        //triangles[3] = 0;
        //triangles[4] = (int)Entity.ActionType.fight;
        //triangles[5] = (int)Entity.ActionType.hide;

        //triangles[6] = 0;
        //triangles[7] = (int)Entity.ActionType.hide;
        //triangles[8] = (int)Entity.ActionType.eat;

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        
        radarMeshCanvasRenderer.SetMesh(mesh);
        radarMeshCanvasRenderer.SetMaterial(radarMaterial, null);
    }
}
