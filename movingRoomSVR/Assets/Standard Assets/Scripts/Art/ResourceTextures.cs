using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResourceTextures {
	
	private static Hashtable textureTable = null;
	
	public static Texture2D GetTexture( string name ) {
		if( textureTable == null ) {
			textureTable = new Hashtable();
		}
		
        if ( !textureTable.ContainsKey( name ) ) {
			textureTable.Add( name, null );
		}
		Texture2D texture = (Texture2D)textureTable[ name ];
		if( texture == null ) {
            texture = (Texture2D)UnityEngine.Resources.Load( name, typeof(Texture2D) );
			if( texture == null ) {
				Log.Main.Error("Trying to load resource texture that does not exist [" + name + "]. Using US flag.");
				name = "flags/us";
				if( textureTable.ContainsKey(name) ) {
					return (Texture2D)textureTable[ name ];
				}
				else {
					textureTable.Add( name, null );
		            texture = (Texture2D)UnityEngine.Resources.Load( name, typeof(Texture2D) );					
				}
			}
			texture.name = name;
			textureTable[ name ] = texture;
		}
		
		return texture;
	}
}
