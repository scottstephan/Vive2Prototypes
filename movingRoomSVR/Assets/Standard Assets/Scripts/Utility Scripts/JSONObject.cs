/*
 * http://www.opensource.org/licenses/lgpl-2.1.php
 * JSONObject class
 * for use with Unity
 * Matt Schoen 2010
 */

using UnityEngine;
using System.Collections;

public class JSONObject {
	public ArrayList keys;
	public enum Type { STRING, NUMBER, OBJECT, ARRAY, BOOL, NULL }
	public JSONObject parent;
	public Type type;
	public ArrayList list;
	public string str;
	public double n;
	public bool b;
	
    // class log
    private static Log log = Log.GetLog(typeof(JSONObject).FullName);
	
    public static string Lookup(JSONObject json, string key) {
		JSONObject data = json.GetField(key);
		if( data == null ) {
			return null;
		}
		string val = data.str;
		return val;
	}

	public static long LookupLong( JSONObject json, string key ) {
		JSONObject data = json.GetField(key);
		
		if( data == null ) {
			return 0;
		}
		
		if( data.str == null ) {
			return (long)data.n;
		}

		return long.Parse(data.str);
	}
	
	public static int LookupInt(JSONObject json, string key) {
		JSONObject data = json.GetField(key);
		
		if( data == null ) {
			return 0;
		}
		
		if( data.str == null ) {
			return (int)data.n;
		}

		return int.Parse(data.str);
	}

	public static float LookupFloat(JSONObject json, string key) {
		JSONObject data = json.GetField(key);
		
		if( data == null ) {
			return 0;
		}
		
		if( data.str == null ) {
			return (float)data.n;
		}

		return float.Parse(data.str);
	}

	public static bool LookupBool(JSONObject json, string key) {
		JSONObject data = json.GetField(key);

		if( data == null ) {
			return false;
		}
		
		if( data.type == JSONObject.Type.BOOL ) {
			return data.b;
		}
		
		// uk
		if( data.type == JSONObject.Type.STRING ) {			
			return ( data.str.Equals("true") || (!data.str.Equals("0") && !data.str.Equals("false")));
		}
		
		if( data.type == JSONObject.Type.NUMBER ) {
			return ( data.n > 0 );
		}
		
		return false;
	}
	

	public JSONObject(JSONObject.Type t) {
		type = t;
		switch(t) {
			case Type.ARRAY:
				list = new ArrayList();
				break;
			case Type.OBJECT:
				list = new ArrayList();
				keys = new ArrayList();
				break;
		}
	}
	
	public JSONObject(bool b){
		type = Type.BOOL;
		this.b = b;
	}
	
	public JSONObject(float f){
		type = Type.NUMBER;
		this.n = f;
	}
	
	public JSONObject() {
		type = Type.NULL;
	}
	
	public JSONObject(string str) {	//create a new JSONObject from a string (this will also create any children, and parse the whole string)
		//log.Debug(str);
		if(str.Length > 0) {
			if(str == "true") {
				type = Type.BOOL;
				b = true;
			} else if(str == "false") {
				type = Type.BOOL;
				b = false;
			} else if(str == "null") {
				type = Type.NULL;
			} else if(str[0] == '"') {
				type = Type.STRING;
				this.str = str.Substring(1, str.Length - 2);
			} else {
				try {
					n = System.Convert.ToDouble(str);
					type = Type.NUMBER;
				} catch(System.FormatException) {
					int token_tmp = 0;
					/*
					 * Checking for the following formatting (www.json.org)
					 * object - {"field1":value,"field2":value}
					 * array - [value,value,value]
					 * value - string	- "string"
					 *		 - number	- 0.0
					 *		 - bool		- true -or- false
					 *		 - null		- null
					 */
					switch(str[0]) {
						case '{':
							type = Type.OBJECT;
							keys = new ArrayList();
							list = new ArrayList();
							break;
						case '[':
							type = JSONObject.Type.ARRAY;
							list = new ArrayList();
							break;
						default:
							type = Type.NULL;
							log.Warning("improper JSON formatting :: " + str);
							return;
					}
					int depth = 0;
					bool openquote = false;
					for(int i = 1; i < str.Length; i++) {
						if(str[i] == '"')
							openquote = !openquote;
						if(str[i] == '[' || str[i] == '{')
							depth++;
						if(depth == 0 && !openquote) {
							if(str[i] == ':') {
								keys.Add(str.Substring(token_tmp + 2, i - token_tmp - 3));
								token_tmp = i;
							}
							if(str[i] == ',') {
								list.Add(new JSONObject(str.Substring(token_tmp + 1, i - token_tmp - 1)));
								token_tmp = i;
							}
							if(str[i] == ']' || str[i] == '}')
                                if ( i > 1 )
    								list.Add(new JSONObject(str.Substring(token_tmp + 1, i - token_tmp - 1)));
						}
						if(str[i] == ']' || str[i] == '}')
							depth--;
					}
				}
			}
		} else {
			type = Type.NULL;
		}
	}
	
	public void AddField(string name, bool val){ AddField(name, new JSONObject(val)); }
    
	public void AddField(string name, float val){ AddField(name, new JSONObject(val)); }
    
	public void AddField(string name, int val){ AddField(name, new JSONObject(val)); }
    
	public void AddField(string name, string val){ AddField(name, new JSONObject{ type = JSONObject.Type.STRING, str = val} ); }
    
	public void AddField(string name, JSONObject obj){
        keys.Add(name);
        list.Add(obj);
    }
	
    public JSONObject GetField(string name) {
        if(type == JSONObject.Type.OBJECT)
            for(int i = 0; i < keys.Count; i++)
                if((string)keys[i] == name)
                    return (JSONObject)list[i];
        return null;
    }
	
	public string print(bool readable){
        return print(0,readable);
    }

	public string print(int depth, bool readable) {    //Convert the JSONObject into a stiring
        if(depth++ > 1000){
            log.Error("infinte recursion!");
            return "";
        }
        string str = "";
        switch(type) {
            case Type.STRING:
                str = "\"" + this.str + "\"";
                break;
            case Type.NUMBER:
                str += n;
                break;
            case JSONObject.Type.OBJECT:
                if(list.Count > 0){
                    str = "{";
					if( readable ) {
	                    str += "\n";
					}
                    depth++;
                    for(int i = 0; i < list.Count; i++) {
                        string key = (string)keys[i];
                        JSONObject obj = (JSONObject)list[i];
                        if(obj){
							if( readable ) {
	                            for(int j = 0; j < depth; j++)
    	                            str += "\t";
							}
                            str += "\"" + key + "\":"; 
                            str += obj.print(depth, readable) +",";
							if( readable ) {
        	                    str += "\n";
							}
                        }
                    }
                    str = str.Substring(0, str.Length - 1);
                    str += "}";
                }
                break;
            case JSONObject.Type.ARRAY:
                if(list.Count > 0){
                    str = "[";
					if( readable ) {
	                    str += "\n";
					}
                    depth++;
                    foreach(JSONObject obj in list) {
                        if(obj){
           					if( readable ) {
								for(int j = 0; j < depth; j++)
                                	str += "\t";
							}
                            str += obj.print(depth,readable) + ",";
							if( readable ) {
      	                      str += "\n";
							}
                        }
                    }
                    str = str.Substring(0, str.Length - 1);
                    str += "]";
                }
                break;
            case Type.BOOL:
                str += b;
                break;
            case Type.NULL:
                str = "null";
                break;
        }
        return str;
    }
	
	public static implicit operator bool(JSONObject j) {
		return j != null;
	}
}
