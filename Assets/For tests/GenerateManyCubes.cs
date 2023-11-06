using UnityEngine;

public class CubesGenerator : MonoBehaviour
{
    public GameObject cubePrefab; // Link to prefab (make it from code later)
    public GameObject parentObject;

    public int numberOfCubes = 100; // Количество кубиков, которые вы хотите создать
    public Vector3 minBounds = new Vector3(-5, -5, -5); // Минимальные пределы
    public Vector3 maxBounds = new Vector3(5, 5, 5); // Максимальные пределы

    void Start()
    {
        GenerateCubes();
    }


    void GenerateCubes()
    {
        for (int i = 0; i < numberOfCubes; i++)
        {
            // Генерация случайных координат
            float x = Random.Range(minBounds.x, maxBounds.x);
            float y = Random.Range(minBounds.y, maxBounds.y);
            float z = Random.Range(minBounds.z, maxBounds.z);
            Vector3 randomPosition = new Vector3(x, y, z);

            // Генерация случайного поворота
            Quaternion randomRotation = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));

            // Создание кубика на основе prefab
            GameObject cubeInstance = Instantiate(cubePrefab, randomPosition, randomRotation, parentObject.transform);
            //Instantiate(cubePrefab, randomPosition, randomRotation);
        }
    }
}
