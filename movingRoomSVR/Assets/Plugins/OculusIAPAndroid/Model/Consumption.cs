using System;
using System.Collections;
using OculusIAPAndroid.MiniJSON;
using System.Collections.Generic;

namespace OculusIAPAndroid.Model
{
    /// <summary>
    /// This is class represents a consumption record of an entitlement.
    /// </summary>
    public class Consumption
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
        /// Gets the use count consumed.
        /// </summary>
        /// <value>The use count consumed.</value>
        public Int32 UseCountConsumed { get; private set; }

        /// <summary>
        /// Gets the tracking GUID of this consumption record.
        /// </summary>
        /// <value>The tracking GUID of this consumption record.</value>
        public String TrackingGuid { get; private set; }

        /// <summary>
        /// Gets the Android package name of this application.
        /// </summary>
        /// <value>The Android package name of this application.</value>
        public String PackageName { get; private set; }

        /// <summary>
        /// Gets the signature timestamp.
        /// </summary>
        /// <value>The signature timestamp.</value>
        public Int64 SignatureTimestamp { get; private set; }

        /// <summary>
        /// Gets the original json payload of this consumption data.
        /// </summary>
        /// <value>The original json payload of this consumption data.</value>
        public String Payload { get; private set; }

        /// <summary>
        /// Gets the signature of the original json payload.
        /// </summary>
        /// <value>The signature of the original json payload.</value>
        public String Signature { get; private set; }

        Consumption()
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

        public static Consumption fromHashtable(Hashtable hashtable)
        {
            var consumption = new Consumption();
            consumption.Payload = (String)hashtable ["payload"];
            
            Hashtable payloadHashtable = consumption.Payload.hashtableFromJson();
            consumption.EntitlementId = (String)payloadHashtable ["entitlementId"];
            consumption.UserId = (String)payloadHashtable ["userId"];
            consumption.Sku = (String)payloadHashtable ["sku"];
            consumption.Type = (String)payloadHashtable ["type"];
            consumption.UseCountConsumed = Convert.ToInt32(payloadHashtable ["useCountConsumed"]);
            consumption.TrackingGuid = (String)payloadHashtable ["trackingGuid"];
            consumption.PackageName = (String)payloadHashtable ["packageName"];
            consumption.SignatureTimestamp = Convert.ToInt64(payloadHashtable ["signatureTimestamp"]);

            consumption.Signature = (String)hashtable ["signature"];
            
            return consumption;
        }

        public static IList<Consumption> fromArrayList(ArrayList arrayList)
        {
            var consumptions = new List<Consumption>();

            foreach (Hashtable hashtable in arrayList)
            {
                var consumption = fromHashtable(hashtable);
                consumptions.Add(consumption);
            }
            
            return consumptions.AsReadOnly();
        }
    }
}