using UnityEngine;
using System.Collections;


public class StoreItemController : MonoBehaviour {

	public string ItemId;
	public float Price;
	public string ItemName;
	Camera currentCamera;

 


	// Use this for initialization
	void Start () {
		currentCamera = GameObject.Find("Camera").camera;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI() {

		var screenPoint = currentCamera.WorldToScreenPoint(gameObject.transform.position);
		var gameObjectSize = currentCamera.WorldToScreenPoint(gameObject.transform.localScale);
		Rect buyButtonRect = new Rect(screenPoint.x - 50, screenPoint.y + 10, 100, 50);
		GUI.Button(buyButtonRect, "BUY");

		// Name
		GUI.skin.button.fontSize = 20;
		GUI.skin.textArea.fontSize = 20;
		Rect labelRect = buyButtonRect;
		buyButtonRect.y += 50;
		GUI.TextArea(buyButtonRect, ItemName);

		// Price


	}
}
