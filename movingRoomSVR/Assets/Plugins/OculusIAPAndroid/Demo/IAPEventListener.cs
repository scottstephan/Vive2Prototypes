using UnityEngine;
using System.Collections;
using OculusIAPAndroid;
using OculusIAPAndroid.Model;
using System.Collections.Generic;

namespace OculusIAPAndroid.Demo
{
#if UNITY_ANDROID
    /// <summary>
    /// Sample Oculus Store IAP event listener.
    /// Use this as reference for building your own solution for listening to IAP callbacks and events.
    /// </summary>
    public class IAPEventListener : MonoBehaviour
    {
        /// <summary>
        /// Unity override of OnEnable
        /// </summary>
        void OnEnable()
        {
            OculusIAPManager.BillingSupportedEvent += OnBillingSupported;
            OculusIAPManager.BillingNotSupportedEvent += OnBillingNotSupported;

            OculusIAPManager.GetUserSucceededEvent += OnGetUserSucceeded;
            OculusIAPManager.GetUserFailedEvent += OnGetUserFailed;

            OculusIAPManager.LoginSucceededEvent += OnLoginSucceeded;
            OculusIAPManager.LoginFailedEvent += OnLoginFailed;

            OculusIAPManager.QueryInventorySucceededEvent += OnQueryInventorySucceeded;
            OculusIAPManager.QueryInventoryFailedEvent += OnQueryInventoryFailed;

            OculusIAPManager.PurchaseOfferSucceededEvent += OnPurchaseOfferSucceeded;
            OculusIAPManager.PurchaseOfferFailedEvent += OnPurchaseOfferFailed;

            OculusIAPManager.ConsumeEntitlementSucceededEvent += OnConsumeEntitlementSucceeded;
            OculusIAPManager.ConsumeEntitlementFailedEvent += OnConsumeEntitlementFailed;
        }

        /// <summary>
        /// Unity override of OnDisable
        /// </summary>
        void OnDisable()
        {
            OculusIAPManager.BillingSupportedEvent -= OnBillingSupported;
            OculusIAPManager.BillingNotSupportedEvent -= OnBillingNotSupported;
        
            OculusIAPManager.GetUserSucceededEvent -= OnGetUserSucceeded;
            OculusIAPManager.GetUserFailedEvent -= OnGetUserFailed;

            OculusIAPManager.LoginSucceededEvent -= OnLoginSucceeded;
            OculusIAPManager.LoginFailedEvent -= OnLoginFailed;
        
            OculusIAPManager.QueryInventorySucceededEvent -= OnQueryInventorySucceeded;
            OculusIAPManager.QueryInventoryFailedEvent -= OnQueryInventoryFailed;
        
            OculusIAPManager.PurchaseOfferSucceededEvent -= OnPurchaseOfferSucceeded;
            OculusIAPManager.PurchaseOfferFailedEvent -= OnPurchaseOfferFailed;
        
            OculusIAPManager.ConsumeEntitlementSucceededEvent -= OnConsumeEntitlementSucceeded;
            OculusIAPManager.ConsumeEntitlementFailedEvent -= OnConsumeEntitlementFailed;
        }

        void OnBillingSupported()
        {
            Debug.Log("OnBillingSupported");

            IAPUIManager.Initialized = true;
        }

        void OnBillingNotSupported(string error)
        {
            Debug.Log("OnBillingNotSupported: " + error);
        }

        void OnGetUserSucceeded(User user)
        {
            Debug.Log("OnGetUserSucceeded: " + user);

            IAPUIManager.CurrentUser = user;
        }
        
        void OnGetUserFailed(string error)
        {
            Debug.Log("OnGetUserFailed: " + error);
        }

        void OnLoginSucceeded(User user)
        {
            Debug.Log("OnLoginSucceeded: " + user);

            IAPUIManager.CurrentUser = user;
        }
    
        void OnLoginFailed(string error)
        {
            Debug.Log("OnLoginFailed: " + error);
        }

        void OnQueryInventorySucceeded(Inventory inventory)
        {
            Debug.Log("OnQueryInventorySucceeded: " + inventory);

            IAPUIManager.CurrentInventory = inventory;
        }

        void OnQueryInventoryFailed(string error)
        {
            Debug.Log("OnQueryInventoryFailed: " + error);
        }

        void OnPurchaseOfferSucceeded(IList<Entitlement> entitlements)
        {
            foreach (Entitlement entitlement in entitlements)
            {
                Debug.Log("OnPurchaseOfferSucceeded: " + entitlement);
            }

            IAPUIManager.PurchasedEntitlements = entitlements;
        }

        void OnPurchaseOfferFailed(string error)
        {
            Debug.Log("OnPurchaseOfferFailed: " + error);
        }

        void OnConsumeEntitlementSucceeded(Consumption consumption)
        {
            Debug.Log("OnConsumeEntitlementSucceeded: " + consumption);

            IAPUIManager.Consumption = consumption;
        }

        void OnConsumeEntitlementFailed(string error)
        {
            Debug.Log("OnConsumeEntitlementFailed: " + error);
        }
    }
#endif
}