using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameController GameController;

    Rect StillZone;

    private void Start()
    {
        StillZone = new Rect(Screen.width * 0.25f,
            Screen.height * 0.25f,
            Screen.width * 0.5f,
            Screen.height * 0.5f
        );
    }

    private void Update()
    {
        var mouse = Input.mousePosition;
        var adjustment = Vector3.zero;

        var ScreenBottomLeft = Camera.main.ScreenToWorldPoint(Vector3.zero);
        var ScreenTopRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        var wheel = Input.GetAxis("Mouse ScrollWheel");
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - wheel, 3f, 10f);

        if (ScreenBottomLeft.x < GameController.WorldBottomLeft.x && !(ScreenTopRight.x > GameController.WorldTopRight.x))
            transform.position -= new Vector3(ScreenBottomLeft.x - GameController.WorldBottomLeft.x, 0, 0);

        else if (ScreenTopRight.x > GameController.WorldTopRight.x && !(ScreenBottomLeft.x < GameController.WorldBottomLeft.x))
            transform.position += new Vector3(GameController.WorldTopRight.x - ScreenTopRight.x, 0, 0);

        if (ScreenBottomLeft.y < GameController.WorldBottomLeft.y && !(ScreenTopRight.y > GameController.WorldTopRight.y))
            transform.position -= new Vector3(0, ScreenBottomLeft.y - GameController.WorldBottomLeft.y, 0);

        else if (ScreenTopRight.y > GameController.WorldTopRight.y && !(ScreenBottomLeft.y < GameController.WorldBottomLeft.y))
            transform.position += new Vector3(0, GameController.WorldTopRight.y - ScreenTopRight.y, 0);

        //Get the percentage of the mouse outside the still zone and lerp the camera based on that amount.
        //IE mouse all the way to the left of screen or all the way right of screen = 1
        //just outside the zone would be close to 0.
        if (mouse.x < StillZone.x && ScreenBottomLeft.x > GameController.WorldBottomLeft.x)
            adjustment.x = (StillZone.x - mouse.x) / StillZone.x * -1;
        else if (mouse.x > StillZone.xMax && ScreenTopRight.x < GameController.WorldTopRight.x)
            adjustment.x = (mouse.x - StillZone.xMax) / StillZone.x;

        if(mouse.y < StillZone.y && ScreenBottomLeft.y > GameController.WorldBottomLeft.y)
            adjustment.y = (StillZone.y - mouse.y) / StillZone.y * -1;
        else if(mouse.y > StillZone.yMax && ScreenTopRight.y < GameController.WorldTopRight.y)
            adjustment.y = (mouse.y - StillZone.yMax) / StillZone.y;

        var target = transform.position + adjustment;

        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime);
    }
}