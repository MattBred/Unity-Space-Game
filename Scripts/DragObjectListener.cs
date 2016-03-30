using UnityEngine;
using System.Collections;

public class DragObjectListener : MonoBehaviour {
	// Update is called once per frame
	void Update () {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            
            float x = transform.rotation.eulerAngles.x;
            float y = transform.rotation.eulerAngles.y - 90;
            float z = transform.rotation.eulerAngles.z;
            transform.Rotate(0, -45, 0);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {

            float x = transform.rotation.eulerAngles.x;
            float y = transform.rotation.eulerAngles.y - 90;
            float z = transform.rotation.eulerAngles.z;
            transform.Rotate(0, 45, 0);
        }
    }
}
