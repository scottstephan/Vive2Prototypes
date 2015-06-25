using UnityEngine;
using System.Collections;

public class HashtableExt {
	
	public static int LoadAsInt( object ob ) {
		if( ob == null ) {
			return 0;
		}
		
		if( ob is int ) {
			return (int)ob;
		}
		else if( ob is string ) {
			int ret = 0;
			try {
				ret = int.Parse((string)ob);
			}
			catch {
				Log.Main.Warning("Hashtable has bad object data. An int was expected. [" + ob.ToString() + "]");
			}
			return ret;
		}
		
		Log.Main.Warning("Hashtable has bad object data. An int was expected. [" + ob.GetType().ToString() + "]");
		
		return 0;
	}
	
	public static float LoadAsFloat( object ob ) {
		if( ob == null ) {
			return 0f;
		}
		
		if( ob is float ) {
			return (float)ob;
		}
		else if( ob is string ) {
			float ret = 0;
			try {
				ret = float.Parse((string)ob);
			}
			catch {
				Log.Main.Warning("Hashtable has bad object data. A float was expected. [" + ob.ToString() + "]");
			}
			return ret;
		}
		
		Log.Main.Warning("Hashtable has bad object data. A float was expected. [" + ob.GetType().ToString() + "]");
		
		return 0;
	}
	
	public static long LoadAsLong( object ob ) {
		if( ob == null ) {
			return 0;
		}
		if( ob is long ) {
			return (long)ob;
		}
		else if( ob is int ) {
			return (long)((int)ob);
		}
		else if( ob is string ) {
			long ret = 0;
			try {
				ret = long.Parse((string)ob);
			}
			catch {
				Log.Main.Warning("Hashtable has bad object data. A long was expected. [" + ob.ToString() + "]");
			}
			return ret;
		}
		
		Log.Main.Warning("Hashtable has bad object data. A long was expected. [" + ob.GetType().ToString() + "]");
		
		return 0;
	}
	
	public static bool LoadAsBool( object ob ) {
		if ( ob == null ) 
			return false;
		
		if( ob is bool ) {
			return (bool)ob;
		}
		else if( ob is string ) {
			string sob = ((string)ob).ToLower();
			
			if( sob.Equals("false") || sob.Equals("0") ) {
				return false;
			}
			
			return true;
		}
		else if( ob is int ) {
			if( (int)ob == 0 ) {
				return false;
			}
			
			return true;
		}
		Log.Main.Warning("Hashtable has bad object data. An int was expected. [" + ob.GetType().ToString() + "]");
		
		return false;
	}
	
}
