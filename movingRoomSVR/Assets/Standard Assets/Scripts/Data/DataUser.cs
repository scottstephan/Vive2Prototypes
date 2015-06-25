using UnityEngine;
using System;
using System.Collections;

[System.Serializable]
public class DataUser {
	
	public string _id = null;
	
	public int userid = 0;
	public string guid = null;
	public string username = null;
	public string nickname = null;
	public string password = null;
	public string email = null;
	public string emailpaypal = null;
	public string facebookuserid = null;
	public string facebooktoken = null;
	public string firstname = null;
	public string lastname = null;
	public string displayname = null;
	public string birthdate = null;
	public string gender = null;
	public string latitude = null;
	public string longitiude = null;
	public string location = null;
	public string url = null;
	public string blurb = null;
	public string love_ocean_blurb = null;
	public string avatar = null;

	public int roles = 0;
	public int credits = 0;
	public int freecredits = 0;
	public int sphereid = 0;
	
	public bool guest = false;
	public bool verified = false;
	public bool graffiti = true;
	public bool skiptut = false;
	public bool prouser = true;
	public bool isFreeUser = false;
	public string authtoken = null;
	public string password_reset_hash = null;
	public string connectionschecked = null;
	public string countrycode = null;
	public string twitterid = null;
	public string twitterauthtoken = null;
	public string twitterauthsecret = null;
	public int points = 0;
	public string ipaddress = null;
	public string referrer = null;
	public string useragent = null;
	public string dateVerified = null;
	public string datePointsEarned = null;
	public string dateLastheartbeat = null;
	public string dateCreated = null;
	public string dateUpdated = null;
	
	
	// System Params
	public bool FacebookConnected {
		get { return ( facebooktoken != null && facebooktoken.Length > 0 ); }
	}
	
	public Texture2D AvatarImage = null;
	
	public void SetToFreeUser() {
		//(App.UIManager.habitats as ScrollPanel_Habitats).RemoveNonProSpheres();
		isFreeUser = true;
		_id = "0";
		
		username = nickname = password = "Free User";
		email = emailpaypal = "";
		firstname = lastname = displayname = "Free User";
		birthdate = "1970-01-01";
		gender = "F";
		longitiude = latitude = location = "0";
		url = blurb = love_ocean_blurb = "";
		avatar = "";
		
		guest = true;
		verified = false;
		graffiti = true;
		skiptut = false;
		prouser = false;
		authtoken = password_reset_hash = connectionschecked = "";
		countrycode = "";
		twitterid = twitterauthtoken = twitterauthsecret = "";
		
		ipaddress = "127.0.0.1";
	}
}
