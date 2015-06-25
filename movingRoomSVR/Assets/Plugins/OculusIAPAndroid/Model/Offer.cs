using System;
using System.Collections;
using System.Collections.Generic;
using OculusIAPAndroid.MiniJSON;

namespace OculusIAPAndroid.Model
{
    /// <summary>
    /// This class represents an In-App-Purchase offer.
    /// </summary>
    public class Offer
    {
        /// <summary>
        /// Gets the offer identifier.
        /// </summary>
        /// <value>The offer identifier.</value>
        public String OfferId { get; private set; }

        /// <summary>
        /// Gets the offer sku.
        /// </summary>
        /// <value>The offer sku.</value>
        public String Sku { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this offer is consumable.
        /// </summary>
        /// <value><c>true</c> if this offer is consumable; otherwise, <c>false</c>.</value>
        public Boolean IsConsumable { get; private set; }

        /// <summary>
        /// Gets the offer type.
        /// </summary>
        /// <value>The offer type.</value>
        public String Type { get; private set; }

        /// <summary>
        /// Gets the offer price.
        /// </summary>
        /// <value>The offer price.</value>
        public String Price { get; private set; }

        /// <summary>
        /// Gets the offer title.
        /// </summary>
        /// <value>The offer title.</value>
        public String Title { get; private set; }

        /// <summary>
        /// Gets the offer description.
        /// </summary>
        /// <value>The offer description.</value>
        public String Description { get; private set; }

        Offer()
        {
        }

        public override string ToString()
        {
            return toHashtable().toJson();
        }
        
        public Hashtable toHashtable()
        {
            Hashtable hashtable = new Hashtable();
            
            hashtable.Add("offerId", OfferId);
            hashtable.Add("sku", Sku);
            hashtable.Add("isConsumable", IsConsumable);
            hashtable.Add("type", Type);
            hashtable.Add("price", Price);
            hashtable.Add("title", Title);
            hashtable.Add("description", Description);

            return hashtable;
        }

        public static Offer fromHashtable(Hashtable hashtable)
        {
            var offer = new Offer();
            
            offer.OfferId = (String)hashtable ["offerId"];
            
            offer.Sku = (String)hashtable ["sku"];

            offer.IsConsumable = (Boolean)hashtable ["isConsumable"];

            offer.Type = (String)hashtable ["type"];

            offer.Price = (String)hashtable ["price"];
            
            offer.Title = (String)hashtable ["title"];
            
            offer.Description = (String)hashtable ["description"];
            
            return offer;
        }

        public static IList<Offer> fromArrayList(ArrayList arrayList)
        {
            var offers = new List<Offer>();
            
            foreach (Hashtable hashtable in arrayList)
            {
                var offer = fromHashtable(hashtable);
                offers.Add(offer);
            }
            
            return offers.AsReadOnly();
        }
    }
}
