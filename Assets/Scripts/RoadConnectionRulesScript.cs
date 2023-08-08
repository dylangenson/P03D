using UnityEngine;

public class RoadConnectionRulesScript : MonoBehaviour
{
    [SerializeField] private bool canConnectLeft = true;
    [SerializeField] private bool canConnectRight = true;
    [SerializeField] private bool canConnectUp = true;
    [SerializeField] private bool canConnectDown = true;


    private void Start()
    {

    }

    public bool CanConnect(GameObject otherRoad, Vector3 offset)
{
    RoadConnectionRulesScript otherRoadConnectionRules = otherRoad.GetComponent<RoadConnectionRulesScript>();

    if (otherRoadConnectionRules == null)
    {
        return false;
    }

    // Determine which sides are adjacent based on the offset
    bool adjacentTop = offset.z > 0;
    bool adjacentRight = offset.x > 0;
    bool adjacentBottom = offset.z < 0;
    bool adjacentLeft = offset.x < 0;

    // Check the connection rules based on the adjacent sides
    if (adjacentTop && canConnectUp && otherRoadConnectionRules.canConnectDown)
    {
        return true;
    }
    else if (adjacentRight && canConnectRight && otherRoadConnectionRules.canConnectLeft)
    {
        return true;
    }
    else if (adjacentBottom && canConnectDown && otherRoadConnectionRules.canConnectUp)
    {
        return true;
    }
    else if (adjacentLeft && canConnectLeft && otherRoadConnectionRules.canConnectRight)
    {
        return true;
    }
    else if (adjacentTop && !canConnectUp && !otherRoadConnectionRules.canConnectDown)
    {
        return true;
    }
    else if (adjacentRight && !canConnectRight && !otherRoadConnectionRules.canConnectLeft)
    {
        return true;
    }
    else if (adjacentBottom && !canConnectDown && !otherRoadConnectionRules.canConnectUp)
    {
        return true;
    }
    else if (adjacentLeft && !canConnectLeft && !otherRoadConnectionRules.canConnectRight)
    {
        return true;
    }

    return false;
}
}
