using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimulatedPhysics : MonoBehaviour
{
    public static SimulatedPhysics Instance { get; private set; }

    [SerializeField] private Transform _environment;
    [SerializeField] private LineRenderer _line;
    [SerializeField] private int _maxPhysicsIterations = 100;
    [SerializeField] private float _pathMultiplier = 1;

    private Scene _simulatedScene;
    private PhysicsScene _physicsScene;

    //ghosts that must follow their real object (movable props)
    private readonly List<(Transform real, Rigidbody ghostRb, GameObject ghost)> _dynamicGhosts = new();

    //Awake (not Start) so the scene exists before BottleSpawner.Start adds bottles to it
    private void Awake()
    {
        Instance = this;

        // unique name, so a restart can never collide with the previous simulated scene
        _simulatedScene = SceneManager.CreateScene($"SimulatedPhysics_{GetInstanceID()}",
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

        foreach (var joint in ghost.GetComponentsInChildren<Joint>(true))
            DestroyImmediate(joint);

        foreach (var r in ghost.GetComponentsInChildren<Renderer>(true))//The true also catches children that are currently inactive
            r.enabled = false;

        foreach (var rb in ghost.GetComponentsInChildren<Rigidbody>(true))
            rb.isKinematic = true;

        SceneManager.MoveGameObjectToScene(ghost, _simulatedScene);
        return ghost;
    }

    //same as AddGhost, but the ghost is moved to match the real object before every prediction
    public GameObject AddDynamicGhost(GameObject original)
    {
        var ghost = AddGhost(original);
        _dynamicGhosts.Add((original.transform, ghost.GetComponent<Rigidbody>(), ghost));
        return ghost;
    }

    private void SyncDynamicGhosts()
    {
        for (int i = _dynamicGhosts.Count - 1; i >= 0; i--)
        {
            var (real, ghostRb, ghost) = _dynamicGhosts[i];

            // real object destroyed, or ghost removed (e.g. bottle was shot) -> forget it
            if (real == null || ghost == null)
            {
                if (ghost != null) Destroy(ghost);
                _dynamicGhosts.RemoveAt(i);
                continue;
            }

            if (ghostRb != null)
            {
                ghostRb.position = real.position;   // teleports the kinematic ghost inside the physics scene
                ghostRb.rotation = real.rotation;
            }
            else
            {
                ghost.transform.SetPositionAndRotation(real.position, real.rotation);
            }
        }
    }

    public void SimulateTrajectory(Ball ballPrefab, Vector3 position, Vector3 velocity)
    {
        SyncDynamicGhosts();

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