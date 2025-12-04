using UnityEngine;

public class UIDirectionControl : MonoBehaviour {
    
    public Transform LocalDirection;
    
	// Use this for initialization
	void Start () {
        Camera currentCam = null;
        currentCam = Camera.main;

        if(currentCam != null)
            LocalDirection = currentCam.transform;

        RectTransform[] textlist = GetComponentsInChildren<RectTransform>();
        for(int i = 0; i < textlist.Length; i++) {
            textlist[i].Rotate(Vector3.up, 180);
        }
    }
	
	// Update is called once per frame
	void Update () {
        this.transform.LookAt(LocalDirection);
    }
}
