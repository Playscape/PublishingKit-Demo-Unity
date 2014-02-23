using UnityEngine;
using System.Collections;
using Chartboost;

public class ChartboostInitiazlier : MonoBehaviour {

	// Use this for initialization
	void Start () {

		#if UNITY_ANDROID
		CBBinding.init();
		#elif UNITY_IPHONE
		CBBinding.init("5309a9902d42da0e6ca7ec6e", "c8a570250e385f380527b85360acd3adb22f6568");
		#endif
	}
}
