
namespace Chapter18DiffuseShading;



public class KeyboardController
{
    //private double dt;
    //private LveGameObject gameObject = null!;
    //private readonly IInputContext input = null!;
    private readonly KeyMappings keys = new();

    private float moveSpeed = 3f;
    private float lookSpeed = 1.5f;
    //private Vector3 rotate = Vector3.Zero;

    //private Key[] _allKeys = Enum.GetValues(typeof(Key)).Cast<Key>().Where(x => x != Key.Unknown).ToArray();
    private readonly List<Key> keysDown = new();

    public void MoveInPlaceXZ(double dt, ref LveGameObject gameObject)
    {
        var rotate = Vector3.Zero;
        if (keysDown.Contains(keys.LookRight)) rotate += Vector3.UnitY;
        if (keysDown.Contains(keys.LookLeft)) rotate -= Vector3.UnitY;
        if (keysDown.Contains(keys.LookUp)) rotate += Vector3.UnitX;
        if (keysDown.Contains(keys.LookDown)) rotate -= Vector3.UnitX;

        if (Vector3.Dot(rotate, rotate) > float.Epsilon)
        {
            gameObject.Transform.Rotation += lookSpeed * (float)dt * Vector3.Normalize(rotate);
        }

        gameObject.Transform.Rotation.X = Math.Clamp(gameObject.Transform.Rotation.X, -1.5f, 1.5f);
        gameObject.Transform.Rotation.Y = gameObject.Transform.Rotation.Y % MathF.Tau;


        float yaw = gameObject.Transform.Rotation.Y;
        Vector3 forwardDir = new(MathF.Sin(yaw), 0f, MathF.Cos(yaw));
        Vector3 rightDir = new(forwardDir.Z, 0f, -forwardDir.Z);
        Vector3 upDir = -Vector3.UnitY;

        Vector3 moveDir = Vector3.Zero;
        if (keysDown.Contains(keys.MoveForward)) moveDir += forwardDir;
        if (keysDown.Contains(keys.MoveBackward)) moveDir -= forwardDir;
        if (keysDown.Contains(keys.MoveRight)) moveDir += rightDir;
        if (keysDown.Contains(keys.MoveLeft)) moveDir -= rightDir;
        if (keysDown.Contains(keys.MoveUp)) moveDir += upDir;
        if (keysDown.Contains(keys.MoveDown)) moveDir -= upDir;

        if (Vector3.Dot(moveDir, moveDir) > float.Epsilon)
        {
            gameObject.Transform.Translation += moveSpeed * (float)dt * Vector3.Normalize(moveDir);
        }

    }

    public KeyboardController(IWindow window)
    {
        //this.dt = dt;
        //this.gameObject = gameObject;
        var input = window.CreateInput();
        input.ConnectionChanged += DoConnect;

        foreach (var keyboard in input.Keyboards)
        {
            if (!keyboard.IsConnected) continue;
            DoConnect(keyboard, keyboard.IsConnected);
        }

        foreach (var mouse in input.Mice)
        {
            if (!mouse.IsConnected) continue;
            DoConnect(mouse, mouse.IsConnected);
        }
    }


    public unsafe void DoConnect(IInputDevice device, bool isConnected)
    {
        //Console.WriteLine("bong");
        //Console.WriteLine(isConnected
        //    ? $"{device.GetType().Name} {device.Name} connected"
        //    : $"{device.GetType().Name} {device.Name} disconnected");
        if (device is IKeyboard keyboard)
        {
            //Console.WriteLine($"Discovered keyboard {keyboard.Index} (Connected: {isConnected})");
            if (isConnected)
            {
                keyboard.KeyDown += KeyboardOnKeyDown;
                keyboard.KeyUp += KeyboardOnKeyUp;
                keyboard.KeyChar += KeyboardOnKeyChar;
            }
            else
            {
                keyboard.KeyDown -= KeyboardOnKeyDown;
                keyboard.KeyUp -= KeyboardOnKeyUp;
                keyboard.KeyChar -= KeyboardOnKeyChar;
            }

            //Console.Write("    Buttons: ");
            //Console.WriteLine(string.Join(", ", keyboard.SupportedKeys.Select(x => x)));
        }
        else if (device is IMouse mouse)
        {
            //Console.WriteLine($"Discovered mouse {mouse.Index} (Connected: {isConnected})");
            if (isConnected)
            {
                //mice.Add(mouse);
                mouse.MouseUp += MouseOnMouseUp;
                mouse.MouseDown += MouseOnMouseDown;
                mouse.Click += MouseOnClick;
                mouse.DoubleClick += MouseOnDoubleClick;
                mouse.Scroll += MouseOnScroll;
                mouse.MouseMove += MouseOnMouseMove;
            }
            else
            {
                //mice.Remove(mouse);
                mouse.MouseUp -= MouseOnMouseUp;
                mouse.MouseDown -= MouseOnMouseDown;
                mouse.Click -= MouseOnClick;
                mouse.DoubleClick -= MouseOnDoubleClick;
                mouse.Scroll -= MouseOnScroll;
                mouse.MouseMove -= MouseOnMouseMove;
            }

            // mouse.Cursor.IsConfined = true;

            //Console.Write("    Buttons: ");
            //Console.WriteLine(string.Join(", ", mouse.SupportedButtons.Select(x => x)));
            //Console.WriteLine($"    {mouse.ScrollWheels.Count} scroll wheels.");
        }
    }

    private void KeyboardOnKeyChar(IKeyboard arg1, char arg2)
    {
        //Console.WriteLine($"K{arg1.Index}> {arg2} received.");
    }

    private void MouseOnMouseMove(IMouse arg1, Vector2 arg2)
    {

        //Console.WriteLine($"M{arg1.Index}> Moved: {arg2}");
    }

    private void MouseOnScroll(IMouse arg1, ScrollWheel arg2)
    {

        //Console.WriteLine($"K{arg1.Index}> Scrolled: ({arg2.X}, {arg2.Y})");
    }

    private void MouseOnMouseDown(IMouse arg1, MouseButton arg2)
    {
        Console.WriteLine($"M{arg1.Index}> {arg2} down.");
    }

    private void MouseOnMouseUp(IMouse arg1, MouseButton arg2)
    {

        Console.WriteLine($"M{arg1.Index}> {arg2} up.");
    }

    private void MouseOnClick(IMouse arg1, MouseButton arg2, Vector2 pos)
    {
        //Console.WriteLine($"M{arg1.Index}> {arg2} single click.");
    }

    private void MouseOnDoubleClick(IMouse arg1, MouseButton arg2, Vector2 pos)
    {
        Console.WriteLine($"M{arg1.Index}> {arg2} double click.");
    }

    private void KeyboardOnKeyUp(IKeyboard arg1, Key arg2, int _)
    {
        //Console.WriteLine($"K{arg1.Index}> {arg2} up.");
        keysDown.RemoveAt(keysDown.IndexOf(arg2));
    }

    private void KeyboardOnKeyDown(IKeyboard arg1, Key arg2, int _)
    {
        //Console.WriteLine($"K{arg1.Index}> {arg2} down.");
        keysDown.Add(arg2);

    }

    struct KeyMappings
    {
        public Key MoveLeft;
        public Key MoveRight;
        public Key MoveForward;
        public Key MoveBackward;
        public Key MoveUp;
        public Key MoveDown;
        public Key LookLeft;
        public Key LookRight;
        public Key LookUp;
        public Key LookDown;

        public KeyMappings()
        {
            MoveLeft = Key.A;
            MoveRight = Key.D;
            MoveForward = Key.W;
            MoveBackward = Key.S;
            MoveUp = Key.E;
            MoveDown = Key.Q;
            LookLeft = Key.Left;
            LookRight = Key.Right;
            LookUp = Key.Up;
            LookDown = Key.Down;
        }
    };
}
