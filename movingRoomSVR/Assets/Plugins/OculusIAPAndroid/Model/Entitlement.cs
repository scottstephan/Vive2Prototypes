using System;
using System.Collections;
using OculusIAPAndroid.MiniJSON;
using System.Collections.Generic;

namespace OculusIAPAndroid.Model
{
    /// <summary>
    /// This is class represents an entitlement that a user has.
    /// </summary>
    public class Entitlement
    {
        /// <summary>
        /// Gets the entitlement identifier.
        /// </summary>
        /// <value>The entitlement identifier.</value>
        public String EntitlementId { get; private set; }

        /// <summary>
        /// Gets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        public String UserId { get; private set; }

        /// <summary>
        /// Gets the sku.
        /// </summary>
        /// <value>The sku.</value>
        public String Sku { get; private set; }

        /// <summary>
        /// Gets the entitlement type.
        /// </summary>
        /// <value>The entitlement type.</value>
        public String Type { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is consumable.
        /// </summary>
        /// <value><c>true</c> if this instance is consumable; otherwise, <c>false</c>.</value>
        public Boolean IsConsumable { get; private set; }

        /// <summary>
        /// Gets the use count.
        /// </summary>
        /// <value>The use count.</value>
        public Int32 UseCount { get; private set; }

        /// <summary>
        /// Gets the name of the package.
        /// </summary>
        /// <value>The name of the package.</value>
        public String PackageName { get; private set; }

        /// <summary>
        /// Gets the signature timestamp.
        /// </summary>
        /// <value>The signature timestamp.</value>
        public Int64 SignatureTimestamp { get; private set; }

        /// <summary>
        /// Gets the original json of this entitlment data.
        /// </summary>
        /// <value>The original json of this entitlment data.</value>
        public String Payload { get; private set; }

        /// <summary>
        /// Gets the signature of the original json.
        /// </summary>
        /// <value>The signature of the original json.</value>
        public String Signature { get; private set; }

        Entitlement()
        {
        }

        public override string ToString()
        {
            return toHashtable().toJson();
        }
        
        public Hashtable toHashtable()
        {
            Hashtable hashtable = new Hashtable();
            
            hashtable.Add("payload", Payload);
            hashtable.Add("signature", Signature);
            
            return hashtable;
        }

        public static Entitlement fromHashtable(Hashtable hashtable)
        {
            var entitlement = new Entitlement();
            entitlement.Payload = (String)hashtable ["payload"];
            
            Hashtable payloadHashtable = entitlement.Payload.hashtableFromJson();
            entitlement.EntitlementId = (String)payloadHashtable ["entitlementId"];
            entitlement.UserId = (String)payloadHashtable ["userId"];
            entitlement.Sku = (String)payloadHashtable ["sku"];
            entitlement.Type = (String)payloadHashtable ["type"];
            entitlement.IsConsumable = (Boolean)payloadHashtable ["isConsumable"];
            entitlement.UseCount = Convert.ToInt32(payloadHashtable ["useCount"]);
            entitlement.PackageName = (String)payloadHashtable ["packageName"];
            entitlement.SignatureTimestamp = Convert.ToInt64(payloadHashtable ["signatureTimestamp"]);

            entitlement.Signature = (String)hashtable ["signature"];
            
            return entitlement;
        }

        public static IList<Entitlement> fromArrayList(ArrayList arrayList)
        {
            var entitlements = new List<Entitlement>();

            foreach (Hashtable hashtable in arrayList)
            {
                var entitlement = fromHashtable(hashtable);
                entitlements.Add(entitlement);
            }
            
            return entitlements.AsReadOnly();
        }
    }
}