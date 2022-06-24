using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

	GameController gameController;

	//This represents our current speed. See IncreaseNodeSpeed() and DecreaseNodeSpeed()
	float playerCurrentSpeed;

	//Vectors representing input and movement
	Vector2 currentMovementInput = Vector2.zero;
	Vector3 lastMovementDirection = Vector3.forward;
	Vector3 currentMovementDirection = Vector3.zero;

	//The character controller for this node
	CharacterController characterController;

	//The player controller from our parent container
	PlayerController playerController;

	//Tells the script/node if we're trying a beam or not
	bool tryingBeam = false;

	bool isMoving = false;

	bool nodeInitialised = false;

	bool isDead = false;

	//Our individual colour (dark/light shade)
	Color nodeColour;
	//Our base colour (red/blue/green/etc)
	Color nodeBaseColour;

	//This gameobject represents the twin node the player is controlling
	GameObject twinNode;
	NodeController twinNodeController;

	//This is the BeamController script on the parent container
	BeamController beamController;

	//This is our particle system (particle emitter)
	public ParticleSystem particleSys;

	float afkTimer = 0;
	public float afkKickTime;


	#endregion

	void Awake()
	{
		if (nodeInitialised == false)
			InitaliseNode();
	}

	//TODO: Rename to InitaliseNode() - call from PlayerController or GameController
	void InitaliseNode()
	{

		if (nodeInitialised == false)
		{

			//Get components
			characterController = GetComponent<CharacterController>(); //Character controller for accepting movement input
			playerController = transform.parent.gameObject.GetComponent<PlayerController>(); //PLayer controller script on our parent container
			beamController = transform.parent.gameObject.GetComponent<BeamController>(); //Beam controller script on our parent container
			gameController = GameObject.Find("GameController").GetComponent<GameController>();

			//Get our twin node for use later
			FindTwinNode();

			//Set our max move speed
			playerCurrentSpeed = playerController.GetPlayerNormalSpeed();

			//Tell our script that we're ready to use
			nodeInitialised = true;
		}

	}

	// void ApplyColours()
	// {

	// 	Color myColour = gameController.ProvideColour(gameObject);

	// 	//Set our colour
	// 	GetComponent<MeshRenderer>().material.color = myColour;

	// 	//Set the beam colour
	// 	beamController.SetBeamColour(myColour);

	// 	//Get our particle system
	// 	particleSys = gameObject.GetComponentInChildren<ParticleSystem>();
	// 	particleSys.Stop();

	// 	//Set the particle  system colour
	// 	var main = particleSys.main;
	// 	main.startColor = myColour;

	// 	//Get our trail renderer and set the colour
	// 	GetComponent<TrailRenderer>().material.color = new Color(nodeColour.r, nodeColour.g, nodeColour.b, 0.5f);

	// 	//Set our energyindicator
	// 	transform.Find("energyindicator").gameObject.GetComponent<SpriteRenderer>().color = new Color(myColour.r + 0.25f, myColour.g + 0.25f, myColour.b + 0.25f, 0.5f);

	// }

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

		twinNodeController = twinNode.gameObject.GetComponent<NodeController>();

	}

	public void OnMoveInput(InputAction.CallbackContext context)
	{

		if (isDead == false)
		{

			if (context.started)
			{

				if (tryingBeam == false)
				{
					moveAudioSource.Play();
				}

			}

			//If cancelled (input stopped) then store the last movement direction and zero our current movement
			if (context.canceled)
			{
				moveAudioSource.Stop();

				if (currentMovementDirection != Vector3.zero)
				{
					lastMovementDirection = currentMovementDirection;
				}

				currentMovementDirection = Vector3.zero;
				currentMovementInput = Vector3.zero;

				isMoving = false;

			}

			//If input performed, get our movement direction for later
			if (context.performed)
			{
				currentMovementInput = context.ReadValue<Vector2>();
				lastMovementDirection = new Vector3(currentMovementInput.x, 0.0f, currentMovementInput.y);
				isMoving = true;
			}
		}
	}

	public void OnBeamInput(InputAction.CallbackContext context)
	{

		if (isDead == false)
		{

			//We're starting to attempt a beam
			if (context.started)
			{

				if (twinNode != null)
				{

					//Tell the rest of our script we're trying a beam
					tryingBeam = true;

					//Deal with audio
					moveAudioSource.Stop();
					beamAudioSource.Play();

					beamController.TryBeam(gameObject);

				}

			}

			if (context.canceled)
			{

				beamAudioSource.Stop();

				if (tryingBeam == true)
				{
					beamController.StopTryBeam(gameObject, false);
					tryingBeam = false;
					particleSys.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
				}
			}
		}
	}

	public void IncreaseNodeSpeed(float speed)
	{
		//TODO: Using hardcoded up/down values for now, I feel like this could be better implemented
		//Debug.Log("Increasing speed of node: " + gameObject);
		playerCurrentSpeed = playerController.GetPlayerNormalSpeed();
	}

	public void DecreaseNodeSpeed(float speed)
	{
		//Decrement speed
		//Debug.Log("Decreasing speed of node: " + gameObject);
		playerCurrentSpeed = playerController.GetPlayerSlowedSpeed();
	}

	public void SetNodeColour(Color colour, Color particlesColour)
	{

		if (nodeInitialised == false)
			InitaliseNode();

		//Set our colour
		GetComponent<MeshRenderer>().material.color = colour;
		nodeColour = colour;

		//Get our particle system
		particleSys = gameObject.GetComponentInChildren<ParticleSystem>();
		particleSys.Stop();

		//Set the particle  system colour
		var main = particleSys.main;
		main.startColor = particlesColour;

		//Get our trail renderer and set the colour
		GetComponent<TrailRenderer>().material.color = new Color(colour.r, colour.g, colour.b, 0.5f);

		//Set our energyindicator
		transform.Find("energyindicator").gameObject.GetComponent<SpriteRenderer>().color = new Color(colour.r + 0.25f, colour.g + 0.25f, colour.b + 0.25f, 0.5f);

		// //Set some vars (actual colour is set in the Awake() function)
		// nodeColour = colour;
		// nodeBaseColour = particlesColour;
	}

	public void DestroyNode()
	{
		isDead = true;

		tryingBeam = false;

		//We are destroyed, so we need to stop trying for a beam
		beamController.StopTryBeam(gameObject, true);
		//beamController.StopTryBeam(twinNode, false);

		//Insantiate an explosion effect
		GameObject myExplosion = Instantiate(explodingNodePrefab, transform.position, Quaternion.identity);
		ExplosionController myExplosionController = myExplosion.GetComponent<ExplosionController>();
		myExplosionController.BeginExplosion(nodeColour);

		//TODO: Move this logic up to the  parent, handle node respawning.
		//Destroy(gameObject);
		characterController.enabled = false;

		//If our twin node is missing, then this player is dead. Destory the parent container
		if (twinNode.activeSelf == false)
		{
			playerController.NodesDestroyed();
			Destroy(transform.parent.gameObject);
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

	public void Respawn()
	{
		characterController.enabled = true;
		isDead = false;
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

		if (afkTimer > afkKickTime)
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



}
