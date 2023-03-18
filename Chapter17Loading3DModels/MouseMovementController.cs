
namespace Chapter17Loading3DModels;

public struct MouseState
{
    public Vector2 Pos2d;
    public Vector3 Pos3d;
    public float Wheel;
    public MouseControlState ControlState;
    public string Debug;

}

public class MouseMovementController
{
    private readonly OrthographicCamera camera = null!;
    public Vector2D<int> screen;

    private Vector2 mouse2d = Vector2.Zero;
    private Vector3 mouse3d = Vector3.Zero;
    private float mouseWheel = 0f;


    private MouseButtonState mouseLeft = MouseButtonState.None;
    private MouseButtonState mouseMiddle = MouseButtonState.None;
    private MouseButtonState mouseRight = MouseButtonState.None;

    private MouseControlState mouseState = MouseControlState.None;

    private string mouseString = "none";

    public MouseControlState MouseState { get => mouseState; set => mouseState = value; }



    private MouseState previous;

    public Action<MouseState> OnMouseStateChanged = null!;

    public void SetPrevious(MouseState previous)
    {
        this.previous = previous;
    }

    public void UpdateMouseState()
    {

        mouseState = (l: mouseLeft, m: mouseMiddle, r: mouseRight, w: mouseWheel) switch
        {
            var s when s.l != MouseButtonState.Down && s.m == MouseButtonState.Down && s.r == MouseButtonState.Down => MouseControlState.Rotate,
            var s when s.l != MouseButtonState.Down && s.m == MouseButtonState.Down && s.r != MouseButtonState.Down && previous.ControlState == MouseControlState.Rotate => MouseControlState.ZoomMouse,
            var s when s.l != MouseButtonState.Down && s.m == MouseButtonState.Down && s.r != MouseButtonState.Down && previous.ControlState != MouseControlState.Rotate => MouseControlState.Pan,
            var s when s.l == MouseButtonState.Down && s.m != MouseButtonState.Down && s.r != MouseButtonState.Down => MouseControlState.Pick,
            var s when s.l != MouseButtonState.Down && s.m != MouseButtonState.Down && s.r == MouseButtonState.Down => MouseControlState.Context,
            var s when s.l != MouseButtonState.Down && s.m != MouseButtonState.Down && s.r != MouseButtonState.Down && MathF.Abs(s.w) > .01 => MouseControlState.ZoomWheel,
            _ => MouseControlState.None
        };

        var ml = mouseLeft == MouseButtonState.Down ? "LX" : "L_";
        var mm = mouseMiddle == MouseButtonState.Down ? "MX" : "M_";
        var mr = mouseRight == MouseButtonState.Down ? "RX" : "R_";
        mouseString = $"{mouseState,-16} [{ml} {mm} {mr}], 2D=[{mouse2d.X:+0.0000;-0.0000},{mouse2d.Y:+0.0000;-0.0000}], 3D=[{mouse3d.X:+0.0000;-0.0000},{mouse3d.Y:+0.0000;-0.0000},{mouse3d.Z:+0.0000;-0.0000}]";
        
        var current = new MouseState()
        {
            Pos2d = mouse2d,
            Pos3d = mouse3d,
            Wheel = mouseWheel,
            ControlState = mouseState,
            Debug = mouseString
        };

        mouseWheel = 0;

        OnMouseStateChanged?.Invoke(current);
    }


    public MouseMovementController(OrthographicCamera camera, IWindow window)
    {
        this.camera = camera;

        var input = window.CreateInput();
        input.ConnectionChanged += DoConnect;

        Resize(window.FramebufferSize);

        foreach (var mouse in input.Mice)
        {
            if (!mouse.IsConnected) continue;
            DoConnect(mouse, mouse.IsConnected);
        }
    }

    public void Resize(Vector2D<int> frameBufferSize)
    {
        screen = frameBufferSize;

        camera.Resize(0, 0, (uint)screen.X, (uint)screen.Y);

    }


    public unsafe void DoConnect(IInputDevice device, bool isConnected)
    {
        if (device is IMouse mouse)
        {
            //Console.WriteLine($"Discovered mouse {mouse.Index} (Connected: {isConnected})");
            if (isConnected)
            {
                mouse.MouseUp += MouseOnMouseUp;
                mouse.MouseDown += MouseOnMouseDown;
                mouse.Click += MouseOnClick;
                mouse.DoubleClick += MouseOnDoubleClick;
                mouse.Scroll += MouseOnScroll;
                mouse.MouseMove += MouseOnMouseMove;
            }
            else
            {
                mouse.MouseUp -= MouseOnMouseUp;
                mouse.MouseDown -= MouseOnMouseDown;
                mouse.Click -= MouseOnClick;
                mouse.DoubleClick -= MouseOnDoubleClick;
                mouse.Scroll -= MouseOnScroll;
                mouse.MouseMove -= MouseOnMouseMove;
            }

        }
    }


    private void MouseOnMouseMove(IMouse arg1, Vector2 arg2)
    {
        //Console.WriteLine($"M{arg1.Index}> Moved: {arg2}");
        float x = 2.0f * (float)arg2.X / (float)screen.X - 1f;
        float y = 2.0f * (float)arg2.Y / (float)screen.Y - 1f;

        mouse2d = new(x, y);
        mouse3d = camera.UnProject(mouse2d);
        UpdateMouseState();
    }

    private void MouseOnScroll(IMouse arg1, ScrollWheel arg2)
    {
        mouseWheel = (float)-arg2.Y;
        UpdateMouseState();
    }

    private void MouseOnMouseDown(IMouse arg1, MouseButton arg2)
    {
        //Console.WriteLine($"M{arg1.Index}> {arg2} down.");
        switch (arg2)
        {
            case MouseButton.Left:
                mouseLeft = MouseButtonState.Down;
                break;
            case MouseButton.Middle:
                mouseMiddle = MouseButtonState.Down;
                break;
            case MouseButton.Right:
                mouseRight = MouseButtonState.Down;
                break;
        }
        UpdateMouseState();

    }

    private void MouseOnMouseUp(IMouse arg1, MouseButton arg2)
    {

        //Console.WriteLine($"M{arg1.Index}> {arg2} up.");
        switch (arg2)
        {
            case MouseButton.Left:
                mouseLeft = MouseButtonState.Up;
                break;
            case MouseButton.Middle:
                mouseMiddle = MouseButtonState.Up;
                break;
            case MouseButton.Right:
                mouseRight = MouseButtonState.Up;
                break;
        }

        UpdateMouseState();
    }

    private void MouseOnClick(IMouse arg1, MouseButton arg2, Vector2 pos)
    {
        //Console.WriteLine($"M{arg1.Index}> {arg2} single click.");
    }

    private void MouseOnDoubleClick(IMouse arg1, MouseButton arg2, Vector2 pos)
    {
        Console.WriteLine($"M{arg1.Index}> {arg2} double click.");
    }



}

public enum MouseButtonState
{
    None,
    Up,
    Down,
    Down2
}
public enum MouseControlState
{
    None,
    Pan,
    ZoomMouse,
    ZoomWheel,
    Rotate,
    Context,
    Pick
}