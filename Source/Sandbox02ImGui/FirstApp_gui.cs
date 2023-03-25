
namespace Sandbox02ImGui;

public partial class FirstApp
{
    private Vector2 wPos;
    private Vector2 wSize;
    private Vector2 toolbarPos;
    private Vector2 toolbarSize;
    private float xc;
    private float yc;
    private Vector2 tl;
    private Vector2 bl;
    private Vector2 tr;
    private Vector2 br;
    private Vector2 cPos;
    private Vector2 buttonSize = new(80, 24);

    // footer props
    private string status = "";
    private long lastSample = 0;
    private long interval = 10_000 * 1000; // 250 ms
    private float memory;
    private Vector2 footerPos;
    private Vector2 footerSize;

    // pointLight props
    private float rotateSpeed = 1f;
    private float yPosition = 0f;
    private float lightIntensity = 0f;
    private float lightRadius = 0f;

    private void FirstAppGuiInit()
    {
        rotateSpeed = pointLightRenderSystem.RotateSpeed;
        var yCheck = gameObjects.Values.FirstOrDefault(s => s.PointLight.HasValue);
        if (yCheck is not null)
        {
            yPosition = yCheck.Transform.Translation.Y;
            lightIntensity = yCheck.PointLight?.LightIntensity ?? 0f;
            lightRadius = yCheck.Transform.Scale.X;
        }
    }

    private void FirstAppGuiUpdate()
    {
        ImGui.SetNextWindowPos(toolbarPos);
        ImGui.SetNextWindowSize(toolbarSize);
        ImGui.Begin("MainToolbar",
            ImGuiWindowFlags.NoBackground
            | ImGuiWindowFlags.NoTitleBar
            | ImGuiWindowFlags.NoResize
            | ImGuiWindowFlags.NoMove
            | ImGuiWindowFlags.NoSavedSettings
        );
        {
            ImGui.Text("Hello");
            ImGui.SameLine();
            if (ImGui.Button("GC", buttonSize))
            {
                GC.Collect();
            }

        }
        ImGui.End();



        ImGui.Begin("Settings");
        {
            // Rotate speed
            ImGui.Text("Rotate Speed Factor");
            if (ImGui.SliderFloat("##rotateSpeed", ref rotateSpeed, -5f, 5f))
            {
                pointLightRenderSystem.RotateSpeed = rotateSpeed;
            }
            ImGui.SameLine();
            if (ImGui.Button("Stop##rotateSpeedZero"))
            {
                rotateSpeed = 0f;
                pointLightRenderSystem.RotateSpeed = rotateSpeed;
            }
            
            // Y Position
            ImGui.Text("Y Position");
            if (ImGui.SliderFloat("##yPosition", ref yPosition, -.5f, 2.5f))
            {
                foreach (var mod in gameObjects.Values.Where(s => s.PointLight.HasValue))
                {
                    mod.Transform.Translation.Y = yPosition;
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("Reset##yPositionZero"))
            {
                yPosition = 0f;
                foreach (var mod in gameObjects.Values.Where(s => s.PointLight.HasValue))
                {
                    mod.Transform.Translation.Y = yPosition;
                }
            }
            
            // Light Intensity
            ImGui.Text("Light Intensity");
            if (ImGui.SliderFloat("##lightIntensity", ref lightIntensity, 0.005f, 1.000f))
            {
                foreach (var mod in gameObjects.Values.Where(s => s.PointLight.HasValue))
                {
                    mod.PointLight = new PointLightComponent(lightIntensity);
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("Reset##lightIntensityZero"))
            {
                lightIntensity = 0f;
                foreach (var mod in gameObjects.Values.Where(s => s.PointLight.HasValue))
                {
                    mod.PointLight = new PointLightComponent(lightIntensity);
                }
            }

            // Light Radius
            ImGui.Text("Light Radius");
            if (ImGui.SliderFloat("##lightRadius", ref lightRadius, 0.01f, 0.5f))
            {
                foreach (var mod in gameObjects.Values.Where(s => s.PointLight.HasValue))
                {
                    mod.Transform.Scale.X = lightRadius;
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("Reset##lightRadiusZero"))
            {
                lightRadius = 0f;
                foreach (var mod in gameObjects.Values.Where(s => s.PointLight.HasValue))
                {
                    mod.Transform.Scale.X = lightRadius;
                }
            }

        }
        ImGui.End();




        // footer
        var tick = DateTime.Now.Ticks;

        if (tick - lastSample > interval)
        {
            lastSample = tick;
            memory = Process.GetCurrentProcess().WorkingSet64 / 1_000_000;
        }

        ImGui.SetNextWindowPos(footerPos);
        ImGui.SetNextWindowSize(footerSize);
        ImGui.Begin("Status", ImGuiWindowFlags.NoBackground
        | ImGuiWindowFlags.NoTitleBar
        | ImGuiWindowFlags.NoResize
        | ImGuiWindowFlags.NoMove
        | ImGuiWindowFlags.NoSavedSettings
        | ImGuiWindowFlags.NoInputs);
        {
            var fps = $"FPS {ImGui.GetIO().Framerate,2:0.00}";
            var mems = $"MEM {memory,3:#,#}MB";
            var m2 = $"M2d {mouseLast.Pos2d.X,6:+0.0000;-0.0000;0.0000},{mouseLast.Pos2d.Y,6:+0.0000;-0.0000;0.0000}";
            //var mhud = $"Mhud {mouseHud.X,7:+0.000;-0.000},{mouseHud.Y,7:+0.000;-0.000}";
            var m3 = $"M3d {mouseLast.Pos3d.X,7:+0.000;-0.000;0.0000},{mouseLast.Pos3d.Y,7:+0.000;-0.000;0.0000},{mouseLast.Pos3d.Z,7:+0.000;-0.000;0.0000}";
            var v = $"V {window.FramebufferSize.X,4:0}x{window.FramebufferSize.Y,-4:0}";
            //var f = $"F {camera.Frustum}";
            //var p = $"P {camera.Position.X:0.000},{camera.Position.Y:0.000},{camera.Position.Z:0.000}";
            //var yaw = $"Yaw {camera.Yaw:0.000}";
            //var pitch = $"Pitch {camera.Pitch:0.000}";
            //ImGui.Text($"{status,-50} | {fps,-8} | {mems,-12} | {raw,-15} | {m2,-12} | {mhud,-12} | {m3,-22} | {v,-12} | {f,-6} | {p,-16} | {yaw,-6} | {pitch,-6}");
            ImGui.Text($"{status,-50} | {fps} | {mems} | {m2} | {m3} | {v}");
        }

        ImGui.End();

    }


    public void FirstAppGuiResize(int xp, int yp, uint wp, uint hp, Vector2D<int> size)
    {
        wPos = new Vector2(xp - 10, yp + 10);
        wSize = new Vector2(wp + 20, hp + 20);
        xc = xp + wp / 2f;
        yc = yp + hp / 2f;
        tl = new Vector2(xp, yp);
        bl = new Vector2(xp, yp + hp);
        tr = new Vector2(xp + wp, yp);
        br = new Vector2(xp + wp, yp + hp);

        cPos = new Vector2(xp + wp - 200, yp + hp - 20);

        toolbarPos = new Vector2(xp + 5, yp + 5);
        toolbarSize = new Vector2(wp - 10, 40);

        footerPos = new Vector2(10, size.Y - 25);
        footerSize = new Vector2(size.X - 20, 25);

    }


}
