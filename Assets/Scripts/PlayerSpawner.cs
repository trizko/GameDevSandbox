using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerSpawner : MonoBehaviour
{
    // Called on the server when a player is spawned.
    public event Action<NetworkObject> OnSpawned;

    // Prefab to spawn for the player.
    [SerializeField] private NetworkObject _playerPrefab;
    // Areas in which players may spawn.
    public Transform[] Spawns = new Transform[0];

    // NetworkManager on this object or within this objects parents.
    private NetworkManager _networkManager;
    private int _nextSpawn = 0;


    private void Start()
    {
        InitializeOnce();
    }

    private void OnDestroy()
    {
        if (_networkManager != null)
            _networkManager.SceneManager.OnClientLoadedStartScenes -= SceneManager_OnClientLoadedStartScenes;
    }

    private void InitializeOnce()
    {
        _networkManager = gameObject.GetComponent<NetworkManager>();
        if (_networkManager == null)
        {
            Debug.LogWarning($"PlayerSpawner on {gameObject.name} cannot work as NetworkManager wasn't found on this object.");
            return;
        }

        _networkManager.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
    }

    // Called when a client loads initial scenes after connecting.
    private void SceneManager_OnClientLoadedStartScenes(NetworkConnection conn, bool asServer)
    {
        if (!asServer)
            return;

        if (_playerPrefab == null)
        {
            Debug.LogWarning($"Player prefab is empty and cannot be spawned for connection {conn.ClientId}.");
            return;
        }

        Transform spawn = Spawns[_nextSpawn];
        Vector3 position = spawn.position;
        Quaternion rotation = spawn.rotation;

        Debug.Log($"spawn position: {position}");

        // Increase next spawn and reset if needed.
        _nextSpawn++;
        if (_nextSpawn >= Spawns.Length)
            _nextSpawn = 0;

        NetworkObject nob = _networkManager.GetPooledInstantiated(_playerPrefab, position, rotation, true);
        _networkManager.ServerManager.Spawn(nob, conn);
        _networkManager.SceneManager.AddOwnerToDefaultScene(nob);

        OnSpawned?.Invoke(nob);
    }

    private void SetSpawn(Transform prefab, out Vector3 pos, out Quaternion rot)
    {
        Transform result = Spawns[_nextSpawn];
        pos = result.position;
        rot = result.rotation;

        //Increase next spawn and reset if needed.
        _nextSpawn++;
        if (_nextSpawn >= Spawns.Length)
            _nextSpawn = 0;
    }
}
