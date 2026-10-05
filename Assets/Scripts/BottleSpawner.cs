using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottleSpawner : MonoBehaviour
{
    [SerializeField] private Bottle[] _bottlePrefabs;        //20 bottle prefabs
    [SerializeField] private Transform _spawnPointsParent;   //empty object whose children are the spawn points
    [SerializeField] private Transform _bottlesParent;       //empty "Bottles" object to keep the Hierarchy tidy
    [SerializeField] private SimulatedPhysics _simulatedPhysics;
    //[SerializeField] private GameManager _gameManager;
    [SerializeField] private bool _randomYRotation = true;

    private void Start()
    {
        int count = 0;

        foreach (Transform point in _spawnPointsParent)
        {
            Bottle prefab = _bottlePrefabs[Random.Range(0, _bottlePrefabs.Length)];

            Quaternion rotation = point.rotation;
            if (_randomYRotation)
                rotation *= Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            Bottle bottle = Instantiate(prefab, point.position, rotation, _bottlesParent);

            //register an invisible, frozen copy for the trajectory preview
            bottle.SetGhost(_simulatedPhysics.AddGhost(bottle.gameObject));

            count++;
        }

        GameManager.Instance.StartGame(count);
    }
}
