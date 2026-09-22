using Meta.XR.MRUtilityKit;
using UnityEngine;

/// <summary>
/// Bridges MRUK's room model to Vivian Ménard's boids.
/// Puts the room's EffectMesh colliders on the obstacle layer the boids raycast against,
/// fits the spawn area to the room, and only then lets the manager spawn the school.
/// </summary>
public class RoomReef : MonoBehaviour
{
    [SerializeField] private EntitiesManagerScript entitiesManager;
    [SerializeField] private EffectMesh[] effectMeshes;
    [SerializeField, Min(0f), Tooltip("Metres kept clear between the spawn area and the room surfaces.")]
    private float spawnMargin = 0.3f;
    [SerializeField] private string obstacleLayerName = "BoidsObstacles";

    private int desiredBoids;

    private void Awake()
    {
        int layer = LayerMask.NameToLayer(obstacleLayerName);
        if (layer < 0)
        {
            Debug.LogError($"[ 🧱 RoomReef.Awake ] Layer '{obstacleLayerName}' does not exist.");
            return;
        }

        foreach (EffectMesh effectMesh in effectMeshes)
            if (effectMesh != null)
                effectMesh.Layer = layer;

        // Hold the school back until the spawn area matches the room.
        desiredBoids = entitiesManager.NumberOfBoids;
        entitiesManager.NumberOfBoids = 0;
    }

    private void Start()
    {
        MRUK.Instance.RegisterSceneLoadedCallback(OnRoomLoaded);
    }

    private void OnRoomLoaded()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room == null)
        {
            Debug.LogError("[ 🏠 RoomReef.OnRoomLoaded ] MRUK reported a loaded scene but has no current room.");
            return;
        }

        Bounds bounds = room.GetRoomBounds();
        Vector3 size = bounds.size - 2f * spawnMargin * Vector3.one;
        size = Vector3.Max(size, 0.5f * Vector3.one);

        GameObject areaGO = entitiesManager.boidsParams.spawnAreaGO;
        areaGO.transform.position = bounds.center;
        areaGO.transform.rotation = Quaternion.identity;
        areaGO.transform.localScale = size;

        // AreaScript only reads its transform in Awake, so replace it to pick up the new bounds.
        AreaScript oldArea = areaGO.GetComponent<AreaScript>();
        if (oldArea != null)
            Destroy(oldArea);
        entitiesManager.boidsParams.spawnArea = areaGO.AddComponent<AreaScript>();

        entitiesManager.NumberOfBoids = desiredBoids;

        Debug.Log($"[ 🐟 RoomReef.OnRoomLoaded ] Room {bounds.size} at {bounds.center}. Spawning {desiredBoids} boids in {size}.");
    }
}
