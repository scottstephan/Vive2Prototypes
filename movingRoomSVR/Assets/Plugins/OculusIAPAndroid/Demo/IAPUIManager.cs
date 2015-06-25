using UnityEngine;
using System.Collections;
using OculusIAPAndroid;
using OculusIAPAndroid.Model;
using System.Collections.Generic;
using System;

namespace OculusIAPAndroid.Demo
{
#if UNITY_ANDROID
    /// <summary>
    /// Oculus IAP sample menu.
    /// </summary>
    public class IAPUIManager : MonoBehaviour
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="OculusIAPAndroid.OculusIAP"/> is initialized.
        /// </summary>
        /// <value><c>true</c> if initialized; otherwise, <c>false</c>.</value>
        public static bool Initialized { get; set; }

        /// <summary>
        /// Gets or sets the current user.
        /// </summary>
        /// <value>The current user.</value>
        public static User CurrentUser { get; set; }

        /// <summary>
        /// Gets or sets the current inventory.
        /// </summary>
        /// <value>The current inventory.</value>
        public static Inventory CurrentInventory { get; set; }

        /// <summary>
        /// Gets or sets the purchased entitlements.
        /// </summary>
        /// <value>The purchased entitlements.</value>
        public static IList<Entitlement> PurchasedEntitlements { get; set; }

        /// <summary>
        /// Gets or sets the consumption.
        /// </summary>
        /// <value>The consumption.</value>
        public static Consumption Consumption { get; set; }

        /// <summary>
        /// The scroll view position.
        /// </summary>
        private Vector2 scrollPos;

        void OnGUI()
        {
            float horizRatio = Screen.width / 432.0f;

            float vertRatio = Screen.height / 768.0f;

            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(horizRatio, vertRatio, 1.0f));

            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(432.0f), GUILayout.Height(768.0f));

            if (GUILayout.Button("Initialize IAP", GUILayout.Width(200)))
            {
                // the first action is initialize the Oculus IAP.
                OculusIAP.Init("YourPublicKeyBase64");
            }

            if (Initialized)
            {
                if (GUILayout.Button("GetUser", GUILayout.Width(200)))
                {
                    // after that call GetUser() to get the current user. If null, call Login()
                    OculusIAP.GetUser();
                }

                if (GUILayout.Button("Login", GUILayout.Width(200)))
                {
                    OculusIAP.Login();
                }

                if (CurrentUser == null)
                {
                    GUILayout.Label("No User Yet.", GUILayout.Width(200));
                } else
                {
                    GUILayout.Label("User: " + CurrentUser, GUILayout.Width(400));

                    if (GUILayout.Button("Query Inventory", GUILayout.Width(200)))
                    {
                        var moreSkus = new string[]
                        {
                            "bird",
                            "birdupgrade"
                        };

                        // after the current user is available, call QueryInventory to get the inventory of the user.
                        OculusIAP.QueryInventory(true, moreSkus);
                    }
                    
                    if (CurrentInventory == null)
                    {
                        GUILayout.Label("Inventory Not Queried Yet.", GUILayout.Width(200));
                    } else
                    {
                        // now you get the offer details and the entitlements the user has.
                        GUILayout.Label("Entitlements you have: ");
                        foreach (Entitlement entitlement in CurrentInventory.Entitlements)
                        {
                            GUILayout.Label(entitlement.ToString(), GUILayout.Width(400));
                        }

                        foreach (Offer offer in CurrentInventory.Offers)
                        {
                            if (GUILayout.Button("Purchase Offer: " + offer.Title, GUILayout.Width(200)))
                            {
                                // call PurchaseOffer to launch the purchase flow.
                                OculusIAP.PurchaseOffer(offer.OfferId);
                            }
                        }

                        if (PurchasedEntitlements != null)
                        {
                            // after PurchaseOffer succeeds, you will get a list of the purchased entitlements.
                            GUILayout.Label("Purchased Entitlements: ");
                            foreach (Entitlement entitlement in PurchasedEntitlements)
                            {
                                GUILayout.Label(entitlement.ToString(), GUILayout.Width(400));
                            }
                        }

                        foreach (Entitlement entitlement in CurrentInventory.Entitlements)
                        {
                            // for a consumable entitlement, you can call ConsumeEntitlement to consume it.
                            if (entitlement.IsConsumable)
                            {
                                if (GUILayout.Button("Consume Entitlement: " + entitlement.Sku, GUILayout.Width(200)))
                                {
                                    OculusIAP.ConsumeEntitlement(entitlement.EntitlementId, 1, Guid.NewGuid().ToString());
                                }
                            }
                        }

                        if (Consumption != null)
                        {
                            // finally you will get the consumption record.
                            GUILayout.Label("Consumed Entitlement: " + Consumption, GUILayout.Width(400));
                        }
                    }
                }
            }

            GUILayout.EndScrollView();
        }
    }
#endif
}