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
		mCurrentCamera = GameObject.Find("Camera").GetComponent<Camera>();
		mPurchaseItem = new PurchaseItem(ItemId);

	}
	
	// Update is called once per frame
	void Update () {

		RaycastHit hit;

		if (StoreController.Instance.CurrentState == StoreController.State.None) {

			bool isMouseHit =  gameObject.GetComponent<Collider>().Raycast(mCurrentCamera.ScreenPointToRay(Input.mousePosition), out hit, 10f) && Input.GetMouseButtonUp(0);
			bool isTouchHit =  Input.touchCount > 0 && 
							   gameObject.GetComponent<Collider>().Raycast(mCurrentCamera.ScreenPointToRay(Input.GetTouch(0).position), out hit, 10f) && 
							   Input.GetTouch(0).phase == TouchPhase.Ended;

			if (isMouseHit || isTouchHit) {
				onBuyClicked();
			}
		}
	}


	void onBuyClicked() {
		StoreController.Instance.StartPurchase(ItemName, mPurchaseItem, Price, ShouldFail);
	}

	void OnGUI() {
		if (StoreController.Instance.CurrentState != StoreController.State.None) {
			return;
		}

		// Skin
		GUI.skin.button.fontSize = 20;
		GUI.skin.textField.fontSize = 20;

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
			onBuyClicked();
		}

		// Name
		buyButtonRect.y += 50;
		GUI.TextField(buyButtonRect, ItemName);

		// Price
		buyButtonRect.y += 50;
		GUI.TextField(buyButtonRect, "$" + Price);


	}

}
