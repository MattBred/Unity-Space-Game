using UnityEngine;
using System.Collections;

public class ElectricalPanelCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
        if (Input.GetMouseButton(2))
        {
            transform.position += transform.up*Input.GetAxis("Mouse Y")*-5;
            transform.position += transform.right * Input.GetAxis("Mouse X")*-5;
        }
        
        if ((Input.GetAxis("Mouse ScrollWheel") > 0 && transform.position.z <= -30 || Input.GetAxis("Mouse ScrollWheel") < 0 && transform.position.z > -200) && viewportContainsMousePos())
        {
            transform.position += transform.forward * Input.GetAxis("Mouse ScrollWheel") * 100;
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
