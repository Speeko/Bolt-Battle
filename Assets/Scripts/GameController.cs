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

	public void GetVars()
	{
		//Other scripts should call this one to get global game options
		//TODO...
	}

	//Called by the InputManager when "start" is pressed on any controller
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

	}

	//Checks all spawn points and returns a spawn location if there are no other players in range
	Vector3 GetClearSpawn()
	{

		//TODO: Make this better

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

	//Called when all of a player's nodes are destroyed
	public void PlayerDestroyed(GameObject player, Color colour)
	{
		//Returns the player colour back to the available colours list
		availableColours.Add(colour);

		//Destroys the player gameobject (thus calling DisconnectPlayer())
		Destroy(player);
	}

	//Provides the requesting player with a colour from the available colours (colours are managed in the editor)
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

	void SpawnContainer()
	{
		//On a random timer...
		//Pick a random spawn point... (use GetClearSpawn())
		//Spawn a container if there are no objects within x distance
		//Repeat
	}

}
