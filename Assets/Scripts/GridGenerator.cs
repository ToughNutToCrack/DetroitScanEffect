using System.Collections;
using UnityEngine;

public class GridGenerator : MonoBehaviour{
    public Material linesMaterial;
    public Material pointsMaterial;
    public Transform target;
    public int dimension;
    public GameObject pointPrefab;

    Vector3 center;
    GameObject points;

    const string ALPHA = "_Alpha";
    const float MAXTRANSPARENCY = 0.5f;

    void Start(){
        spawnPoints();
        resetMaterials();
        center = transform.position;
        center = Vector3.zero;
    }

    void Update(){
        if(Input.GetKeyUp(KeyCode.R)){
            show();
        }
    }

    void OnPostRender(){
        draw3DMatrix();
    }

    void draw3DMatrix(){    

        Vector3 offset = center + new Vector3(dimension/2, dimension/2, dimension/2);

        for(int i=0; i<=dimension; i++){
           for(int j=0; j<=dimension; j++){
                Vector3 A1 = new Vector3(0, i, j) - offset;
                Vector3 B1 = new Vector3(center.x + dimension, i, j) - offset;
                drawLine(A1, B1);

                Vector3 A2 = new Vector3(i, j, 0) - offset;
                Vector3 B2 = new Vector3(i, j, center.z + dimension) - offset;
                drawLine(A2, B2);
           }
        }

        for(int i=0; i<=dimension; i++){
           for(int j=0; j<=dimension; j++){
                Vector3 A1 = new Vector3(i, j, 0) - offset;
                Vector3 B1 = new Vector3(i, j, center.z + dimension) - offset;
                drawLine(A1, B1);

                Vector3 A2 = new Vector3(i, 0, j) - offset;
                Vector3 B2 = new Vector3(i, center.y + dimension, j) - offset;
                drawLine(A2, B2);
           }
        }
     
    }

    void drawLine(Vector3 from, Vector3 to){
        linesMaterial.SetPass(0);
        GL.Begin(GL.LINES);
        GL.Vertex(from);
        GL.Vertex(to);
        GL.End();
    }

    void spawnPoints(){
        points = new GameObject();
        Vector3 offset = center + new Vector3(dimension/2, dimension/2, dimension/2);

        for(int i=0; i<=dimension; i++){
            for(int j=0; j<=dimension; j++){
                for(int k=0; k<=dimension; k++){
                    Vector3 pos = new Vector3(i, j, k) - offset;
                    GameObject p = Instantiate(pointPrefab, pos, Quaternion.identity);
                    p.transform.SetParent(points.transform);
                }
            }
        }
    }

    void resetMaterials(){
        linesMaterial.SetFloat(ALPHA, 0);
        pointsMaterial.SetFloat(ALPHA, 0);
    }

    public void show(){
        StartCoroutine(showCoroutine());
    }
    
    public void hide(){
        StartCoroutine(hideCoroutine());
    }

    IEnumerator showCoroutine(){
        float alpha = 0;
        linesMaterial.SetFloat(ALPHA, alpha);
        pointsMaterial.SetFloat(ALPHA, alpha);
        while (alpha < MAXTRANSPARENCY){
            alpha += 0.01f;
            linesMaterial.SetFloat(ALPHA, alpha);
            pointsMaterial.SetFloat(ALPHA, alpha);
            yield return null;
        }
    }

    IEnumerator hideCoroutine(){
        float alpha = MAXTRANSPARENCY;
        linesMaterial.SetFloat(ALPHA, alpha);
        pointsMaterial.SetFloat(ALPHA, alpha);
        while (alpha > 0f){
            alpha -= 0.01f;
            linesMaterial.SetFloat(ALPHA, alpha);
            pointsMaterial.SetFloat(ALPHA, alpha);
            yield return null;
        }
    }

}
