
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

    }


}
