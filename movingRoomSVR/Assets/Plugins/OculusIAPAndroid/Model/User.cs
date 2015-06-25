using System;
using System.Collections;
using OculusIAPAndroid.MiniJSON;

namespace OculusIAPAndroid.Model
{
    /// <summary>
    /// This class represents the current user that has logged in Oculus App Store.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        public String UserId { get; private set; }

        /// <summary>
        /// Gets the username.
        /// </summary>
        /// <value>The username.</value>
        public String Username { get; private set; }

        User()
        {
        }

        public override string ToString()
        {
            return toHashtable().toJson();
        }

        public Hashtable toHashtable()
        {
            Hashtable hashtable = new Hashtable();

            hashtable.Add("userId", UserId);
            hashtable.Add("username", Username);

            return hashtable;
        }

        public static User fromHashtable(Hashtable hashtable)
        {
            var user = new User();

            user.UserId = (String)hashtable ["userId"];

            user.Username = (String)hashtable ["username"];

            return user;
        }
    }
}

