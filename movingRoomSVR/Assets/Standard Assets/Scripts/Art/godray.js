#pragma strict
var lightBeam : GameObject;
var lightRadius : float = 100;
var beamCount : int = 30;
var rotAmplitude : float = 0.1;
var speed : float = 0.1;
var camOffset : Vector3 = Vector3(-200,0,0);
var axis : Vector3 = Vector3(0,-1,0.3);
var lightTransform : Transform = null;


private var myRays = new Array ();
private var myTransform :Transform;
private var myRenderer : Renderer;
private var myCam : Transform;
private var myAxis : Vector3 ;

function Start()
{
	myTransform = transform as Transform;
	//myRenderer = GetComponent.<Renderer>();
	//var cam : GameObject =  GameObject.FindWithTag ("MainCamera");
	//myCam = cam.transform;
	//renderer.enabled = false;
	for (var i=0;i<beamCount;i++) 
	{
		var randomPos : Vector3 = lightRadius*(Vector3(Random.value-0.5,0,Random.value-0.5));
		//var randomPos : Vector3 = lightRadius*(Vector3(Mathf.Sin(i*360/beamCount),0,Mathf.Cos(i*360/beamCount)));
		var wp : Vector3 = myTransform.position + randomPos;
		var norm : Vector3 = myTransform.TransformDirection(Vector3(randomPos.x,-3*lightRadius,randomPos.z+1.3*lightRadius));
		//var norm : Vector3 = myTransform.TransformDirection(Vector3(randomPos.x,-3*lightRadius,randomPos.z));
		var rot : Quaternion = Quaternion.LookRotation (norm);
		//print (i + " " + col);
		var clone : GameObject = Instantiate (lightBeam, wp, rot) as GameObject;
		clone.transform.parent =myTransform;
		myRays.push(clone);
		clone.name = lightBeam.name + i;
		//var godray_beam : godray_beam = clone.GetComponent(godray_beam) as godray_beam;
		var godray_beam : godrayLine_beam = clone.GetComponent(godrayLine_beam) as godrayLine_beam;
		//godray_beam.axis = Vector3(randomPos.x*1,-3*lightRadius,randomPos.z*1).normalized;
		if(lightTransform != null) 
		{
			godray_beam.axis = lightTransform.forward;
			myAxis = lightTransform.forward;
		}
		else 
		{
			godray_beam.axis = axis.normalized;
			myAxis = axis.normalized;
		}
		rot = Quaternion.LookRotation (myAxis);
		clone.transform.rotation = rot;
	}
}

/*

function Update()
{
	myTransform.position = myCam.position + camOffset;
	myTransform.position.y = 50;

	
	for(var i : int = 0;i<beamCount;i++)
	{
		var obj : GameObject = myRays[i] as GameObject;
		var randomPos : Vector3 = lightRadius*(Vector3(Mathf.Sin(i*360/beamCount + Time.time * speed),0,Mathf.Cos(i*360/beamCount + Time.time * speed)));
		var newAxis : Vector3 = myAxis + rotAmplitude * randomPos.normalized;
		var rot : Quaternion = Quaternion.LookRotation (newAxis);
		obj.transform.rotation = rot;
		//obj.transform.position = myTransform.position + randomPos;
	}
	
}
*/

