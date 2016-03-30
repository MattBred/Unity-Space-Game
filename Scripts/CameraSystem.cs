using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    private static CameraSystem cameraSystem;

    // The target we are following
    [SerializeField]
    private Transform target;

    private static Vector3 nullTargetVector = new Vector3();

    // The distance in the x-z plane to the target
    [SerializeField]
	private float distance = 10.0f;
    private float rotation = 0;
	// the height we want the camera to be above the target
	[SerializeField]
	private float height = 10.0f;

	[SerializeField]
	private float rotationDamping;
	[SerializeField]
	private float heightDamping;
    private static Vector3 nullVector = new Vector3(0, 0, 0);

    private float radius = 10f;
    private float cameraAngle;

    bool lookAt = true;

    // Use this for initialization
    void Start() {
        cameraAngle = transform.rotation.eulerAngles.x;
        if (name.Equals("MainCamera")) cameraSystem = this;
        if (target)
        {
            setTarget(target);
        }
    }

	// Update is called once per frame
	void Update()
	{
		// Early out if we don't have a target
		if (!name.Equals("MainCamera"))
			return;

        // Calculate the current rotation angles
        var wantedRotationAngle = rotation;
		var wantedHeight = target.position.y + height;



		var currentRotationAngle = transform.eulerAngles.y;
		var currentHeight = transform.position.y;
        var currentPosition = transform.position;
        bool lerp = false;


        // Damp the rotation around the y-axis
        if (lerp) currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, Time.deltaTime * 5);
        else currentRotationAngle = wantedRotationAngle;

        // Damp the height
        if (lerp) currentHeight = Mathf.Lerp(currentHeight, wantedHeight, Time.deltaTime * 5);
        else currentHeight = wantedHeight;

		// Convert the angle into a rotation
		var currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        var wantedPosition = target.position - currentRotation * Vector3.forward * distance;
        if (lerp) currentPosition = Vector3.Lerp(currentPosition, wantedPosition, Time.deltaTime * 5);
        else currentPosition = wantedPosition;

        // Set the position of the camera on the x-z plane to distance meters behind the target
        transform.position = currentPosition;

        // Set the height of the camera
        transform.position = new Vector3(transform.position.x ,currentHeight , transform.position.z);


        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            float input = Input.GetAxis("Mouse ScrollWheel");

            if (viewportContainsMousePos() && (input < 0 || (input > 0 && radius > 2)))
            {
                radius += input*-2*radius;
                recalculateHeightDistance();
            }
        }
        if (Input.GetMouseButtonDown(2))
        {
            if (!target)
            {
                //target.position = getMouseWorldPos();
                //setTarget(target.position);
            }
        }
        if (Input.GetMouseButton(2) && viewportContainsMousePos())
        {
            Transform nullTarget = GameObject.Find("MainCameraTargetNull").transform;
            if (target != nullTarget)
            {
                nullTarget.position = getCenterScreenWorldPos();
                setTarget(nullTarget.transform);
                //transform.LookAt(nullTarget);
            }
            float inputValX = Input.GetAxis("Mouse X");
            float inputValY = Input.GetAxis("Mouse Y");
            nullTarget.position += transform.TransformDirection(Vector3.left)*inputValX;
            nullTarget.position -= transform.TransformVector(Vector3.up) * inputValY;
            lookAt = false;
            lerp = false;            
        } else
        {
            lookAt = true;
            lerp = true;
        }
        if (Input.GetMouseButton(1) && viewportContainsMousePos())
        {
            //Moving mouse around while holding middle button
            if (Input.GetAxis("Mouse Y") != 0)
            {
                    float newCameraAngle = cameraAngle + Input.GetAxis("Mouse Y") * 10;
                    if ((newCameraAngle > 5 && Input.GetAxis("Mouse Y") < 0) || (newCameraAngle < 175 && Input.GetAxis("Mouse Y") > 0)) {
                    cameraAngle = newCameraAngle;
                    //Debug.Log("Camera Angle: " + cameraAngle);
                    recalculateHeightDistance();
                }
            }
            if (Input.GetAxis("Mouse X") != 0)
            {
                //transform.Translate(Vector3.right * Input.GetAxis("Mouse X"));
                rotation += Input.GetAxis("Mouse X")*10;
            }

        }
        // Always look at the target
        if (lookAt) transform.LookAt(target.position);
	}

    public static CameraSystem getInstance()
    {
        return cameraSystem;
    }

    public void setTarget(Transform obj)
    {
        target = obj;
        target.position = obj.position;
        Vector2 target2d = new Vector2(target.position.x, target.position.z);
        Vector2 camera2d = new Vector2(transform.position.x, transform.position.z);
        float dist = Vector2.Distance(target2d, camera2d);
        cameraAngle = Mathf.Atan2(dist, transform.position.y - target.position.y) * Mathf.Rad2Deg;
        //Debug.Log("Dist: " + dist + " Height: " + (transform.position.y - obj.position.y) + " Angle: " + cameraAngle);
        radius = Vector3.Distance(target.position, transform.position);
        recalculateHeightDistance();
    }

    private void recalculateHeightDistance()
    {
        //Debug.Log("Camera Angle: " + cameraAngle);
        float cameraRad = cameraAngle * Mathf.Deg2Rad;
        float dirRad = transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

        Vector3 pos = target.position + (Get3dPosition(cameraRad, dirRad, radius));
        Vector2 v1 = new Vector2(target.position.x, target.position.z);
        Vector2 v2 = new Vector2(pos.x, pos.z);

        distance = Vector2.Distance(v1, v2);
        height = pos.y;
    }

    private Vector3 Get3dPosition(float rad1, float rad2, float radius)
    {
        return new Vector3(radius * Mathf.Sin(rad1) * Mathf.Cos(rad2),  radius * Mathf.Cos(rad1), radius * Mathf.Sin(rad1) * Mathf.Sin(rad2));
    }

    Vector3 Get2dPosition(float rad, float dist)
    {
        return new Vector3(Mathf.Sin(rad) * dist, 0, Mathf.Cos(rad) * dist * 1.025f);
    }

    public static Vector3 getCenterScreenWorldPos()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit.point;
        }
        else {
            float distance;
            Plane xy = new Plane(Vector3.down, nullVector);
            xy.Raycast(ray, out distance);
            return ray.GetPoint(distance);
        }
    }

    public static Vector3 getMouseWorldPos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit.point;
        }
        else {
            float distance;
            Plane xy = new Plane(Vector3.down, nullVector);
            xy.Raycast(ray, out distance);
            return ray.GetPoint(distance);
        }
    }

    public bool viewportContainsMousePos()
    {
        //Debug.Log("Ship Cam Mouse Viewport: " + Camera.main.ScreenToViewportPoint(Input.mousePosition));
        //Debug.Log("Elec Cam Mouse Viewport: " + GameObject.Find("ElectricalPanelCamera").GetComponent<Camera>().ScreenToViewportPoint(Input.mousePosition));
        Camera cam = gameObject.GetComponent<Camera>();
        Vector2 screenViewport = cam.ScreenToViewportPoint(Input.mousePosition);
        if (screenViewport.x >= 0 && screenViewport.x <= 1 && screenViewport.y >= 0 && screenViewport.y <= 1) return true;
        return false;
    }
}