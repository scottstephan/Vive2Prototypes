using UnityEngine;
using System.Collections;
namespace OculusIAPAndroid
{
	#if UNITY_ANDROID
    /// <summary>
    /// This is a group of the Oculus IAP functions.
    /// </summary>
    public class OculusIAP
    {
        static readonly AndroidJavaObject plugin;

        static OculusIAP()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                return;
            }
        
            using (var pluginClass = new AndroidJavaClass("com.oculusvr.shop.iap.DefaultIAPPlugin"))
            {
                plugin = pluginClass.CallStatic<AndroidJavaObject>("getInstanceWithLogging");
            }
        }
        OculusIAP()
        {
        }

        /// <summary>
        /// Initialize the Oculus IAP module.
        /// OculusIAPManager.BillingSupportedEvent is invoked when succeeded.
        /// OculusIAPManager.BillingNotSupportedEvent is invoked when failed.
        /// </summary>
        /// <param name="publicKeyBase64">the public key in base64 used to verify the signature.</param>
        public static void Init(string publicKeyBase64)
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                return;
            }
        
            plugin.Call("init", publicKeyBase64);
        }

        /// <summary>
        /// Releases all resource used by the <see cref="OculusIAPAndroid.OculusIAP"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="OculusIAPAndroid.OculusIAP"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="OculusIAPAndroid.OculusIAP"/> in an unusable state. After
        /// calling <see cref="Dispose"/>, you must release all references to the
        /// <see cref="OculusIAPAndroid.OculusIAP"/> so the garbage collector can reclaim the memory that the
        /// <see cref="OculusIAPAndroid.OculusIAP"/> was occupying.</remarks>
        public static void Dispose()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                return;
            }
        
            plugin.Call("dispose");
        }

        /// <summary>
        /// Gets the current user that is logged in Oculus App Store.
        /// OculusIAPManager.GetUserSucceededEvent is invoked when succeeded.
        /// OculusIAPManager.GetUserFailedEvent is invoked when failed.
        /// </summary>
        public static void GetUser()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                return;
            }
            
            plugin.Call("getUser");
        }

        /// <summary>
        /// Launches the Login flow through the Oculus App Store.
        /// OculusIAPManager.GetUserSucceededEvent is invoked when succeeded.
        /// OculusIAPManager.GetUserFailedEvent is invoked when failed.
        /// </summary>
        public static void Login()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                return;
            }
        
            plugin.Call("login");
        }

        /// <summary>
        /// Query the inventory that the current user has.
        /// OculusIAPManager.QueryInventorySucceededEvent is invoked when succeeded.
        /// OculusIAPManager.QueryInventoryFailedEvent is invoked when failed.
        /// </summary>
        /// <param name="queryOffers">whether to query the offer details.</param>
        /// <param name="moreSkus">more skus to include in the inventory.</param>
        public static void QueryInventory(bool queryOffers, string[] moreSkus)
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                return;
            }
        
            plugin.Call("queryInventory", queryOffers, moreSkus);
        }


        /// <summary>
        /// Launches the Purchase flow of the given offer through the Oculus App Store.
        /// OculusIAPManager.PurchaseOfferSucceededEvent is invoked when succeeded.
        /// OculusIAPManager.PurchaseOfferFailedEvent is invoked when failed.
        /// </summary>
        /// <param name="offerId">the offer identifier.</param>
        public static void PurchaseOffer(string offerId)
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                return;
            }
        
            plugin.Call("purchaseOffer", offerId);
        }

        /// <summary>
        /// Consumes the entitlement with the given id and by the given user count.
        /// OculusIAPManager.ConsumeEntitlementSucceededEvent is invoked when succeeded.
        /// OculusIAPManager.ConsumeEntitlementFailedEvent is invoked when failed.
        /// </summary>
        /// <param name="entitlementId">the entitlement identifier.</param>
        /// <param name="useCount">the use count to consume.</param>
        /// <param name="trackingGuid">the tracking guid to identify this consumption.</param>
        public static void ConsumeEntitlement(string entitlementId, int useCount, string trackingGuid)
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                return;
            }
        
            plugin.Call("consumeEntitlement", entitlementId, useCount, trackingGuid);
        }
    }
#endif
}