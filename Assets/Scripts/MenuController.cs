using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{

	public void RestartGame()
	{
		Debug.Log("Restart Button clicked");
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}
