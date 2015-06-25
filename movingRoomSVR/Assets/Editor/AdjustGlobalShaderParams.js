//@MenuItem("WEMOTools/Adjust Global Shader Params")
static function AdjustGlobalShaderParams ()
{
	
	var master : GameObject = GameObject.Find("MasterOceanObject") as GameObject;
	
	if( master != null ) {
		var globalAdjust = master.GetComponent("GlobalOceanShaderAdjust");
		
		if( globalAdjust != null ) {
			globalAdjust.AdjustToParams( false );
		}
	}
}
