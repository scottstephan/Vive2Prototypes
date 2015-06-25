using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClickArea {
    public bool valid;
    public int zIndex = 0;
    public Rect rect;
    
    public ClickArea( bool _valid, int _zIndex, Rect _rect )
    {
        valid = _valid;
        zIndex = _zIndex;
        rect = _rect;
    }
}

public class ClickValidator {
    
    private List< ClickArea > areas = new List< ClickArea >();
    
    public void AddArea( ClickArea area )
    {
        // keeps list sorted in reverse z-index order
        int index = areas.FindLastIndex( ( clickArea ) => {
            return clickArea.zIndex > area.zIndex; 
        });
        
        areas.Insert( index < 0 ? 0 : index, area );
		//Debug.LogError("added " + area.rect);
    }
    
    public void RemoveArea( Rect rect )
    {
        areas.RemoveAll( ( clickArea ) => {
            return clickArea.rect == rect;
        });
		//Debug.LogError("remved " + rect);
    }
    
    public void RemoveArea( ClickArea area )
    {
        areas.Remove( area );
		//Debug.LogError("remved " + a);
    }

    /*public bool IsValidClick( Vector2 location )
    {
        for ( int index = 0; index < areas.Count; ++index )
        {
            if ( areas[ index ].rect.Contains( location ) )
            {
                if ( areas[ index ].valid )
                {
					Debug.Log("						debug Adam (IsValidClick): Valid Click inside " + areas[index].rect + "(" + location + ")");
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        
        return false;
    }*/
    
    public bool IsInvalidClick( Vector2 location )
    {
        for ( int index = 0; index < areas.Count; ++index )
        {			
            if ( areas[ index ].rect.Contains( location ) )
            {
                if ( areas[ index ].valid )
                {
                    return false;
                }
                else
                {
					//Debug.Log("						debug Adam (IsInvalidClick): Valid Click inside " + areas[index].rect + "(" + location + ")");
                    return true;
                }
            }
        }
        
        return false;
    }
    
    public override string ToString()
    {
        // TODO: use stringbuilder
        string result = "ClickValidator areas:\n\n";
        for ( int index = 0; index < areas.Count; ++index )
        {
            ClickArea area = areas[ index ];
            result += "Valid: " + area.valid + " zIndex: " + area.zIndex + " rect: " + area.rect.ToString() + "\n";
        }
        
        return result;
    }
}
