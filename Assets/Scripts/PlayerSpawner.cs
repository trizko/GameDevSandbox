using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerSpawner : MonoBehaviour
{
    #region Public.
    /// <summary>
    /// Called on the server when a player is spawned.
    /// </summary>
    public event Action<NetworkObject> OnSpawned;
    #endregion

    #region Serialized.
    /// <summary>
    /// Prefab to spawn for the player.
    /// </summary>
    [Tooltip("Prefab to spawn for the player.")]
    [SerializeField]
    private NetworkObject _playerPrefab;
    /// <summary>
    /// True to add player to the active scene when no global scenes are specified through the SceneManager.
    /// </summary>
    [Tooltip("True to add player to the active scene when no global scenes are specified through the SceneManager.")]
    [SerializeField]
    private bool _addToDefaultScene = true;
    /// <summary>
    /// Areas in which players may spawn.
    /// </summary>
    [Tooltip("Areas in which players may spawn.")]
    [FormerlySerializedAs("_spawns")]//Remove on 2024/01/01
    public Transform[] Spawns = new Transform[0];
    #endregion

    #region Private.
    /// <summary>
    /// NetworkManager on this object or within this objects parents.
    /// </summary>
    private NetworkManager _networkManager;
    /// <summary>
    /// Next spawns to use.
    /// </summary>
    private int _nextSpawn;
    #endregion

    private void Start()
    {
        InitializeOnce();
    }

    private void OnDestroy()
    {
        if (_networkManager != null)
            _networkManager.SceneManager.OnClientLoadedStartScenes -= SceneManager_OnClientLoadedStartScenes;
    }


    /// <summary>
    /// Initializes this script for use.
    /// </summary>
    private void InitializeOnce()
    {
        _networkManager = gameObject.GetComponent<NetworkManager>();
        if (_networkManager == null)
        {
            Debug.LogWarning($"PlayerSpawner on {gameObject.name} cannot work as NetworkManager wasn't found on this object or within parent objects.");
            return;
        }

        _networkManager.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
    }

    /// <summary>
    /// Called when a client loads initial scenes after connecting.
    /// </summary>
    private void SceneManager_OnClientLoadedStartScenes(NetworkConnection conn, bool asServer)
    {
        if (!asServer)
            return;
        if (_playerPrefab == null)
        {
            Debug.LogWarning($"Player prefab is empty and cannot be spawned for connection {conn.ClientId}.");
            return;
        }

        Vector3 position;
        Quaternion rotation;
        SetSpawn(_playerPrefab.transform, out position, out rotation);

        NetworkObject nob = _networkManager.GetPooledInstantiated(_playerPrefab, true);
        nob.transform.SetPositionAndRotation(position, rotation);
        _networkManager.ServerManager.Spawn(nob, conn);

        //If there are no global scenes 
        if (_addToDefaultScene)
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
