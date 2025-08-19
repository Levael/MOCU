/*using UnityEngine;


public class CubesGenerator : MonoBehaviour
{
    public GameObject cubePrefab;
    public GameObject parentObject;

    public int numberOfCubes = 69;
    public Vector3 minBounds = new Vector3(-5, -5, -5);
    public Vector3 maxBounds = new Vector3(5, 5, 5);


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
}*/

using UnityEngine;


public class CubesGenerator : MonoBehaviour
{
    public int cubeCount = 1000;
    public Vector3 bounds = new Vector3(3, 3, 3);
    public Vector3 cloudCenter = Vector3.zero;

    public Mesh cubeMesh;
    public Material cubeMaterial; // Материал с включенным GPU Instancing

    // Массив для хранения всех трансформаций (позиция, вращение, масштаб)
    private Matrix4x4[] matrices;
    private bool isVisible = false;

    private void Awake()
    {
        matrices = new Matrix4x4[cubeCount];
        RandomizeCubes();
    }

    public void RandomizeCubes()
    {
        for (int i = 0; i < cubeCount; i++)
        {
            // Случайная позиция в заданных границах (это остаётся как было)
            Vector3 localPosition = new Vector3(
                Random.Range(-bounds.x / 2, bounds.x / 2),
                Random.Range(-bounds.y / 2, bounds.y / 2),
                Random.Range(-bounds.z / 2, bounds.z / 2)
            );

            // ✨ ПРИБАВЛЯЕМ СМЕЩЕНИЕ ЦЕНТРА
            Vector3 worldPosition = cloudCenter + localPosition;

            Quaternion rotation = Quaternion.Euler(
                Random.Range(0, 360),
                Random.Range(0, 360),
                Random.Range(0, 360)
            );

            Vector3 scale = new Vector3(0.05f, 0.05f, 0.05f);

            // Используем новую позицию в матрице
            matrices[i] = Matrix4x4.TRS(worldPosition, rotation, scale);
        }
    }

    void Update()
    {
        // Если кубы видимы, рисуем их все одной командой
        if (isVisible && cubeCount > 0)
        {
            Graphics.DrawMeshInstanced(
                cubeMesh,
                0,
                cubeMaterial,
                matrices,
                cubeCount,
                null, // MaterialPropertyBlock - нам не нужен
                UnityEngine.Rendering.ShadowCastingMode.On, // Включить тени (стандартно)
                true, // Принимать тени (стандартно)
                gameObject.layer // ✨ ЗАДАЕМ НУЖНЫЙ СЛОЙ
            );
        }
    }

    public void Hide()
    {
        isVisible = false;
    }

    public void Show()
    {
        isVisible = true;
    }
}