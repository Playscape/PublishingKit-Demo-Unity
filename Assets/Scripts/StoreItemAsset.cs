using UnityEngine;
using System.Collections;
using Soomla.Store;

namespace Soomla.Store {

	public class StoreItemAsset : IStoreAssets {

		private static StoreItemAsset mInstance;

		public static StoreItemAsset Instance {
			get {
				if (mInstance == null) {
					mInstance = new StoreItemAsset();
				}

				return mInstance;
			}
		}

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
			return new VirtualCurrencyPack[] {DARK_MATTER, DIAMOND, FIRE_BALL};
		}

		public VirtualCategory[] GetCategories() {
			return new VirtualCategory[] {};
		}

		public static string getItemIdByName(string name) {
			VirtualCurrencyPack[] paks = StoreItemAsset.Instance.GetCurrencyPacks ();
			foreach (VirtualCurrencyPack pack in paks) {
				if(pack.Name.Equals(name)) {
					return pack.ItemId;
				}
			}
			return "no_found_item_by_name_in_soomla_asset";
		}

		public const string BALLS_CURRENCY_ITEM_ID      = "currency_balls";

		public const string DARK_MATTER_PRODUCT_ID      = "dark_matter";
		public const string DIAMOND_PRODUCT_ID      = "diamond_item";
		public const string FIRE_BALL_PRODUCT_ID      = "buy_fail";


		public static VirtualCurrency BALLS_CURRENCY = new VirtualCurrency(
			"Balls",										// name
			"",												// description
			BALLS_CURRENCY_ITEM_ID							// item id
			);

		public static VirtualCurrencyPack DARK_MATTER = new VirtualCurrencyPack(
			"Dark Matter",                                   // name
			"Test refund of an item",                       // description
			"dark_matter_item",                                   // item id
			1,												// number of currencies in the pack
			BALLS_CURRENCY_ITEM_ID,                        // the currency associated with this pack
			new PurchaseWithMarket(DARK_MATTER_PRODUCT_ID, 9.99)
			);

		public static VirtualCurrencyPack DIAMOND = new VirtualCurrencyPack(
			"Diamond",                                   // name
			"Test refund of an item",                       // description
			"diamond_item",                                   // item id
			1,												// number of currencies in the pack
			BALLS_CURRENCY_ITEM_ID,                        // the currency associated with this pack
			new PurchaseWithMarket(DIAMOND_PRODUCT_ID, 49.99)
			);

		public static VirtualCurrencyPack FIRE_BALL = new VirtualCurrencyPack(
			"Fire Ball",                                   // name
			"Test refund of an item",                       // description
			"buy_fail",                                   // item id
			1,												// number of currencies in the pack
			BALLS_CURRENCY_ITEM_ID,                        // the currency associated with this pack
			new PurchaseWithMarket(FIRE_BALL_PRODUCT_ID, 119.99)
			);
	}
}
