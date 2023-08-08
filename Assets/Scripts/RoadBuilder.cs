using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;




public class RoadBuilder : MonoBehaviour
{
    public GameObject roadPrefab;
    public GameObject roadPrefab1;
    public GameObject intersectionPrefab;
    public GameObject curvePrefab;
    public GameObject curvePrefab1;
    public GameObject curvePrefab2;
    public GameObject curvePrefab3;
    public GameObject connectorPrefab;
    public GameObject connectionIndicatorPrefab;
    public GameObject nonConnectionIndicatorPrefab;
    public GameObject buildingSmallPrefab;
    public GameObject buildingMediumPrefab;
    public GameObject buildingMonumentPrefab;
    private Vector3[] directions = { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
    private bool[,] buildingGeneratedGrid;
    public int rowCount = 5;
    public int columnCount = 5;




    private GameObject[,] grid;




    void Start()
    {
        grid = new GameObject[rowCount, columnCount];
        buildingGeneratedGrid = new bool[rowCount, columnCount];
        StartCoroutine(CreateRoadGridWithConnections());

        
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetScene();
        }
    }


    void ResetScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }


public IEnumerator CreateRoadGridWithConnections()
{
    int centerRow = rowCount / 2;
    int centerCol = columnCount / 2;

    GameObject currentPrefab = GetRandomPrefabFromAll();
    grid[centerRow, centerCol] = currentPrefab;

    bool[,] filledPositions = new bool[rowCount, columnCount];
    
    int pos = rowCount* columnCount;

    Queue<Vector2Int> positionsToCheck = new Queue<Vector2Int>();
    positionsToCheck.Enqueue(new Vector2Int(centerRow, centerCol));

    while (positionsToCheck.Count > 0)
    {   
        Vector2Int currentPosition = positionsToCheck.Dequeue();
        int row = currentPosition.x;
        int col = currentPosition.y;

        List<Vector3> validConnections = new List<Vector3>();
        foreach (Vector3 direction in directions)
        {
            int newRow = row + Mathf.RoundToInt(direction.z);
            int newCol = col + Mathf.RoundToInt(direction.x);
            if (newRow >= 0 && newRow < rowCount && newCol >= 0 && newCol < columnCount)
            {
                validConnections.Add(direction);
            }
        }

        Vector3 bestConnection = Vector3.zero;
        int minValidConnections = int.MaxValue;

        foreach (Vector3 connection in validConnections)
        {
            int neighborRow = row + Mathf.RoundToInt(connection.z);
            int neighborCol = col + Mathf.RoundToInt(connection.x);

            if (IsValidPosition(new Vector3(neighborCol, 0f, neighborRow)) && filledPositions[neighborRow, neighborCol])
            {
                int connectionsCount = CountValidConnections(new Vector3(neighborCol, 0f, neighborRow));
                if (connectionsCount < minValidConnections)
                {
                    minValidConnections = connectionsCount;
                    bestConnection = connection;
                }
            }
        }

        if (minValidConnections > 0)
        {
            if(filledPositions[row,col]!= true){
            currentPrefab = GetRandomPrefab(currentPrefab, validConnections, new Vector3(col, 0f, row));
            grid[row, col] = currentPrefab;
            PlacePrefabAtPosition(currentPrefab, row, col);
            //Debug.Log(currentPrefab.name +" Placed at ("+ row + "," + col + ")");
            filledPositions[row, col] = true;
            
            yield return new WaitForSeconds(0f); // Delay for visualization
            }
            foreach (Vector3 connection in validConnections)
            {
                int newRow = row + Mathf.RoundToInt(connection.z);
                int newCol = col + Mathf.RoundToInt(connection.x);
                positionsToCheck.Enqueue(new Vector2Int(newRow, newCol));
                bool allFilled = true;
                    foreach (bool filled in filledPositions)
                    {   
                        if (!filled)
                        {
                            allFilled = false;
                            
                            break;
                        }
                    }

                    if (allFilled)
                    {   
                        positionsToCheck.Clear();
                        GenerateBuildings();
                        Debug.Log(positionsToCheck.Count);
                        yield break; // Exit the loop
                        
                    }
            }
        }
        else
        {
            Debug.Log($"No valid connections for prefab at row: {row}, col: {col}");
        } 
    }
    }
   


    bool IsValidPosition(Vector3 position)
    {
        int row = Mathf.RoundToInt(position.z);
        int col = Mathf.RoundToInt(position.x);




        return row >= 0 && row < rowCount && col >= 0 && col < columnCount && grid[row, col] == null;
    }


    int CountValidConnections(Vector3 position)
    {
        int count = 0;
        foreach (Vector3 offset in new Vector3[] { Vector3.left, Vector3.right, Vector3.forward, Vector3.back })
        {
            if (IsValidPosition(position + offset))
            {
                count++;
            }
        }
        return count;
    }


    void PlacePrefabAtPosition(GameObject prefab, int row, int col)
{
    if (prefab == null)
    {
        //Debug.LogError("Attempted to place a null prefab at position.");
        return;
    }




    Vector3 position = new Vector3(col - (columnCount - 1) * 0.5f, 0f, row - (rowCount - 1) * 0.5f);
    GameObject newRoad = Instantiate(prefab, position, prefab.transform.rotation);
    grid[row, col] = newRoad;




    RoadConnectionRulesScript roadConnectionRules = prefab.GetComponent<RoadConnectionRulesScript>();
    if (roadConnectionRules != null)
    {
        //InstantiateConnectionIndicators(newRoad, roadConnectionRules, position);
        //InstantiateNonConnectionIndicators(newRoad, roadConnectionRules, position);
    }
}


    GameObject GetRandomPrefab(GameObject currentPrefab, List<Vector3> validConnections, Vector3 position)
{   
    List<GameObject> compatiblePrefabs = new List<GameObject>();

    foreach (GameObject prefab in new GameObject[] { roadPrefab, roadPrefab1, intersectionPrefab,  curvePrefab3, curvePrefab2, curvePrefab1, curvePrefab, connectorPrefab })
    {
        RoadConnectionRulesScript roadConnectionRules = prefab.GetComponent<RoadConnectionRulesScript>();
        if (roadConnectionRules != null)
        {
            bool isCompatible = true;

            foreach (Vector3 offset in validConnections)
            {
                int newRow = Mathf.RoundToInt(position.z) + Mathf.RoundToInt(offset.z);
                int newCol = Mathf.RoundToInt(position.x) + Mathf.RoundToInt(offset.x);

                if (newRow >= 0 && newRow < rowCount && newCol >= 0 && newCol < columnCount)
                {
                    GameObject adjacentPrefab = grid[newRow, newCol];

                    if (adjacentPrefab != null && !roadConnectionRules.CanConnect(adjacentPrefab, -offset))
                    {
                        isCompatible = false;
                    }
                }
            }

            if (isCompatible)
            {
                compatiblePrefabs.Add(prefab);
            }
            else
            {
                //Debug.Log("Incompatible prefab: " + prefab.name);
            }
        }
    }

    if (compatiblePrefabs.Count > 0)
    {
        int randomIndex = Random.Range(0, compatiblePrefabs.Count);
        return compatiblePrefabs[randomIndex];
    }

    Debug.Log("No valid prefabs found.");
    return null;
}
public void CheckPrefabsOfType(GameObject prefabToCheck)
{   
    Debug.Log("Checking...");
    for (int row = 0; row < rowCount; row++)
    {
        for (int col = 0; col < columnCount; col++)
        {
            GameObject prefabAtPosition = grid[row, col];

            if (prefabAtPosition == prefabToCheck)
            {
                Debug.Log($"Prefab of type {prefabToCheck.name} found at position ({row}, {col}).");
            }
        }
    }
}

    void InstantiateConnectionIndicators(GameObject prefab, RoadConnectionRulesScript connectionRules, Vector3 prefabPosition)
{
    foreach (Vector3 offset in directions)
    {
        if (connectionRules.CanConnect(prefab, offset))
        {
            Vector3 connectionPosition = prefabPosition + offset;
            Instantiate(connectionIndicatorPrefab, connectionPosition, Quaternion.identity);
        }
    }
}

    void InstantiateNonConnectionIndicators(GameObject prefab, RoadConnectionRulesScript connectionRules, Vector3 prefabPosition)
{
    foreach (Vector3 offset in directions)
    {
        if (!connectionRules.CanConnect(prefab, offset))
        {
            Vector3 nonConnectionPosition = prefabPosition + offset;
            Instantiate(nonConnectionIndicatorPrefab, nonConnectionPosition, Quaternion.identity);
        }
    }
}


    GameObject GetRandomPrefabFromAll()
{
    GameObject[] allPrefabs = { roadPrefab, roadPrefab1, intersectionPrefab,  curvePrefab3, curvePrefab2, curvePrefab1, curvePrefab, connectorPrefab };
    int randomIndex = Random.Range(0, allPrefabs.Length);
    return allPrefabs[randomIndex];
}

   void GenerateBuildings()
{
    for (int x = 1; x < rowCount; x++)
    {
        for (int z = 1; z < columnCount; z++)
        {
            // Check if there are ConnectorPrefab tiles at the adjacent positions
            GameObject[] adjacentPrefabs = new GameObject[]
            {
                grid[z-1, x-1], // Left Bot
                grid[z-1, x],   // Right BOT
                grid[z, x-1],   // Forward
                grid[z, x],      // Back
            };

            bool allConnectors = true;
            foreach (GameObject adjacentPrefab in adjacentPrefabs)
            {
                if (adjacentPrefab == null || adjacentPrefab.tag != "Connector")
                {
                    allConnectors = false;
                    break;
                }
            }

            // Check if a building has not been generated on this tile yet
            if (allConnectors && !buildingGeneratedGrid[z, x] && !buildingGeneratedGrid[z, x-1] && !buildingGeneratedGrid[z-1, x] && !buildingGeneratedGrid[z-1, x-1])
            {
                Vector3 centerPosition = Vector3.zero;

                foreach (GameObject adjacentPrefab in adjacentPrefabs)
                {
                    if (adjacentPrefab != null)
                    {
                        centerPosition += adjacentPrefab.transform.position;
                    }
                }

                if (adjacentPrefabs.Length > 0)
                {
                    centerPosition /= 4; // Divide by the number of adjacent prefabs (4)
                }

                // Spawn the building prefab at the calculated center position
                GameObject buildingPrefab = GetSmallBuildingPrefab();
                Instantiate(buildingPrefab, centerPosition, Quaternion.identity);
                Debug.Log($"Building spawned at center position: {centerPosition}");

                // Mark the tile as having a building generated
                buildingGeneratedGrid[z, x] = true;
            }
        }
    }
}

    GameObject GetSmallBuildingPrefab()
    {
            return buildingSmallPrefab;
    }
}