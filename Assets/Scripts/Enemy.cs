﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// FSM States for the enemy
public enum EnemyState { STATIC, CHASE, REST, MOVING, DEFAULT };

public enum EnemyBehavior {EnemyBehavior1, EnemyBehavior2, EnemyBehavior3 };

public class Enemy : MonoBehaviour
{
    //pathfinding
    protected PathFinder pathFinder;
    public GenerateMap mapGenerator;
    protected Queue<Tile> path;
    protected GameObject playerGameObject;

    public Tile currentTile;
    protected Tile targetTile;
    public Vector3 velocity;

    //properties
    public float speed = 1.0f;
    public float visionDistance = 5;
    public int maxCounter = 5;
    protected int playerCloseCounter;

    protected EnemyState state = EnemyState.DEFAULT;
    protected Material material;

    public EnemyBehavior behavior = EnemyBehavior.EnemyBehavior1; 

    // Start is called before the first frame update
    void Start()
    {
        path = new Queue<Tile>();
        pathFinder = new PathFinder();
        playerGameObject = GameObject.FindWithTag("Player");
        playerCloseCounter = maxCounter;
        material = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (mapGenerator.state == MapState.DESTROYED) return;

        // Stop Moving the enemy if the player has reached the goal
        if (playerGameObject.GetComponent<Player>().IsGoalReached() || playerGameObject.GetComponent<Player>().IsPlayerDead())
        {
            //Debug.Log("Enemy stopped since the player has reached the goal or the player is dead");
            return;
        }

        switch(behavior)
        {
            case EnemyBehavior.EnemyBehavior1:
                HandleEnemyBehavior1();
                break;
            case EnemyBehavior.EnemyBehavior2:
                HandleEnemyBehavior2();
                break;
            case EnemyBehavior.EnemyBehavior3:
                HandleEnemyBehavior3();
                break;
            default:
                break;
        }

    }

    public void Reset()
    {
        Debug.Log("enemy reset");
        path.Clear();
        state = EnemyState.DEFAULT;
        currentTile = FindWalkableTile();
        transform.position = currentTile.transform.position;
    }

    Tile FindWalkableTile()
    {
        Tile newTarget = null;
        int randomIndex = 0;
        while (newTarget == null || !newTarget.mapTile.Walkable)
        {
            randomIndex = (int)(Random.value * mapGenerator.width * mapGenerator.height - 1);
            newTarget = GameObject.Find("MapGenerator").transform.GetChild(randomIndex).GetComponent<Tile>();
        }
        return newTarget;
    }

    // Dumb Enemy: Keeps Walking in Random direction, Will not chase player
    private void HandleEnemyBehavior1()
    {
        switch (state)
        {
            case EnemyState.DEFAULT: // generate random path 
                
                //Changed the color to white to differentiate from other enemies
                material.color = Color.white;
                
                if (path.Count <= 0) path = pathFinder.RandomPath(currentTile, 20);

                if (path.Count > 0)
                {
                    targetTile = path.Dequeue();
                    state = EnemyState.MOVING;
                }
                break;

            case EnemyState.MOVING:
                //move
                velocity = targetTile.gameObject.transform.position - transform.position;
                transform.position = transform.position + (velocity.normalized * speed) * Time.deltaTime;
                
                //if target reached
                if (Vector3.Distance(transform.position, targetTile.gameObject.transform.position) <= 0.05f)
                {
                    currentTile = targetTile;
                    state = EnemyState.DEFAULT;
                }

                break;
            default:
                state = EnemyState.DEFAULT;
                break;
        }
    }

    // TODO: Enemy chases the player when it is nearby
    private void HandleEnemyBehavior2()
    {
        // Finds the player GameObject using its tag
        GameObject playerObject = GameObject.FindWithTag("Player");

        switch (state)
        {
            case EnemyState.DEFAULT: // Generates a random path or chase the player if in range
                material.color = Color.white;

                if (playerObject != null && Vector3.Distance(transform.position, playerObject.transform.position) <= visionDistance)
                {
                    // Get the Player component from the GameObject
                    Player player = playerObject.GetComponent<Player>();

                    // If the player is within vision distance, set the last known tile of the player as the target
                    Tile playerLastTile = player.currentTile; // Ensure the Player class has 'currentTile' defined

                    // Use the pathfinder to find the path to the player's last known tile
                    path = pathFinder.FindPathAStar(currentTile, playerLastTile);

                    if (path.Count > 0)
                    {
                        // Set the first tile in the path as the target and move towards it
                        targetTile = path.Dequeue();
                        state = EnemyState.MOVING;
                    }
                }
                else
                {
                    // If player is not in range, move randomly
                    if (path.Count <= 0)
                    {
                        path = pathFinder.RandomPath(currentTile, 20); // Use a step count for random pathfinding
                    }

                    if (path.Count > 0)
                    {
                        targetTile = path.Dequeue();
                        state = EnemyState.MOVING;
                    }
                }
                break;

            case EnemyState.MOVING:
                // Move towards the target tile
                velocity = targetTile.transform.position - transform.position;
                transform.position = transform.position + (velocity.normalized * speed) * Time.deltaTime;

                // If target tile is reached, update the current tile
                if (Vector3.Distance(transform.position, targetTile.transform.position) <= 0.05f)
                {
                    currentTile = targetTile;
                    state = EnemyState.DEFAULT;
                }
                break;

            default:
                state = EnemyState.DEFAULT;
                break;
        }
    }

    // TODO: Third behavior (Describe what it does)
    private void HandleEnemyBehavior3()
    {
        // Finds the player GameObject using its tag
        GameObject playerObject = GameObject.FindWithTag("Player");

        switch (state)
        {
            case EnemyState.DEFAULT: // Generates a random path or chase the player if in range
                material.color = Color.white; 

                if (playerObject != null && Vector3.Distance(transform.position, playerObject.transform.position) <= visionDistance)
                {
                    // Get the Player component from the GameObject
                    Player player = playerObject.GetComponent<Player>();

                    // If the player is within vision distance, set the last known tile of the player as the target
                    Tile playerLastTile = player.currentTile; // Ensure the Player class has 'currentTile' defined
                 
                    // Use the pathfinder to find the path to the player's last known tile
                    path = pathFinder.FindPathAStar(currentTile, playerLastTile);

                    if (path.Count > 0)
                    {
                        // Set the first tile in the path as the target and move towards it
                        targetTile = path.Dequeue();
                        state = EnemyState.MOVING;
                    }
                }
                else
                {
                    // If player is not in range, move randomly
                    if (path.Count <= 0)
                    {
                        path = pathFinder.RandomPath(currentTile, 20); // Use a step count for random pathfinding
                    }

                    if (path.Count > 0)
                    {
                        targetTile = path.Dequeue();
                        state = EnemyState.MOVING;
                    }
                }
                break;

            case EnemyState.MOVING:
                // Move towards the target tile
                velocity = targetTile.transform.position - transform.position;
                transform.position = transform.position + (velocity.normalized * speed) * Time.deltaTime;

                // If target tile is reached, update the current tile
                if (Vector3.Distance(transform.position, targetTile.transform.position) <= 0.05f)
                {
                    currentTile = targetTile;
                    state = EnemyState.DEFAULT;
                }
                break;

            default:
                state = EnemyState.DEFAULT;
                break;
        }
    }
}
