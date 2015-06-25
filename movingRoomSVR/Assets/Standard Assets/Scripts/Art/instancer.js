var prefab0 : GameObject;
var cross1 : boolean = true;
var cross2 : boolean = true;
var rotationOnNormal : boolean = false;
var randomScale : Vector3 = Vector3(0.2,0.2,0.2);
var posOffset : float = -10;

private var mesh : Mesh;
mesh = GetComponent(MeshFilter).mesh;
private var verts : Vector3[];
verts = GetComponent(MeshFilter).mesh.vertices;
private var vertsN : Vector3[];
vertsN = GetComponent(MeshFilter).mesh.normals;
private var vertsC : Color[];
vertsC = GetComponent(MeshFilter).mesh.colors;
private var myTransform :Transform;
private var myRenderer : Renderer;

function Start()
{
	myTransform = transform as Transform;
	myRenderer = GetComponent.<Renderer>();
	//renderer.enabled = false;
	for (var i=0;i<verts.length;i++) 
	{
		var vert : Vector3 = verts[i];
		var wp : Vector3 = myTransform.TransformPoint(vert);
		var norm : Vector3 = myTransform.TransformDirection(vertsN[i]);
		if(rotationOnNormal) var randUp : Vector3 = Vector3(Random.value-0.5,0.7,Random.value-0.5).normalized;
		else randUp = Vector3.up;
		if(cross1) var bi : Vector3 = Vector3.Cross(norm,randUp);
		if(cross2) norm = Vector3.Cross(bi,norm);
		var rot : Quaternion = Quaternion.LookRotation (norm,randUp);
		//var col : Color = vertsC[i];
		//print (i + " " + col);
		var clone : GameObject = Instantiate (prefab0, wp + norm*posOffset, rot) as GameObject;
		var randSc : Vector3 = Vector3(1+Random.value*randomScale.x ,1+Random.value*randomScale.y ,1+Random.value*randomScale.z ); 
		clone.transform.localScale = randSc;
		clone.transform.parent = myTransform;
		
		// set vertex colors
		var red : float = Random.value;
		var meshCmpts = clone.GetComponentsInChildren(MeshFilter);
		for (var meshCmpt : MeshFilter in meshCmpts) {
			var mesh = meshCmpt.mesh;
			//var vertsC : Color[]  = mesh.colors ;
			var vertsC : Color[]  = mesh.colors ;
			for (var j=0;j<vertsC.length;j++) 
			{
				var green : float = Random.value;
				var blue : float = Random.value;
				var a : float = Random.value;
				//vertsC[i] = Color(red,green,blue,a);
				vertsC[j] = Color(red,green,blue,a);
				//print(vertsC[j]);
			}
			//mesh.colors  = vertsC;
			mesh.colors  = vertsC;
		}
	}
}