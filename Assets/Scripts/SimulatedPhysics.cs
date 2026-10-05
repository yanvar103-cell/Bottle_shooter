using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimulatedPhysics : MonoBehaviour
{
    [SerializeField] private Transform _environment;
    [SerializeField] private LineRenderer _line;
    [SerializeField] private int _maxPhysicsIterations = 100;
    [SerializeField] private float _pathMultiplier = 1;

    private Scene _simulatedScene;
    private PhysicsScene _physicsScene;

    //Awake (not Start) so the scene exists before BottleSpawner.Start adds bottles to it
    private void Awake()
    {
        _simulatedScene = SceneManager.CreateScene("SimulatedPhysics",
            new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        _physicsScene = _simulatedScene.GetPhysicsScene();

        foreach (Transform obstacle in _environment)
        {
            if (obstacle.CompareTag("Obstacle"))
                AddGhost(obstacle.gameObject);
        }
    }

    //Creates an invisible, frozen, script-free copy of an object in the simulated scene
    public GameObject AddGhost(GameObject original)
    {
        var ghost = Instantiate(original, original.transform.position, original.transform.rotation);
        ghost.transform.localScale = original.transform.lossyScale;   //keep world size if the original had a scaled parent

        //remove gameplay scripts (e.g. Bottle) so ghosts never count hits or call the GameManager
        foreach (var mb in ghost.GetComponentsInChildren<MonoBehaviour>(true))
            DestroyImmediate(mb);

        foreach (var r in ghost.GetComponentsInChildren<Renderer>(true))//The true also catches children that are currently inactive
            r.enabled = false;

        foreach (var rb in ghost.GetComponentsInChildren<Rigidbody>(true))
            rb.isKinematic = true;

        SceneManager.MoveGameObjectToScene(ghost, _simulatedScene);
        return ghost;
    }

    public void SimulateTrajectory(Ball ballPrefab, Vector3 position, Vector3 velocity)
    {
        var ghostBall = Instantiate(ballPrefab, position, Quaternion.identity);
        foreach (var r in ghostBall.GetComponentsInChildren<Renderer>(true))
            r.enabled = false;
        SceneManager.MoveGameObjectToScene(ghostBall.gameObject, _simulatedScene);
        ghostBall.Init(velocity);

        _line.positionCount = _maxPhysicsIterations;
        for (int i = 0; i < _maxPhysicsIterations; i++)
        {
            _physicsScene.Simulate(Time.fixedDeltaTime * _pathMultiplier);
            _line.SetPosition(i, ghostBall.transform.position);
        }

        DestroyImmediate(ghostBall.gameObject);
    }
}