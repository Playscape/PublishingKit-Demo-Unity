using UnityEngine;

namespace Playscape.Internal {
	public class PushWooshStub : PushWooshCommon {

		public void StubInit() {
			SendMessage("onRegisteredForPushNotifications", "PushWooshStubToken");
			SendMessage("onPushNotificationsReceived", "hello");
		}
		#region implemented abstract members of PushWooshCommon
		protected override void SetTagImpl (string name, string value)
		{
			L.D (string.Format("PushWoosh stub setting: {0} to {1}", name, value));
		}
		protected override void SetTagImpl (string name, int value)
		{
			L.D (string.Format("PushWoosh stub setting: {0} to {1}", name, value));
		}
		#endregion
	}
}