using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeBugger : MonoBehaviour
{

	[SerializeField] bool debugMode;

	public bool IsEnabled()
	{
		if (debugMode == true)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

}
