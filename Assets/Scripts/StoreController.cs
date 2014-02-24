﻿using UnityEngine;
using System.Collections;
using Playscape.Purchase;
using Playscape.Analytics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Playscape;

public class StoreController : MonoBehaviour {

	#region Current Purchased Item

	PurchaseItem mCurrentItemPurchasing;
	bool mShouldPurchaseFail;
	float mCurrentItemPrice;
	string mCurrentItemName;

	#endregion

	const string STORE_FLOW_TYPE = "Store";
	const string OPEN_STORE_FLOW_STEP = "OpenStore";
	const string SELECT_CATEGORY_FLOW_STEP = "SelectCategory";
	const string SELECT_ITEM_FLOW_STEP = "SelectItem";
	const string PURCHASED_FLOW_STEP = "Purchased";
	const string CANCELED_FLOW_STEP = "Canceled";

	
	const double PREMIUM_CURRENCY = 1000;
	const double TRASH_CURRENCY = 200;
	const double CATEGORY = 1;
	const double ITEM_ID = 2;

	public enum State {
		None,
		PurchaseStarted,
		Failed
	}

	public State CurrentState { private set; get; }

	static StoreController mInstance;

	IDictionary<string, double> mDummyFlowDetails = MakeFlowDetails(PREMIUM_CURRENCY, TRASH_CURRENCY, CATEGORY, ITEM_ID);

	FlowInstance mStoreFlow;

	public static StoreController Instance {
		get {
			if (mInstance == null) {
				var go = GameObject.Find("StoreController");

				if (go != null) {
					mInstance = go.GetComponent<StoreController>();
				}
			}

			return mInstance;
		}
	}


	// Use this for initialization
	void Start () {
		CurrentState = State.None;

		RegisterStoreFlow();

		Report.Instance.ReportFlowStep(mStoreFlow, OPEN_STORE_FLOW_STEP, "ok", mDummyFlowDetails);
	}

	void RegisterStoreFlow ()
	{
		var stepNameToId = new Dictionary<string, int>();
		stepNameToId [OPEN_STORE_FLOW_STEP] = 1;
		stepNameToId [SELECT_CATEGORY_FLOW_STEP] = 2;
		stepNameToId [SELECT_ITEM_FLOW_STEP] = 3;
		stepNameToId [PURCHASED_FLOW_STEP] = 4;
		stepNameToId [CANCELED_FLOW_STEP] = 5;
		Report.Instance.RegisterFlow(STORE_FLOW_TYPE, stepNameToId);
		mStoreFlow = Report.Instance.StartNewFlow(STORE_FLOW_TYPE);

	}

	static IDictionary<string, double> MakeFlowDetails(double premiumCurrency, double trashCurrency, double category, double itemId) {
		var details = new Dictionary<string, double>();

		details["PremiumCurrency"] = premiumCurrency;
		details["TrashCurrency"] = trashCurrency;
		details["Category"] = category;
		details["ItemID"] = itemId;

		return details;
	}


	void OnDestroy() {
		mInstance = null;
	}

	public void StartPurchase (string name, PurchaseItem purchaseItem, float price, bool shouldFail)
	{
		Report.Instance.ReportFlowStep(mStoreFlow, SELECT_CATEGORY_FLOW_STEP, "ok", mDummyFlowDetails);
		Report.Instance.ReportFlowStep(mStoreFlow, SELECT_ITEM_FLOW_STEP, "ok", mDummyFlowDetails);


		CurrentState = State.PurchaseStarted;
		mCurrentItemPurchasing = purchaseItem;
		mCurrentItemName = name;
		mShouldPurchaseFail = shouldFail;
		mCurrentItemPrice = price;


	}


	void OnGUI() {
		if (CurrentState != State.None) {
			float boxWidth = Screen.width * 0.80f;
			GUI.Box(new Rect(Screen.width/2 - boxWidth/2, 15, boxWidth, Screen.height * 0.8f), "Buy " + mCurrentItemName);
			GUI.skin.textArea.fontSize = 20;
			GUI.skin.button.fontSize = 20;

			float buttonWidth = Screen.width * 0.4f;
			float buttonHeight = Screen.height * 0.2f;


			if (CurrentState == State.PurchaseStarted) {
				// Confirm
				bool confirm = GUI.Button(new Rect(Screen.width/2 - buttonWidth/2, Screen.height/4, buttonWidth, buttonHeight), "Confirm"); 
				if (confirm) {
					if (mShouldPurchaseFail) {
						Report.Instance.ReportPurchaseStarted(mCurrentItemPurchasing);
						Report.Instance.ReportPurchaseFailed(mCurrentItemPurchasing, "User Canceled");
						CurrentState = State.Failed;
					} else {
						Report.Instance.ReportFlowStep(mStoreFlow, PURCHASED_FLOW_STEP, "ok", mDummyFlowDetails);

						Report.Instance.ReportPurchaseStarted(mCurrentItemPurchasing);
						Report.Instance.ReportPurchaseSuccess(mCurrentItemPurchasing, mCurrentItemPrice, "USD", Utils.CurrentTimeMillis, "fake-tranaction-id");


						CurrentState = State.None;
					}

				}
				
	            // Cancel
				bool cancel = GUI.Button(new Rect(Screen.width/2 - buttonWidth/2,10+ Screen.height/4 + buttonHeight, buttonWidth, buttonHeight), "Cancel"); 
				if (cancel) {
					Report.Instance.ReportFlowStep(mStoreFlow, CANCELED_FLOW_STEP, "ok", mDummyFlowDetails);

					Report.Instance.ReportPurchaseStarted(mCurrentItemPurchasing);
					Report.Instance.ReportPurchaseCancelled(mCurrentItemPurchasing);

					CurrentState = State.None;
				}
			} else if (CurrentState == State.Failed) {
				// Failed
				GUI.TextField(new Rect(Screen.width/2 - buttonWidth/2, Screen.height/4, buttonWidth, buttonHeight), "Purchase Failed :-/"); 

				// OK
				bool ok = GUI.Button(new Rect(Screen.width/2 - buttonWidth/2,10+ Screen.height/4 + buttonHeight, buttonWidth, buttonHeight), "OK"); 
				if (ok) {
					CurrentState = State.None;
				}
			}

		}
	}
}
