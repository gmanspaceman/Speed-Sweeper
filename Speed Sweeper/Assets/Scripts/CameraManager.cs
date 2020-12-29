using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // Start is called before the first frame update
    public Camera PerspectiveCam;
    public Camera OrthographicCam;

    public void SetOrthographicCam(bool b)
    {
        OrthographicCam.gameObject.SetActive(b);
        OrthographicCam.gameObject.GetComponent<AudioListener>().enabled = b;

        PerspectiveCam.gameObject.SetActive(!b);
        PerspectiveCam.gameObject.GetComponent<AudioListener>().enabled = !b;
    }

}
