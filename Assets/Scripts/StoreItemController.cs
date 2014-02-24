using UnityEngine;
using System.Collections;
using Playscape.Analytics;
using Playscape.Purchase;

public class StoreItemController : MonoBehaviour {

	public string ItemId;
	public float Price;
	public string ItemName;
	public bool ShouldFail = false;

	Camera mCurrentCamera;

	PurchaseItem mPurchaseItem;

	// Use this for initialization
	void Start () {
		mCurrentCamera = GameObject.Find("Camera").camera;
		mPurchaseItem = new PurchaseItem(ItemId);

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI() {
		if (StoreController.Instance.CurrentState != StoreController.State.None) {
			return;
		}

		// Skin
		GUI.skin.button.fontSize = 20;
		GUI.skin.textArea.fontSize = 20;

		// Buy Button
		var screenPoint = mCurrentCamera.WorldToScreenPoint(gameObject.transform.position);

		Rect buyButtonRect = new Rect(screenPoint.x - 65, screenPoint.y + 10, 130, 50);
		bool buyClicked = false;
		if (!ShouldFail) {
			buyClicked = GUI.Button(buyButtonRect, "Buy");
		} else {
			buyClicked = GUI.Button(buyButtonRect, "Buy & Fail");
		}

		if (buyClicked) {
			StoreController.Instance.StartPurchase(ItemName, mPurchaseItem, Price, ShouldFail);
		}

		// Name
		buyButtonRect.y += 50;
		GUI.TextArea(buyButtonRect, ItemName);

		// Price
		buyButtonRect.y += 50;
		GUI.TextArea(buyButtonRect, "$" + Price);


	}

}
