@MenuItem("WEMOTools/Pre Bake Lightmap")
static function AdjustGlobalShaderParams ()
{
	var objs:GameObject[] = GameObject.FindObjectsOfType(GameObject);
	var count:int = objs.length;
	for(var i:int = 0; i < count; i ++)
	{
		var obj:GameObject = objs[i];
		var fb:FishBowl = obj.GetComponent(FishBowl);
		//var wo:WemoObject = obj.GetComponent(WemoObject);
		//Debug.Log(i + " " + obj.name + "  layer " + obj.layer);
		if( fb || obj.layer == 16)
		{
			Debug.Log("################# " + obj);
			var ren:Renderer = obj.GetComponent.<Renderer>();
			ren.castShadows = false;
			ren.receiveShadows = false;
		}
	}

}
