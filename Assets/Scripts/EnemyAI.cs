using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{

	DeBugger deBugger;

	public NavMeshAgent agent;
	public Transform player;
	public LayerMask whatIsGround, whatIsPlayer;

	//Patrolling
	public Vector3 walkpoint;
	bool walkPointSet;
	public float walkPointRange;

	//Attacking
	public float timeBetweenAttacks;
	bool alreadyAttacked;

	//States
	public float attackRange;
	public bool playerInAttackRange;

	List<Transform> players;

	void CheckDebugMode()
	{
		deBugger = GameObject.Find("GameController").GetComponent<DeBugger>();
	}

	void Awake()
	{
		CheckDebugMode();

		Debug.Log("GameObject: " + gameObject + " | Method: [" + System.Reflection.MethodBase.GetCurrentMethod() + "] started.");

		if (deBugger.IsEnabled() == true) Debug.Log("Establishing player list for AI targeting.");
		players = new List<Transform>();
		FindPlayers();
	}

	void FindPlayers()
	{

		if (deBugger.IsEnabled() == true) Debug.Log("Finding players:");

		foreach (GameObject node in GameObject.FindGameObjectsWithTag("Node"))
		{
			players.Add(node.transform);
			if (deBugger.IsEnabled() == true) Debug.Log("- Found: " + node.transform);
		}
	}

	void Update()
	{
		//Get random player
		if (players.Count > 0 && player == null)
		{
			int randomIndex = Random.Range(0, players.Count);
			player = players[randomIndex];
			if (deBugger.IsEnabled() == true) Debug.Log(gameObject + " enemy AI has targeted the following player: " + players[randomIndex]);
		}
		else
		{
			//Try and find some players
			FindPlayers();
		}

		if (player == null)
		{
			Patrolling();
		}
		else
		{
			ChasePlayer();
		}


	}

	void Patrolling()
	{
		if (deBugger.IsEnabled() == true) Debug.Log("Enemy AI " + gameObject + " is patrolling.");

		if (!walkPointSet) SearchWalkPoint();

		if (walkPointSet)
			agent.SetDestination(walkpoint);

		Vector3 distanceToWalkPOint = transform.position - walkpoint;

		//Walkpoint reached
		if (distanceToWalkPOint.magnitude < 1f)
		{
			walkPointSet = false;
		}



	}

	void SearchWalkPoint()
	{
		//Calculate random point in range
		float randomX = Random.Range(-walkPointRange, walkPointRange);
		float randomZ = Random.Range(-walkPointRange, walkPointRange);

		walkpoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

		if (Physics.Raycast(walkpoint, -transform.up, 2f, whatIsGround))
		{
			walkPointSet = true;
		}

	}

	void ChasePlayer()
	{
		agent.SetDestination(player.position);
		transform.LookAt(player);
	}

	void AttackPlayer()
	{
		//Make sure enemy doesn't move
		agent.SetDestination(transform.position);
		transform.LookAt(player);

		if (!alreadyAttacked)
		{

			//Attack code here

			alreadyAttacked = true;
			Invoke(nameof(ResetAttack), timeBetweenAttacks);
		}

	}

	void ResetAttack()
	{
		alreadyAttacked = false;
	}

	void TryAvoid()
	{

	}

	void TryRevive()
	{

	}

	void RunAway()
	{
	}
}
