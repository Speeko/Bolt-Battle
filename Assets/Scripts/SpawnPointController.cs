using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointController : MonoBehaviour
{

	bool canSpawnHere = true;

	void OnTriggerStay(Collider collider)
	{
		if (collider.gameObject.tag == "Node")
		{
			canSpawnHere = false;
		}
	}

	void OnTriggerExit(Collider collider)
	{
		if (collider.gameObject.tag == "Node")
		{
			canSpawnHere = true;
		}
	}

	public bool CanISpawn()
	{

		//TODO: Change this to a spherecast - allow it to be used by both player spawning and container spawning.

		if (canSpawnHere == true)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
}
