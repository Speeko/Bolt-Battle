using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//This script should be attached to each player node.
//It is primarily responsible for handling inputs such as movement & beaming. As well as handling colour setting, destruction, revival, etc.
public class NodeController : MonoBehaviour
{

	#region vars

	//Tell this script if this node is player or bot controlled
	public bool isBot;

	// Attach an AudioSource component to this gameObject
	public AudioSource moveAudioSource;
	public AudioSource beamAudioSource;

	//Prefabs
	public GameObject explodingNodePrefab;
	public GameObject reviveIconPrefab;

	//These vars handle destroying the player node if they are AFK
	public bool enableAFKKick; //Determines if AFK-destroy is enabled
	float afkTimer = 0; //The AFK counter (starts at zero)
	public float afkKickTime; //The time to count to before destroying the node (set in the editor)

	//This represents our current speed. See IncreaseNodeSpeed() and DecreaseNodeSpeed()
	float playerCurrentSpeed;

	//Vectors representing input and movement
	Vector2 currentMovementInput = Vector2.zero;
	Vector3 lastMovementDirection = Vector3.forward;
	Vector3 currentMovementDirection = Vector3.zero;

	//The character controller for this node
	CharacterController characterController;

	//The parent player controller
	PlayerController playerController;

	//The game controller in the scene
	GameController gameController;

	//This is the BeamController script on the parent container
	BeamController beamController;

	//This gameobject represents the twin node the player is controlling
	GameObject twinNode;
	//This is the twin node's controller
	NodeController twinNodeController;

	//Tells the script/node if we're trying a beam or not
	bool tryingBeam = false;

	//Tells the script/node if we're moving or not
	bool isMoving = false;

	//Tells the script/node if we've already been set up (ie: InitaliseNode() has been run)
	bool nodeInitialised = false;

	//Tells the script if we're dead (waiting to revive)
	bool isDead = false;

	//Our colours:
	Color nodeColour; //This is our individual colour (ie: some shade of red/green/blue/etc)
	Color nodeBaseColour; //This is our "base" colour (ie: red/blue/green etc)

	//This is our particle system (particle emitter)
	public ParticleSystem particleSys;

	#endregion

	void Awake()
	{
		//If we haven't already been inialised then call InitaliseNode()
		if (nodeInitialised == false)
			InitaliseNode();
	}

	//TODO: Call this from GameController or PlayerController - rather than relying on Awake()
	void InitaliseNode()
	{

		if (nodeInitialised == false)
		{

			//Get some components
			characterController = GetComponent<CharacterController>(); //Character controller for accepting movement input
			playerController = transform.parent.gameObject.GetComponent<PlayerController>(); //Player controller script on our parent container
			beamController = transform.parent.gameObject.GetComponent<BeamController>(); //Beam controller script on our parent container
			gameController = GameObject.Find("GameController").GetComponent<GameController>(); //The GameController in our scene

			//Get our twin node for use later
			FindTwinNode();

			//Set our max move speed
			playerCurrentSpeed = playerController.GetPlayerNormalSpeed();

			//Tell our script that we've been initalised
			nodeInitialised = true;
		}

	}

	void FindTwinNode()
	{

		//Get our siblings from our parent by starting a new list of objects...
		List<GameObject> childObjects = new List<GameObject>();

		//Loop through that list and if there's a node, add it to our list
		foreach (Transform child in transform.parent)
		{
			if (child.gameObject.tag == "Node")
			{
				childObjects.Add(child.gameObject);
			}
		}

		//Remove ourselves from the list
		childObjects.Remove(gameObject);

		//Trim the list
		childObjects.TrimExcess();

		//The remaining object in the list is our sibling (hopefully)
		twinNode = childObjects[0];

		//Get our twin node's controller
		twinNodeController = twinNode.gameObject.GetComponent<NodeController>();

	}

	//This is called through the InputManager component on the prefab in the editor
	public void OnMoveInput(InputAction.CallbackContext context)
	{

		//Only allow movement if we're not dead
		if (isDead == false)
		{

			//If movement started...
			if (context.started)
			{
				//If we're not trying for a beam then play the movement audio
				if (tryingBeam == false)
				{
					moveAudioSource.Play();
				}

			}

			//If movement input held...
			if (context.performed)
			{
				//Get the movement value from the input and store it to currentMovementInput
				currentMovementInput = context.ReadValue<Vector2>();

				//Set lastMovementDirection to our current input
				lastMovementDirection = new Vector3(currentMovementInput.x, 0.0f, currentMovementInput.y);

				//Tell the script we're moving
				isMoving = true;
			}

			//If cancelled (input stopped) then store the last movement direction and zero our current movement
			if (context.canceled)
			{

				//Stop playing our movement audio
				moveAudioSource.Stop();

				//Get our last movement input and store it into lastMovementDirection
				if (currentMovementDirection != Vector3.zero)
				{
					lastMovementDirection = currentMovementDirection;
				}

				//Input has stopped being pressed, zero our movement vectors
				currentMovementDirection = Vector3.zero;
				currentMovementInput = Vector3.zero;

				//Tell our script we're not moving anymore
				isMoving = false;
			}



		}
	}

	//This is called through the InputManager component on the prefab in the editor
	public void OnBeamInput(InputAction.CallbackContext context)
	{

		//Only allow beam creation if we're not dead
		if (isDead == false)
		{

			//We're starting to attempt a beam
			if (context.started)
			{

				DecreaseNodeSpeed(0);

				//If our twin node exists...
				if (twinNode.activeSelf == true)
				{

					//Tell the rest of our script we're trying a beam
					tryingBeam = true;

					//Stop playing movement audio
					moveAudioSource.Stop();

					//Start playing beam audio
					beamAudioSource.Play();

					//Tell the BeamController on our parent that this node is attempting a beam
					beamController.TryBeam(gameObject);

				}

			}

			//If beam input stopped...
			if (context.canceled)
			{

				IncreaseNodeSpeed(0);

				//Stop the beam audio
				beamAudioSource.Stop();

				//If we were trying a beam...
				if (tryingBeam == true)
				{
					//Tell the BeamController on our parent that we've stopped trying
					beamController.StopTryBeam(gameObject, false);

					//Tell the rest of our script that we've stopped trying.
					tryingBeam = false;

					//Stop displaying the "reaching out" particles
					particleSys.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
				}
			}

		}
	}

	//Called from various places to increment the node speed
	public void IncreaseNodeSpeed(float speed)
	{
		//TODO: Using hardcoded up/down values for now, I feel like this could be better implemented
		//Debug.Log("Increasing speed of node: " + gameObject);
		if (playerCurrentSpeed != playerController.GetPlayerNormalSpeed())
			playerCurrentSpeed = playerController.GetPlayerNormalSpeed();

	}

	//Called from various places to decrement the node speed
	public void DecreaseNodeSpeed(float speed)
	{
		//Decrement speed
		//Debug.Log("Decreasing speed of node: " + gameObject);
		if (playerCurrentSpeed != playerController.GetPlayerSlowedSpeed())
			playerCurrentSpeed = playerController.GetPlayerSlowedSpeed();

	}

	//Called by the PlayerController on our parent to set the colours of this node and its particle system
	public void SetNodeColour(Color colour, Color particlesColour)
	{

		//If we've not already been initalised, then do it
		if (nodeInitialised == false)
			InitaliseNode();

		//Set our colour to the colour provided by our parent PlayerController
		GetComponent<MeshRenderer>().material.color = colour;

		//Also set our colour variable.. (used to generate matching colour explosions)
		nodeColour = colour;

		//Get our particle system
		particleSys = gameObject.GetComponentInChildren<ParticleSystem>();

		//Ensure the particle system is stopped
		particleSys.Stop();

		//Set the particle system colour (should be different to the node colour - should match the colour of our twin)
		var main = particleSys.main;
		main.startColor = particlesColour;

		//Get our trail renderer and set the colour with a slight fade
		GetComponent<TrailRenderer>().material.color = new Color(colour.r, colour.g, colour.b, 0.5f);

		//Set our energyindicator (circular sprite that appears around node when beam established)
		transform.Find("energyindicator").gameObject.GetComponent<SpriteRenderer>().color = new Color(colour.r + 0.25f, colour.g + 0.25f, colour.b + 0.25f, 0.5f);

	}

	//Called when this node has been destroyed by a beam
	public void DestroyNode()
	{
		//Tell our script we're dead
		isDead = true;

		//Tell our script we're not trying a beam
		tryingBeam = false;

		//Tell our script we're not moving
		isMoving = false;

		//We are destroyed, so we need to stop trying for a beam
		beamController.StopTryBeam(gameObject, true);

		//Insantiate an explosion effect
		GameObject myExplosion = Instantiate(explodingNodePrefab, transform.position, Quaternion.identity);
		ExplosionController myExplosionController = myExplosion.GetComponent<ExplosionController>();

		//Set the explosion to match our node colour
		myExplosionController.BeginExplosion(nodeColour);

		//TODO: Move this logic up to the  parent, handle node respawning.
		//Destroy(gameObject);
		characterController.enabled = false;

		//If our twin node is missing, then this player is dead. Destory the parent container
		if (twinNode.activeSelf == false)
		{
			//Tell the PlayerController on our parent that we're destroyed
			playerController.NodesDestroyed();
			//Destroy(transform.parent.gameObject);
		}
		else
		{
			//Draw a revive icon, to let other nodes respawn this node
			GameObject reviveIcon = Instantiate(reviveIconPrefab, transform.position, Quaternion.identity, transform.parent);
			ReviveIconController reviveIconController = reviveIcon.GetComponentInChildren<ReviveIconController>();
			reviveIconController.SetNodeToRevive(gameObject, twinNode);
			reviveIconController.SetColours(nodeColour);
		}

	}

	//If our respawn icon has been touched by our twin node, revives this node
	public void Respawn()
	{
		characterController.enabled = true;
		isDead = false;
		IncreaseNodeSpeed(0);
	}

	void FixedUpdate()
	{

		if (isMoving == true)
		{
			if (moveAudioSource.isPlaying == false && tryingBeam == false)
			{
				moveAudioSource.Play();
			}

			//Move the node
			currentMovementDirection = new Vector3(currentMovementInput.x * playerCurrentSpeed, 0.0f, currentMovementInput.y * playerCurrentSpeed);
			characterController.Move(currentMovementDirection * Time.deltaTime);
		}

		if (tryingBeam == false)
		{
			//Face the node to the direction we're moving
			transform.forward = lastMovementDirection;
		}
		else
		{
			//If we're trying a beam, we should be facing our twin node
			Vector3 direction = Vector3.Normalize(twinNode.transform.position - transform.position);
			//Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);
			transform.forward = direction;
			lastMovementDirection = direction;
		}

		EffectsHandler();

	}

	void Update()
	{

		if (isMoving == true)
		{
			afkTimer = 0;
		}

		afkTimer += Time.deltaTime;

		if (afkTimer > afkKickTime && enableAFKKick == true)
			DestroyNode();
	}

	void EffectsHandler()
	{
		//Turn off particle system if
		// - No longer trying for beam
		// - Beam successfully established
		if (beamController.IsBeamEstablished() == true)
		{
			particleSys.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
			transform.Find("energyindicator").gameObject.SetActive(true);
			GetComponent<TrailRenderer>().enabled = false;
		}
		else
		{
			transform.Find("energyindicator").gameObject.SetActive(false);
			GetComponent<TrailRenderer>().enabled = true;

			if (tryingBeam == true)
			{
				particleSys.Play();
			}
		}

	}

	public void OnShootInput(InputAction.CallbackContext context)
	{
		//Shoot a "boulder" - slow moving projectile that can be reflected by a beam


	}

	void OnTriggerEnter(Collider collider)
	{
		if (collider.gameObject.tag == "Pickup")
		{
			beamController.UpdateBeamDistance(10);
			Destroy(collider.gameObject);
		}
	}

}
