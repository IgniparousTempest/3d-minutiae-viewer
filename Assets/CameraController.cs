using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Vector3 _target = Vector3.zero;
    [SerializeField]
    private float
        _distance = 10f,
        _xSpeed = 250f,
        _ySpeed = 120f,
        _zoomSpeed = 400f,
        _yMinLimit = -20f,
        _yMaxLimit = 80f;

    private float _x, _y;

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f)
            angle += 360f;
        if (angle > 360f)
            angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }

    private void Start()
    {
        var eulerAngles = transform.eulerAngles;
        _x = eulerAngles.x; //TODO fix maybe
        _y = eulerAngles.y;

        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;
    }

    private void LateUpdate()
    {
        var axis = Input.GetAxis("Mouse X");
        var axis2 = Input.GetAxis("Mouse Y");
        if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0.0f && Input.mousePosition.x >= 0 && Input.mousePosition.x < Screen.width && Input.mousePosition.y >= 0 && Input.mousePosition.y < Screen.height) // Scroll
        {
            var num = Input.GetAxis("Mouse ScrollWheel");
            num = -num * _zoomSpeed * 0.03f;
            _distance += num * (Mathf.Max(_distance, 0.02f) * 0.03f);
        }
        else if (Input.GetMouseButton(1)) // Right Mouse
        {
            _x += axis * _xSpeed * 0.03f;
            _y += -axis2 * _ySpeed * 0.03f;
            _y = ClampAngle(_y, _yMinLimit, _yMaxLimit);
        }
        else if (Input.GetMouseButton(2)) // Middle Mouse
        {
            var a = transform.rotation * Vector3.right;
            var a2 = transform.rotation * Vector3.up;
            var a3 = -a * axis * _xSpeed * 0.02f;
            var b = -a2 * axis2 * _ySpeed * 0.02f;
            _target += (a3 + b) * (Mathf.Max(_distance, 0.04f) * 0.01f);
        }

        var rotation = Quaternion.Euler(_y, _x, 0f);
        var position = rotation * new Vector3(0f, 0f, -_distance) + _target;
        transform.rotation = rotation;
        transform.position = position;
    }

    public void SetTarget(Vector3 target)
    {
        _target = target;
    }
}