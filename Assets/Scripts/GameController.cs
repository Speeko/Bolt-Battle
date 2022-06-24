using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class GameController : MonoBehaviour
{

	DeBugger deBugger;

	#region vars

	public GameObject spawnPointsGameObject;

	public List<Color> availableColours = new List<Color>();
	List<GameObject> playerNodes = new List<GameObject>();
	List<GameObject> players = new List<GameObject>();

	PlayerInputManager playerInputManager;

	#endregion

	//This script should be attached to an empty game object named "GameController".
	//It is responsible for controlling game-wide functions, other scripts should begin by grabbing some global game vars from the GetVars() method

	void CheckDebugMode()
	{
		deBugger = GetComponent<DeBugger>();
	}

	void Awake()
	{

		CheckDebugMode();

		//Get our input manager
		playerInputManager = GetComponent<PlayerInputManager>();

	}

	//Called by the InputManager when "start" is pressed
	public void IncomingPlayer(PlayerInput playerInput)
	{
		//Get our incoming player GameObject
		GameObject incomingPlayer;
		incomingPlayer = playerInput.gameObject;

		//Set the player position to a clear spawn point
		incomingPlayer.transform.position = GetClearSpawn();

		//Pick a random colour from the list of colours
		int randomIndex = Random.Range(0, availableColours.Count);

		//Get the PlayerController script
		PlayerController playerController = incomingPlayer.GetComponent<PlayerController>();

		//Call SetNodeColours
		playerController.SetNodeColours();

		//playerController.SetNodeColours(Colours[randomIndex]);

		//Remove our colour from the list of available colours
		// availableColours.RemoveAt(randomIndex);

		// //Re-order/trim our list of colours
		// availableColours.TrimExcess();

	}

	//Called by the InputManager when a player gameobject is destroyed
	public void DisconnectPlayer(PlayerInput playerInput)
	{
		//TODO: If a player is killed, call this to elegantly disconnect them
		//Debug.Log("Player has left:  " + playerInput.gameObject);

		//TODO: Return the players colour back to the pool
		//MeshRenderer nodeMesh = playerInput.gameObject.GetComponentInChildren<MeshRenderer>();

		// //TODO: This sucks, implement it better
		// if (nodeMesh.material.color.r > 0 && nodeMesh.material.color.g > 0 && nodeMesh.material.color.b > 0)
		// {
		// 	//Node was purple, return white to the poool
		// 	availableColours.Add(new Color(0.75f, 0.2f, 0.75f, 1.0f)); //PURPLE
		// }

		// else if (nodeMesh.material.color.r > 0)
		// {
		// 	//Node was red, return red to the pool
		// 	availableColours.Add(new Color(0.5f, 0f, 0f, 1.0f)); //RED
		// }

		// else if (nodeMesh.material.color.g > 0)
		// {
		// 	//Node was green, return red to the pool
		// 	availableColours.Add(new Color(0f, 0.5f, 0f, 1.0f)); //GREEN
		// }

		// else if (nodeMesh.material.color.b > 0)
		// {
		// 	//Node was blue, return red to the pool
		// 	availableColours.Add(new Color(0f, 0f, 0.5f, 1.0f)); //BLUE
		// }

		//		availableColours.TrimExcess();


	}

	//Checks all spawn points and spawns player if there is no one else in range
	Vector3 GetClearSpawn()
	{

		Vector3 returnPosition = Vector3.zero;

		foreach (Transform child in spawnPointsGameObject.transform)
		{
			SpawnPointController spawnPointController = child.gameObject.GetComponent<SpawnPointController>();
			if (spawnPointController.CanISpawn() == true)
			{
				returnPosition = child.position;
			}
		}

		return returnPosition;

	}

	//Called when all nodes are destroyed
	public void PlayerDestroyed(GameObject player, Color colour)
	{
		//Returns the player colour back to the available colours list
		availableColours.Add(colour);

		//Destroys the player gameobject (thus calling DisconnectPlayer())
		Destroy(player);
	}

	//Provides the requesting player with a colour from the available colours
	public Color ProvideColour()
	{
		//Pick a random colour from the list of colours
		int randomIndex = Random.Range(0, availableColours.Count);

		//Set our return var to the random colour
		Color returnColour = availableColours[randomIndex];

		//Remove our colour from the list of available colours
		availableColours.RemoveAt(randomIndex);
		availableColours.TrimExcess();

		//Send the colour
		return returnColour;
	}

}
