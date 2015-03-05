using UnityEngine;
using System.Collections;

namespace Soomla.Store {

	public class StoreItemAsset : IStoreAssets {

		// Use this for initialization
		/// <summary>
		/// see parent.
		/// </summary>
		public int GetVersion() {
			return 0;
		}
		
		/// <summary>
		/// see parent.
		/// </summary>
		public VirtualCurrency[] GetCurrencies() {
			return new VirtualCurrency[]{BALLS_CURRENCY};
		}
		
		/// <summary>
		/// see parent.
		/// </summary>
		public VirtualGood[] GetGoods() {
			return new VirtualGood[] {};
		}
		
		/// <summary>
		/// see parent.
		/// </summary>
		public VirtualCurrencyPack[] GetCurrencyPacks() {
			return new VirtualCurrencyPack[] {DARK_MATTER};
		}

		public VirtualCategory[] GetCategories() {
			return new VirtualCategory[] {};
		}

		public const string BALLS_CURRENCY_ITEM_ID      = "currency_balls";

//		public const string DARK_MATTER_PRODUCT_ID      = "dark_matter";
		public const string DARK_MATTER_PRODUCT_ID      = "android.test.purchased";


		public static VirtualCurrency BALLS_CURRENCY = new VirtualCurrency(
			"Balls",										// name
			"",												// description
			BALLS_CURRENCY_ITEM_ID							// item id
			);

		public static VirtualCurrencyPack DARK_MATTER = new VirtualCurrencyPack(
			"Dark Matter",                                   // name
			"Test refund of an item",                       // description
			"dark_matter",                                   // item id
			1,												// number of currencies in the pack
			BALLS_CURRENCY_ITEM_ID,                        // the currency associated with this pack
			new PurchaseWithMarket(DARK_MATTER_PRODUCT_ID, 0.99)
			);
	}
}
