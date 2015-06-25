var axis : Vector3 = Vector3(0,1,0);
var speed : float = 4;
var randSpeed : float = 2;
var rotAmplitude : float = 4;
var randTgt : Vector4;

private var rotSpeed : float = 0;
private var rotTimeOffset : float = 0;
private var myTransform : Transform;
private var myCam : Transform;

function Start()
{
	myTransform = transform;
	//var cam : GameObject = GameObject.FindWithTag ("MainCamera");
	//myCam = cam.transform;
	rotSpeed = speed + (Random.value - 0.5)*randSpeed;
	rotTimeOffset = Random.value*360;
	
	randTgt = Vector4(Random.value,Random.value,Random.value,1.0);
	var meshCmpt = GetComponent(MeshFilter) as MeshFilter;
	var mesh = meshCmpt.mesh;
	var vertsC : Vector4[]  = mesh.tangents ;
	for (var i=0;i<vertsC.length;i++) 
	{
		vertsC[i] = randTgt;
	}
	mesh.tangents  = vertsC;
}

/*
function Update () 
{
	//axis = ( myCam.position - myTransform.position ).normalized;
	var newAxis : Vector3 = axis + rotAmplitude * Vector3(Mathf.Sin(Time.time*rotSpeed+rotTimeOffset),0,Mathf.Cos(Time.time*rotSpeed+rotTimeOffset)).normalized;
	var rot : Quaternion = Quaternion.LookRotation (newAxis);
	myTransform.rotation = rot;
}
*/