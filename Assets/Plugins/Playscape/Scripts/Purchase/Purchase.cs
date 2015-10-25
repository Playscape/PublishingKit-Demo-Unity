using System;
using System.Collections.Generic;

namespace Playscape.Purchase
{
    /// <summary>
    /// A purchasable item
    /// </summary>
    public class PurchaseItem
    {
        /// <summary>
        /// Item's name (serves as the default identifier if specific identifier
        /// is missing)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Whether this item is consumable or not
        /// </summary>
        public bool Consumable { get; set; }

        /// <summary>
        /// The item's idetifier in Google Play.
        /// 
        /// <code>Id</code> will be used is this is <code>null</code>.
        /// </summary>
        /// </value>
        public string GooglePlayId { get; set; }

        /// <summary>
        /// The item's idetifier in Apple's App Store.
        /// 
        /// <code>Id</code> will be used is this is <code>null</code>.
        /// </summary>
        /// </value>
        public string AppleAppStoreId { get; set; }


        /// <summary>
        /// The item's idetifier in Amazon's App Store.
        /// 
        /// <code>Id</code> will be used is this is <code>null</code>.
        /// </summary>
        /// </value>
        public string AmazonAppStoreId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="Playscape.Purchase.PurchaseItem"/> class.
        /// </summary>
        /// <param name='name'>
        /// Item's name. (and default identifier)
        /// </param>
        public PurchaseItem(string name)
        {
            Name = name;
        }
    }

}
