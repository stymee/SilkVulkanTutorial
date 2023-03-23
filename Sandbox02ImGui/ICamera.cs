namespace Sandbox02ImGui
{
    public interface ICamera
    {
        float AspectRatio { get; }
        bool EnableRotation { get; set; }
        Vector3 FrontVec { get; set; }
        Vector4 GetFrontVec4();
        uint Hp { get; }
        float Pitch { get; set; }
        float PitchDegrees { get; }
        Vector3 Position { get; set; }
        Vector3 RightVec { get; set; }
        Vector3 UpVec { get; set; }
        uint Wp { get; }
        int Xp { get; }
        float Yaw { get; set; }
        float YawDegrees { get; }
        int Yp { get; }

        float Frustum { get; }

        Matrix4x4 GetInverseViewMatrix();
        Matrix4x4 GetProjectionMatrix();
        //Matrix4x4 GetProjectionMatrixGlm();
        Matrix4x4 GetViewMatrix();
        //Matrix4x4 GetViewMatrixGlm();
        void Pan(Vector3 vStart, Vector3 vStop);
        Vector2 Project(Vector3 mouse3d);
        void Reset();
        void Resize(int xp, int yp, uint wp, uint hp);
        void Rotate(Vector2 vStart, Vector2 vStop);
        Vector3 UnProject(Vector2 mouse2d);
        void ZoomIncremental(float amount);
        void ZoomSetPrevious();
    }
}