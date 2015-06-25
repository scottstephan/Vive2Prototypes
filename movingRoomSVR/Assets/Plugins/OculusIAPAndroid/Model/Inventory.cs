using System.Collections.Generic;
using System.Collections;
using OculusIAPAndroid.MiniJSON;

namespace OculusIAPAndroid.Model
{
    /// <summary>
    /// This is class represents the offer details and the entitlements a user has.
    /// </summary>
    public class Inventory
    {
        /// <summary>
        /// Gets the offer details.
        /// </summary>
        /// <value>The offer details.</value>
        public IList<Offer> Offers { get; private set; }

        /// <summary>
        /// Gets the entitlements a user has.
        /// </summary>
        /// <value>The entitlements a user has.</value>
        public IList<Entitlement> Entitlements { get; private set; }

        Inventory()
        {
        }

        public override string ToString()
        {
            return toHashtable().toJson();
        }
        
        public Hashtable toHashtable()
        {
            Hashtable hashtable = new Hashtable();

            ArrayList entitlementsArray = new ArrayList();
            foreach (Entitlement entitlement in Entitlements)
            {
                entitlementsArray.Add(entitlement.toHashtable());
            }

            hashtable.Add("entitlements", entitlementsArray);

            ArrayList offersArray = new ArrayList();
            foreach (Offer offer in Offers)
            {
                offersArray.Add(offer.toHashtable());
            }

            hashtable.Add("offers", offersArray);
            
            return hashtable;
        }
        
        public static Inventory fromHashtable(Hashtable hashtable)
        {
            var inventory = new Inventory();

            var entitlements = (ArrayList)hashtable ["entitlements"];
            inventory.Entitlements = Entitlement.fromArrayList(entitlements);

            var offers = (ArrayList)hashtable ["offers"];
            inventory.Offers = Offer.fromArrayList(offers);

            return inventory;
        }
    }
}