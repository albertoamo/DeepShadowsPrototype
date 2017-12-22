using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowController : MonoBehaviour {

    public enum CharacterStatus
    {
        Physical,
        Shadow
    }

    public Material shadowMaterial;
    public Material shadowPlayer;
    public bool isShadow;

    private Light[] lights;
    private GameObject target;
    private CharacterStatus status;

    void Start()
    {
        // Initialization player variables
        Cursor.visible = false;
        status = CharacterStatus.Physical;

        GenerateShadowColliders(2, 10, 0.65f);
        lights = FindObjectsOfType(typeof(Light)) as Light[];
    }

    void FixedUpdate()
    {
        UpdateInput();
        UpdatePlayerStatus();
    }

    // Move this function when refactor needed.
    void UpdateInput()
    {
        if (Input.GetKeyDown(KeyCode.C) && isShadow == true) // Change status
        {
            ExitShadowMode();
            EnterShadowMode();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Escape button pressed");
            Cursor.visible = !Cursor.visible;
        }
    }

    public void EnterShadowMode()
    {
        if (status != CharacterStatus.Physical) return;

        status = CharacterStatus.Shadow;
        this.transform.GetChild(1).gameObject.SetActive(false);
    }

    public void ExitShadowMode()
    {
        if (status != CharacterStatus.Shadow) return;

        status = CharacterStatus.Physical;
        this.transform.GetChild(1).gameObject.SetActive(true);
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

    // Refactored function
    public bool IsPlayerInShadows()
    {
        for (int i = 0; i < target.transform.childCount; i++)
        {
            if (!IsPointInShadows(target.transform.GetChild(i).transform.position))
                return false;
        }

        return true;
    }

    public bool IsPlayerInShadowsLed()
    {
        bool shadowMode = true;

        for (int i = 0; i < target.transform.childCount; i++)
        {
            GameObject child = target.transform.GetChild(i).gameObject;

            if (IsPointInShadows(child.transform.position))
            {
                child.GetComponent<MeshRenderer>().material.color = Color.red;
            }
            else
            {
                shadowMode = false;
                child.GetComponent<MeshRenderer>().material.color = Color.blue;
            }
        }

        return shadowMode;
    }

    public void UpdatePlayerStatus()
    {
        isShadow = IsPlayerInShadowsLed();

        if (isShadow)
        {
            target.GetComponent<MeshRenderer>().material.color = Color.yellow;
            shadowPlayer.color = Color.yellow;
        }
        else
        {
            ExitShadowMode();
            target.GetComponent<MeshRenderer>().material.color = Color.green;
            shadowPlayer.color = Color.red;
        }
    }

    void GenerateShadowColliders(int levels, int amount, float radius)
    {
        target = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        target.transform.SetParent(this.transform);
        target.transform.position = this.transform.position + new Vector3(0,0.02f,0);
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
                float h = .5f * lRadius * Mathf.Cos(t * 2 * Mathf.PI) + target.transform.position.x;
                float v = .5f * lRadius * Mathf.Sin(t * 2 * Mathf.PI) + target.transform.position.z;

                PlaceSquaredLed(new Vector3(h, target.transform.position.y, v), ledScale, Color.blue);
            }
        }

        PlaceSquaredLed(target.transform.position, ledScale, Color.blue);
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