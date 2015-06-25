using System;
using UnityEngine;
using OculusIAPAndroid.Model;
using System.Collections.Generic;
using OculusIAPAndroid.MiniJSON;

namespace OculusIAPAndroid
{
    /// <summary>
    /// This is a group of events triggered by the Oculus Store IAP.
    /// </summary>
    public class OculusIAPManager : MonoBehaviour
    {
        /// <summary>
        /// Occurs when OculusIAP.Init() succeeded.
        /// </summary>
        public static event Action BillingSupportedEvent;

        /// <summary>
        /// Occurs when OculusIAP.Init() failed. arg0 is the error message.
        /// </summary>
        public static event Action<string> BillingNotSupportedEvent;

        /// <summary>
        /// Occurs when OculusIAP.GetUser() succeeded. arg0 is the current user. It could be null.
        /// </summary>
        public static event Action<User> GetUserSucceededEvent;

        /// <summary>
        /// Occurs when OculusIAP.GetUser() failed. arg0 is the error message.
        /// </summary>
        public static event Action<string> GetUserFailedEvent;

        /// <summary>
        /// Occurs when OculusIAP.Login() succeeded. arg0 is the current user.
        /// </summary>
        public static event Action<User> LoginSucceededEvent;

        /// <summary>
        /// Occurs when OculusIAP.Login() failed. arg0 is the error message.
        /// </summary>
        public static event Action<string> LoginFailedEvent;

        /// <summary>
        /// Occurs when OculusIAP.QueryInventory() succeeded. arg0 is the inventory of the current user.
        /// </summary>
        public static event Action<Inventory> QueryInventorySucceededEvent;

        /// <summary>
        /// Occurs when OculusIAP.QueryInventory() failed. arg0 is the error message.
        /// </summary>
        public static event Action<string> QueryInventoryFailedEvent;

        /// <summary>
        /// Occurs when OculusIAP.PurchaseOffer() succeeded. arg0 is the list of purchased entitlements.
        /// </summary>
        public static event Action<IList<Entitlement>> PurchaseOfferSucceededEvent;

        /// <summary>
        /// Occurs when OculusIAP.PurchaseOffer() failed. arg0 is the error message.
        /// </summary>
        public static event Action<string> PurchaseOfferFailedEvent;

        /// <summary>
        /// Occurs when OculusIAP.ConsumeEntitlment() succeeded. arg0 is the consumption record.
        /// </summary>
        public static event Action<Consumption> ConsumeEntitlementSucceededEvent;

        /// <summary>
        /// Occurs when OculusIAP.ConsumeEntitlment() failed. arg0 is the error message.
        /// </summary>
        public static event Action<string> ConsumeEntitlementFailedEvent;

        void Awake()
        {
            gameObject.name = GetType().ToString();
            DontDestroyOnLoad(this);
        }


        void BillingSupported(string ignore)
        {
            if (BillingSupportedEvent != null)
            {
                BillingSupportedEvent();
            }
        }
        
        
        void BillingNotSupported(string error)
        {
            if (BillingNotSupportedEvent != null)
            {
                BillingNotSupportedEvent(error);
            }
        }

        void GetUserSucceeded(string json)
        {
            if (GetUserSucceededEvent != null)
            {
                User user;
                if (json == "null")
                {
                    user = null;
                }
                else
                {
                    user = User.fromHashtable(json.hashtableFromJson());
                }

                GetUserSucceededEvent(user);
            }
        }
        
        void GetUserFailed(string error)
        {
            if (GetUserFailedEvent != null)
            {
                GetUserFailedEvent(error);
            }
        }

        void LoginSucceeded(string json)
        {
            if (LoginSucceededEvent != null)
            {
                var user = User.fromHashtable(json.hashtableFromJson());

                LoginSucceededEvent(user);
            }
        }

        void LoginFailed(string error)
        {
            if (LoginFailedEvent != null)
            {
                LoginFailedEvent(error);
            }
        }

        void QueryInventorySucceeded(string json)
        {
            if (QueryInventorySucceededEvent != null)
            {
                var inventory = Inventory.fromHashtable(json.hashtableFromJson());

                QueryInventorySucceededEvent(inventory);
            }
        }
        
        void QueryInventoryFailed(string error)
        {
            if (QueryInventoryFailedEvent != null)
            {
                QueryInventoryFailedEvent(error);
            }
        }

        void PurchaseOfferSucceeded(string json)
        {
            if (PurchaseOfferSucceededEvent != null)
            {
                var entitlements = Entitlement.fromArrayList(json.arrayListFromJson());

                PurchaseOfferSucceededEvent(entitlements);
            }
        }
        
        
        void PurchaseOfferFailed(string error)
        {
            if (PurchaseOfferFailedEvent != null)
            {
                PurchaseOfferFailedEvent(error);
            }
        }
        
        void ConsumeEntitlementSucceeded(string json)
        {
            if (ConsumeEntitlementSucceededEvent != null)
            {
                var entitlement = Consumption.fromHashtable(json.hashtableFromJson());

                ConsumeEntitlementSucceededEvent(entitlement);
            }
        }

        void ConsumeEntitlementFailed(string error)
        {
            if (ConsumeEntitlementFailedEvent != null)
            {
                ConsumeEntitlementFailedEvent(error);
            }
        }
    }
}

