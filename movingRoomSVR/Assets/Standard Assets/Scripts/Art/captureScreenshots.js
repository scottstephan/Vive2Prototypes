var lastShot : float = 0;
var pathname : String = "screenshot.";
var camAnim : Animation;
var writeFrames  = false; 
var frameEnd = 360;
var frame : int = 0;
public static var fps : float = 30;
public static var captureTime : float = 0f;
function Update () 
{
	//if(Time.time  - lastShot > 1/25)
	if(Time.time  - lastShot > 1)
	{
		captureTime = frame / fps;
		if(camAnim != null)
		{
			camAnim["Take 001"].time = captureTime ;
			camAnim["Take 001"].enabled = true;
			camAnim.Sample();
			camAnim["Take 001"].enabled = false;
		}
		var imgName = pathname + frame + ".png"; 
		if ( writeFrames && frame <= frameEnd)
		{
			Application.CaptureScreenshot(imgName);
			Debug.Log(captureTime );
		}
		else Debug.Log("Done");
		frame++;
		lastShot = Time.time;
	}
}