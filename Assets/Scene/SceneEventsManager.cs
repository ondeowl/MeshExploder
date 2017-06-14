using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneEventsManager : MonoBehaviour {

    public GameObject meshExploder_Ball;
    public GameObject meshExploder_Column;

    void Start ()
    {
        StartCoroutine(DoSomeThingsinTime());
    }
	
	void Update ()
    {
        meshExploder_Ball.transform.Rotate(0, 0, Time.deltaTime * 5f);
    }

    IEnumerator DoSomeThingsinTime()
    {
        yield return new WaitForSeconds(1.0f);
        meshExploder_Ball.GetComponent<MeshExplode>().enabled = true;
        meshExploder_Column.GetComponent<MeshExplode>().enabled = true;
        yield return new WaitForSeconds(3.0f);
        StartCoroutine(SmoothExp(meshExploder_Ball, 200f));
        yield return new WaitForSeconds(2.0f);
        StartCoroutine(SmoothExp(meshExploder_Column, 50f));

    }

    IEnumerator SmoothExp(GameObject go, float noiseForce)
    {
        while (go.GetComponent<MeshExplode>().noiseAmp < noiseForce)
        {
            go.GetComponent<MeshExplode>().noiseAmp += noiseForce/5f * Time.deltaTime;
            go.GetComponent<MeshExplode>().rotationAmp = new Vector3(Random.value, Random.value, Random.value) * 50;
            yield return null;
        }
    }
}
