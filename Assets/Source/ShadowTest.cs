using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowTest : MonoBehaviour {

    public enum CharacterStatus
    {
        Physical,
        Shadow
    }

    public Material shadowMaterial;
    public Material shadowPlayer;
    private CharacterStatus status;

    private Light[] lights;
    private GameObject target;

    void Start()
    {

        // Initialization player variables
        Cursor.visible = false;
        status = CharacterStatus.Physical;

        GenerateShadowColliders(2, 10, 0.65f);
        lights = FindObjectsOfType(typeof(Light)) as Light[];
    }

    void Update()
    {
        UpdateInput();
        UpdatePlayerStatus();
    }

    void UpdateInput()
    {
        if (status == CharacterStatus.Physical)
        {
            if (Input.GetKeyDown(KeyCode.C)) // Change status
            {

            }
        }
        else if (status == CharacterStatus.Shadow)
        {

        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Escape button pressed");
            Cursor.visible = !Cursor.visible;
        }
    }

    public bool IsPointInShadows(Vector3 point)
    {
        foreach (Light light in lights)
        {
            // If it has a collider, and point is outside, don't check this light
            if (light.gameObject.transform.childCount > 0 && !light.gameObject.transform.GetChild(0).GetComponent<Collider>().bounds.Contains(point))
                continue;

            float maxDist = Vector3.Distance(point, light.transform.position); // Optimize this part in the future
            if (!Physics.Raycast(point + new Vector3(0, 0.02f, 0), -light.transform.forward, maxDist, 5))
                return false;
        }

        return true;
    }

    void UpdatePlayerStatus()
    {
        bool isShadow = true;

        for (int i = 0; i < target.transform.childCount; i++)
        {
            GameObject child = target.transform.GetChild(i).gameObject;

            if (IsPointInShadows(child.transform.position))
            {
                child.GetComponent<MeshRenderer>().material.color = Color.red;
            }
            else
            {
                isShadow = false;
                child.GetComponent<MeshRenderer>().material.color = Color.blue;
            }
        }

        if (isShadow)
        {
            target.GetComponent<MeshRenderer>().material.color = Color.yellow;
            shadowPlayer.color = Color.yellow;
        }
        else
        {
            target.GetComponent<MeshRenderer>().material.color = Color.green;
            shadowPlayer.color = Color.red;
        }
    }

    void GenerateShadowColliders(int levels, int amount, float radius)
    {
        target = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        target.transform.SetParent(this.transform);
        target.transform.position = this.transform.position;
        target.transform.rotation = this.transform.rotation;
        target.transform.localScale = new Vector3(radius, 0.001f, radius);
        target.GetComponent<MeshRenderer>().material = shadowMaterial;
        target.GetComponent<MeshRenderer>().material.color = Color.green;
        Vector3 ledScale = new Vector3(0.05f, 0.05f, 0.05f);
        Destroy(target.GetComponent<CapsuleCollider>());

        for (int x = 0; x < levels; x++)
        {
            for (int y = 0; y < amount; y++)
            {
                float lRadius = radius / (x + 1);
                float t = y / (float)amount;
                float h = .5f * lRadius * Mathf.Cos(t * 2 * Mathf.PI) + target.transform.localPosition.x;
                float v = .5f * lRadius * Mathf.Sin(t * 2 * Mathf.PI) + target.transform.localPosition.z;

                PlaceSquaredLed(new Vector3(h, target.transform.localPosition.y, v), ledScale, Color.blue);
            }
        }

        PlaceSquaredLed(target.transform.localPosition, ledScale, Color.blue);
    }

    void PlaceSquaredLed(Vector3 position, Vector3 scale, Color color)
    {
        GameObject childSource = GameObject.CreatePrimitive(PrimitiveType.Cube);
        childSource.transform.SetParent(target.transform);
        childSource.transform.position = position + new Vector3(0, 0.02f, 0);
        childSource.transform.localScale = scale;
        childSource.GetComponent<MeshRenderer>().material = shadowMaterial;
        childSource.GetComponent<MeshRenderer>().material.color = color;
        Destroy(childSource.GetComponent<BoxCollider>());
    }

}