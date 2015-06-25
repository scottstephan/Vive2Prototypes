var axis : Vector3 = Vector3(0,1,0);
var speed : float = 4;
var randSpeed : float = 2;
var rotAmplitude : float = 4;
var randCol : Vector4;
var widthStart : float ;
var widthEnd : float ;

private var rotSpeed : float = 0;
private var rotTimeOffset : float = 0;
private var myTransform : Transform;

function Start()
{
	myTransform = transform;
	rotSpeed = speed + (Random.value - 0.5)*randSpeed;
	rotTimeOffset = Random.value*360;

	var mat : Material = GetComponent.<Renderer>().material;
	var opacmult = mat.GetFloat("_OpacMult");
	var uvspeed = mat.GetFloat("_uvSpeed");
	
	var newMat : Material = new Material(mat.shader);
	newMat.CopyPropertiesFromMaterial(mat);

	if( GetComponent.<Renderer>().material != null ) {
		DestroyImmediate(GetComponent.<Renderer>().material);
	}
	
	GetComponent.<Renderer>().material = newMat;
	//randCol = Vector4(Random.value,Random.value,Random.value,1);
	randCol = Vector4(Random.value,Random.value,Random.value,opacmult);
	newMat.SetColor("_Color", randCol);
	var randSpeed : float = uvspeed * (0.6 + 2 * Random.value);
	newMat.SetFloat("_uvSpeed", randSpeed);
	newMat.SetFloat("_posX", myTransform.position.x);
	newMat.SetFloat("_posY", myTransform.position.y);
	newMat.SetFloat("_posZ", myTransform.position.z);
	
	var lr : LineRenderer = GetComponent(LineRenderer) as LineRenderer;
	//var w : float= 10 * (1 + 2*Random.value);
	var ws : float= widthStart * (1 + 1*Random.value);
	var we : float= widthEnd * (1 + 1*Random.value);
	lr.SetWidth(we,ws);
	myTransform.localScale.z = 0.4 + Random.value;
}


function Update () 
{
	//axis = ( myCam.position - myTransform.position ).normalized;
	var newAxis : Vector3 = axis + rotAmplitude * Vector3(Mathf.Sin(Time.time*rotSpeed+rotTimeOffset),0,Mathf.Cos(Time.time*rotSpeed+rotTimeOffset)).normalized;
	var rot : Quaternion = Quaternion.LookRotation (newAxis);
	myTransform.rotation = rot;
}
