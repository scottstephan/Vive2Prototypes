var prefab0 : GameObject;
var count : int;
var offset : Vector3;

function Start()
{
	var origPos : Vector3 = transform.position;
	for (var i : int =0;i<count;i++) 
	{
		var pos : Vector3 = origPos + i*offset;
		var clone : GameObject = Instantiate (prefab0, pos, Quaternion.identity) as GameObject;
		clone.transform.parent = transform;
		//clone.transform.localScale = Vector3(col.r,col.g,col.b);
	}
}