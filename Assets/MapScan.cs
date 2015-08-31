using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;


public class MapScan : MonoBehaviour {
	//一些限定常量值
	private float speed = 0.04f;
	private float scale;
	private float minPinchDistance = 10.0f;
	private float angleRangeOfRotate = 80;
	private float minHigh = 0.0f;
	private float maxHigh = 0.0f;
	private float minAngle = 45f;
	private float maxAngle = 89f;
	private float maxRotateHigh = 20;
	private float maxUpDownHigh = 23;

	private float pinchDistanceDelta = 0.0f;
	private bool isDistanceChangeHuge = false;
	private bool isRotateBack = false;
	private bool hasRotated = false;
	private bool hasUpDown = false;

	private bool supportRotate = true;
//	private bool supportUpDown = true;

	private bool isRotate = false;
	public string gps_info = "";
	private GameObject[] texts = new GameObject[13];
	GameObject ball;
	private CharacterController controller;

	private float testLon = 116.272430f;
	private float testLat = 39.991851f;
	private string testBallLoc = "";
	private string clickMessage = "info:";
	private float baseLon = 0f;
	private float baseLat = 0f;
	Camera camera;
	KeyAndValue kav;
	GoogleProjection gp = new GoogleProjection ();

	string currentGesture = "";//begin,zoom,rotate,updown
	ArrayList gestureList = new ArrayList();  
	string platform = "android";

	string testCoorString0 = "";
	string testCoorString1 = "";

	Vector2 touchBefore;
	Vector2 touch0before;
	Vector2 touch1before;


	// Use this for initialization
	void Start () {
//		Time.timeScale = 0.2f;
		camera = GetComponent<Camera>();
		kav = new KeyAndValue ();
		if (platform.Equals ("ios")) {
			scale = 1500;
			minPinchDistance = 10.0f;
		} else {
			scale = 1500;
			minPinchDistance = 10.0f;
		}

		minHigh = transform.position.y / 5;
		maxHigh = 70;
		print ("start");
		StartCoroutine(StartGPS());
		for (int i=1; i<=13; i++) {
			GameObject text;
			if(i<10){
				text = GameObject.Find ("Text0"+i);
			}else{
				text = GameObject.Find ("Text"+i);
			}
			texts[i-1] = text;
		}
		print ("length is "+texts.Length);


		ball = GameObject.Find ("ball");
		controller = ball.GetComponent<CharacterController>();
	}



	// Update is called once per frame
	void Update () {
		gps_info = "手机实际位置: " + Input.location.lastData.longitude + "," + Input.location.lastData.latitude;
		testBallLoc = "小球位置: " + testLon + "," + testLat;

		if (baseLat > 1 && baseLon > 1) {//start follow
			testLon += Input.location.lastData.longitude - baseLon;
			testLat += Input.location.lastData.latitude -baseLat;
			baseLon = Input.location.lastData.longitude;
			baseLat = Input.location.lastData.latitude;
		}

		LonLatPoint lonlatpoint = new LonLatPoint(testLon,testLat);
		PixelPoint point = gp.lonlatToPixel (lonlatpoint,17);
		controller.SimpleMove (controller.transform.InverseTransformPoint((new Vector3 (-(float)point.pointX/100f,0.8f,(float)point.pointY/100f))));

//		LonLatPoint mylonlatpoint = new LonLatPoint(Input.location.lastData.longitude,Input.location.lastData.latitude);
//		PixelPoint mypoint = gp.lonlatToPixel (mylonlatpoint,17);
//		controller.SimpleMove (controller.transform.InverseTransformPoint((new Vector3 (-(float)mypoint.pointX/100f,0.8f,(float)mypoint.pointY/100f))));
//		print ("x:"+(-(float)mypoint.pointX/100f)+"   y:"+(float)mypoint.pointY/100f);


		if (isRotateBack) {//
			Vector3 centerPoint = new Vector3(Screen.width/2,Screen.height/2,transform.position.y/Mathf.Sin(DegreetoRadians(transform.eulerAngles.x)));
			float angleBefore = transform.eulerAngles.y;
			float rotateSpeed; 
			if(angleBefore > 180){
				rotateSpeed = -50.0f*Time.deltaTime;
			}else{
				rotateSpeed = 50.0f*Time.deltaTime;
			}
			transform.RotateAround (camera.ScreenToWorldPoint(centerPoint), new Vector3(0,1,0), rotateSpeed);
			for (int i=0; i<=12; i++) {
				texts[i].transform.Rotate(Vector3.forward*rotateSpeed);
			}
			float angleAfter = transform.eulerAngles.y;
			print("angleBefore is "+angleBefore+"  angleAfter is "+angleAfter);
			if(angleBefore <= 180 && angleAfter > 180){
				isRotateBack = false;
				hasRotated = false;
//				_unityCallIOS("back");
			}
			print("transform.eulerAngles.y is "+transform.eulerAngles.y);
		}
		speed = transform.position.y/scale;
		if (Input.touchCount == 1) {
			if (Input.GetTouch (0).phase == TouchPhase.Began) {//屏幕点击事件
				touchBefore = Input.GetTouch (0).position;
				Ray ray = camera.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if(Physics.Raycast(ray,out hit))
				{
					print("click:"+hit.transform.gameObject.name);
					clickMessage = "点击:" + kav.getValueFromkey(hit.transform.gameObject.name);
				}
				currentGesture = "";
			}
			if (Input.GetTouch (0).phase == TouchPhase.Moved) {
				if(!currentGesture.Equals("")){
					return;
				}
				Vector2 touchAfter = Input.GetTouch (0).position;
				Vector2 touchDeltaPosition = touchAfter - touchBefore;
				float lengthScreen = touchDeltaPosition.magnitude;

				Vector3 touchAfterToWorld = camera.ScreenToWorldPoint(new Vector3(touchAfter.x,touchAfter.y,transform.position.y/Mathf.Sin(DegreetoRadians(transform.eulerAngles.x))));
				Vector3 touchBeforeToWorld = camera.ScreenToWorldPoint(new Vector3(touchBefore.x,touchBefore.y,transform.position.y/Mathf.Sin(DegreetoRadians(transform.eulerAngles.x))));
				Vector3 deltaWorld = touchAfterToWorld - touchBeforeToWorld;
				float lengthWorld = deltaWorld.magnitude;
				float scaleFromSceenToWorld = lengthWorld/lengthScreen;
				float y_weight = -touchDeltaPosition.y*Mathf.Sin(DegreetoRadians(transform.eulerAngles.x));
				float z_weight = -touchDeltaPosition.y*Mathf.Cos(DegreetoRadians(transform.eulerAngles.x));
				transform.Translate (-touchDeltaPosition.x * scaleFromSceenToWorld, y_weight * scaleFromSceenToWorld, z_weight*scaleFromSceenToWorld );
				touchBefore = touchAfter;
			}
		} else if (Input.touchCount == 2) {
			Touch touchZero = Input.GetTouch(0);
			Touch touchOne = Input.GetTouch(1);
			if(touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began){
				touch0before = Input.GetTouch(0).position;
				touch1before = Input.GetTouch(1).position;
				currentGesture = "begin";
				gestureList.Clear();
//				print("coor0:"+testCoorString0);
//				print("coor1:"+testCoorString1);
				testCoorString0 = "";
				testCoorString1 = "";
			}else if (touchZero.phase == TouchPhase.Moved && touchOne.phase == TouchPhase.Moved) {
				testCoorString0 += touchZero.position+"\n";
				testCoorString1 += touchOne.position+"\n";
				//判断之前和之后，两点间的距离的变化是否是巨大的
				float pinchDistance = Vector2.Distance(touchZero.position, touchOne.position);
				float prevDistance = Vector2.Distance(touch0before,touch1before);
				pinchDistanceDelta = pinchDistance - prevDistance;
				print("pinchDistanceDelta is "+pinchDistanceDelta);
				if (Mathf.Abs(pinchDistanceDelta) > minPinchDistance) {
					isDistanceChangeHuge = true;
				}else{
					isDistanceChangeHuge = false;
				}
//				Vector2 touch0before = touchZero.position - touchZero.deltaPosition;
//				Vector2 touch1before = touchOne.position - touchOne.deltaPosition;
				Vector2 vectorbefore01 = new Vector2(touch1before.x-touch0before.x,touch1before.y-touch0before.y);
				float angleZero = VectorAngle(vectorbefore01, touchZero.position - touch0before);
				Vector2 vectorbefore10 = new Vector2(touch0before.x-touch1before.x,touch0before.y-touch1before.y);
				float angleOne = VectorAngle(vectorbefore10, touchOne.position - touch1before);
				if(angleZero * angleOne > 0 && Mathf.Abs(angleZero) > 90-angleRangeOfRotate/2 && Mathf.Abs(angleZero) < 90+angleRangeOfRotate/2 && Mathf.Abs(angleOne) > 90-angleRangeOfRotate/2 && Mathf.Abs(angleOne) < 90+angleRangeOfRotate/2){
					isRotate = true;
				}else{
					isRotate = false;
				}
				if(isRotate){
					if(currentGesture.Equals("begin")){
						gestureList.Add("rotate");
						if(isContinuousSameGesture("rotate")){//连续三个rotate
							currentGesture = "rotate";
						}else{
							touch0before = touchZero.position;
							touch1before = touchOne.position;
							return;
						}
					}else{
						if(!currentGesture.Equals("rotate")){//zoom or updown
							touch0before = touchZero.position;
							touch1before = touchOne.position;
							return;
						}
					}

					print("---------rotate_camera");
					Vector2 vectorAfter01 = new Vector2(touchOne.position.x-touchZero.position.x,touchOne.position.y-touchZero.position.y);
					float rotateAngle = VectorAngle(vectorbefore01, vectorAfter01);
					Vector3 centerPoint = new Vector3((touch0before.x+touch1before.x)/2,(touch0before.y+touch1before.y)/2,transform.position.y/Mathf.Sin(DegreetoRadians(transform.eulerAngles.x)));
					transform.RotateAround (camera.ScreenToWorldPoint(centerPoint),new Vector3(0,1,0) , -rotateAngle);
					print("centerPoint is "+camera.ScreenToWorldPoint(centerPoint));
					hasRotated = true;
				
					//text rotate
//					text.transform.Rotate(Vector3.forward*rotateAngle);
					for (int i=0; i<=12; i++) {
						texts[i].transform.Rotate(Vector3.forward*(-rotateAngle));
					}
				}else{
					if(isDistanceChangeHuge){
						if(currentGesture.Equals("begin")){
							gestureList.Add("zoom");
							if(isContinuousSameGesture("zoom")){//连续三个zoom
								currentGesture = "zoom";
							}else{
								touch0before = touchZero.position;
								touch1before = touchOne.position;
								return;
							}
						}else{
							if(!currentGesture.Equals("zoom")){//rotate or updown
								touch0before = touchZero.position;
								touch1before = touchOne.position;
								return;
							}
						}
						print("------------zoom");
						Vector2 touchZeroPrevPos = touchZero.position - (touchZero.position - touch0before);
						Vector2 touchOnePrevPos = touchOne.position - (touchOne.position - touch1before);
						
						// Find the magnitude of the vector (the distance) between the touches in each frame.
						float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
						float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
						
						// Find the difference in the distances between each frame.
						float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
						print ("height:"+transform.position.y);
						if(deltaMagnitudeDiff > 0){//zoom out
							if(transform.position.y > maxHigh){
								print("too high");
								hasUpDown = false;
								//call ios native function back
								touch0before = touchZero.position;
								touch1before = touchOne.position;
								return;
							}
						}else{//zoom in
							if(transform.position.y < minHigh){
								print("too low");
								touch0before = touchZero.position;
								touch1before = touchOne.position;
								return;
							}
						}

						float h1 = transform.position.y;
						transform.Translate(-Vector3.forward*deltaMagnitudeDiff*speed);
						if(transform.position.y > maxHigh*9/10){
							supportRotate = false;
							if(h1 <= maxHigh*9/10){//转回来
								startRotateBack();
								hasUpDown = false;
							}
						}else{
							supportRotate = true;
						}
						print ("after height:"+transform.position.y);
						float h2 = transform.position.y;
						float angle1 = 90.0f-transform.eulerAngles.x;


						if(deltaMagnitudeDiff > 0){//zoom out
							if(transform.eulerAngles.x < getMaxAngleByHeight()){
								print("transform.eulerAngles.x is "+transform.eulerAngles.x+"   getMaxAngleByHeight is "+getMaxAngleByHeight());
								Vector3 cameraLeftWorldVector = transform.TransformDirection (Vector3.left);
								float anglecorret = getMaxAngleByHeight()-transform.eulerAngles.x;
								transform.RotateAround (transform.position, cameraLeftWorldVector,-anglecorret);
								float dis1 = Mathf.Tan(DegreetoRadians(angle1))*h2;
								float dis2 = Mathf.Tan(DegreetoRadians(angle1-anglecorret))*h2;
								float dis = dis1 - dis2;
								float y_weight = dis*Mathf.Sin(DegreetoRadians(transform.eulerAngles.x));
								float z_weight = dis*Mathf.Cos(DegreetoRadians(transform.eulerAngles.x));
								transform.Translate (0, y_weight, z_weight );
							}
						}else{//zoom in
							if(!hasUpDown){
								Vector3 cameraLeftWorldVector = transform.TransformDirection (Vector3.left);
								float anglecorret = transform.eulerAngles.x - getMaxAngleByHeight();
								print ("transform.eulerAngles.x is "+transform.eulerAngles.x);
								print ("getMaxAngleByHeight() is "+getMaxAngleByHeight());
								print ("anglecorret is "+anglecorret);
								transform.RotateAround (transform.position, cameraLeftWorldVector,anglecorret);
								float dis1 = Mathf.Tan(DegreetoRadians(angle1))*h2;
								float dis2 = Mathf.Tan(DegreetoRadians(angle1+anglecorret))*h2;
								float dis = dis2 - dis1;
								float y_weight = dis*Mathf.Sin(DegreetoRadians(transform.eulerAngles.x));
								float z_weight = dis*Mathf.Cos(DegreetoRadians(transform.eulerAngles.x));
								transform.Translate (0, -y_weight, -z_weight );
							}
						}
					}else{
						if(currentGesture.Equals("begin")){
							gestureList.Add("updown");
							if(isContinuousSameGesture("updown")){//连续三个updown
								currentGesture = "updown";
							}else{
								touch0before = touchZero.position;
								touch1before = touchOne.position;
								return;
							}
						}else{
							if(!currentGesture.Equals("updown")){//zoom or rotate
								touch0before = touchZero.position;
								touch1before = touchOne.position;
								return;
							}
						}
						print("look up and down");
						float angle = (touchZero.position - touch0before).y*90.0f / Screen.height;
						if(angle > 0){//look up
							if(transform.eulerAngles.x < getMaxAngleByHeight()){
								print("can not look up anymore");
								touch0before = touchZero.position;
								touch1before = touchOne.position;
								return;
							}
						}else{
							if(transform.eulerAngles.x > maxAngle){
								print("can not look down anymore");
								touch0before = touchZero.position;
								touch1before = touchOne.position;
								return;
							}
						}
						if(transform.eulerAngles.x - angle > maxAngle){//防止倒过来看
							angle = transform.eulerAngles.x - maxAngle;
						}
						Vector3 cameraLeftWorldVector = transform.TransformDirection (Vector3.left);
						Vector3 centerPoint = new Vector3(Screen.width/2,Screen.height/2,transform.position.y/Mathf.Sin(DegreetoRadians(transform.eulerAngles.x)));
						transform.RotateAround (camera.ScreenToWorldPoint(centerPoint), cameraLeftWorldVector, angle);
						hasUpDown = true;
					}
				}
				touch0before = touchZero.position;
				touch1before = touchOne.position;
			}
		}
	}
	float VectorAngle(Vector2 from, Vector2 to)
	{
		float angle;
		Vector3 cross=Vector3.Cross(from, to);
		angle = Vector2.Angle(from, to);
		return cross.z > 0 ? -angle : angle;
	}
	float getMaxAngleByHeight(){
		float height = transform.position.y;
		if (height <= minHigh*5) {
			return minAngle;
		} else if (height < maxHigh) {
			return minAngle + (maxAngle-minAngle)/(maxHigh-minHigh*5)*(height-minHigh*5);
		} else {
			return maxAngle;
		}
	}

	void startRotateBack(){
		if(hasRotated){
			isRotateBack = true;
		}
	}
	float DegreetoRadians(float x)
	{
		return x * 0.017453292519943295769f;
	}
	public void iosCallUnity(string message){
		print ("iosCallUnity-------"+message);
		transform.position = new Vector3 (0, minHigh*5, 0);
		transform.eulerAngles = new Vector3 (90,180,0); 
	}
	IEnumerator StartGPS () {
		// Input.location 用于访问设备的位置属性（手持设备）, 静态的LocationService位置
		// First, check if user has location service enabled
		if (!Input.location.isEnabledByUser)
			yield break;
		
		// Start service before querying location
		Input.location.Start();
		
		// Wait until service initializes
		int maxWait = 20;
		while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
		{
			yield return new WaitForSeconds(1);
			maxWait--;
		}
		
		// Service didn't initialize in 20 seconds
		if (maxWait < 1)
		{
			print("Timed out");
			yield break;
		}
		
		// Connection has failed
		if (Input.location.status == LocationServiceStatus.Failed)
		{
			print("Unable to determine device location");
			yield break;
		}
		else
		{
			// Access granted and location value could be retrieved
			print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
		}
		
		// Stop service if there is no need to query location updates continuously
		//		Input.location.Stop();
	}
	void OnGUI () {  
		GUI.skin.label.fontSize = 35;  
		GUI.skin.button.fontSize = 40;
		GUI.color = Color.black;
		GUI.Label (new Rect (20, 20, 1000, 80), this.gps_info);
		GUI.Label (new Rect (20, 100, 1000, 80), this.testBallLoc);
		GUI.Label (new Rect (20, 180, 1000, 80), clickMessage);
		if (GUI.Button (new Rect (20, 260, 200, 100), "增加经度")) {
			testLon += 0.0005f;
		}
		if (GUI.Button (new Rect (240, 260, 200, 100), "减小经度")) {
			testLon -= 0.0005f;
		}
		if (GUI.Button (new Rect (20, 380, 200, 100), "增加纬度")) {
			testLat += 0.0005f;
		}
		if (GUI.Button (new Rect (240, 380, 200, 100), "减小纬度")) {
			testLat -= 0.0005f;
		}
		if (GUI.Button (new Rect (20, 500, 200, 100), "小球跟随")) {
			if(Input.location.lastData.longitude < 1){
				clickMessage = "还没定上位";
			}else{
				baseLon = Input.location.lastData.longitude;
				baseLat = Input.location.lastData.latitude;
				clickMessage = "小球跟随";
			}

		}
		if (GUI.Button (new Rect (240, 500, 200, 100), "取消跟随")) {
			baseLon = 0;
			baseLat = 0;
			clickMessage = "取消跟随";
		}
		if (GUI.Button (new Rect (20, 620, 200, 100), "back")) {
//			_unityCallIOS("back");
		}
	}
	bool isContinuousSameGesture(string ges){
		int count = gestureList.Count;
		if (count < 3) {
			return false;
		} else {
			if(gestureList[count-1].Equals(ges)&&gestureList[count-2].Equals(ges)&&gestureList[count-3].Equals(ges)){
				return true;
			}else{
				return false;
			}
		}
	}
//	[DllImport ("__Internal")]
//	private static extern void _unityCallIOS (string message);

	
}

