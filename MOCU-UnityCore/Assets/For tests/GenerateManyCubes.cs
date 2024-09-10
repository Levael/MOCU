using UnityEngine;

public class CubesGenerator : MonoBehaviour
{
    public GameObject cubePrefab;
    public GameObject parentObject;

    public int numberOfCubes = 69;
    public Vector3 minBounds = new Vector3(-5, -5, -5);
    public Vector3 maxBounds = new Vector3(5, 5, 5);

    void Start()
    {
        GenerateCubes();
    }


    void GenerateCubes()
    {
        for (int i = 0; i < numberOfCubes; i++)
        {
            float x = Random.Range(minBounds.x, maxBounds.x);
            float y = Random.Range(minBounds.y, maxBounds.y);
            float z = Random.Range(minBounds.z, maxBounds.z);
            Vector3 randomPosition = new Vector3(x, y, z);

            Quaternion randomRotation = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));

            GameObject cubeInstance = Instantiate(cubePrefab, randomPosition, randomRotation, parentObject.transform);
            cubeInstance.layer = parentObject.layer;
        }
    }
}
