using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerController : MonoBehaviour
{

	public GameObject pickupPrefab;

	public void OpenContainer()
	{
		//TODO: Spawn a pickup
		Instantiate(pickupPrefab, transform.position, Quaternion.identity);
		Destroy(gameObject);
	}

}
