
namespace Sandbox03MeshShaders;

public class KeyboardController
{
    private readonly KeyMappings keys = new();

    //private Key[] _allKeys = Enum.GetValues(typeof(Key)).Cast<Key>().Where(x => x != Key.Unknown).ToArray();
    private readonly List<Key> keysDown = new();

    public Action<Key> OnKeyPressed = null!;

    public KeyboardController(IWindow window)
    {
        var input = window.CreateInput();
        input.ConnectionChanged += DoConnect;

        foreach (var keyboard in input.Keyboards)
        {
            if (!keyboard.IsConnected) continue;
            DoConnect(keyboard, keyboard.IsConnected);
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
    }

    private void KeyboardOnKeyChar(IKeyboard arg1, char arg2)
    {
        //Console.WriteLine($"K{arg1.Index}> {arg2} received.");
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
        OnKeyPressed?.Invoke(arg2);
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
