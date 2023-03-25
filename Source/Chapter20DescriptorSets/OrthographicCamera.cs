
namespace Chapter20DescriptorSets;

public class OrthographicCamera
{

    // orthographic stuff
    private float frustum = 40;
    private float aspect = 1;
    private float left = -40;
    private float right = 40;
    private float bottom = 40;
    private float top = -40;
    private float near = 0.01f;
    private float far = 100f;
    //private float width = 80f;
    //private float height = 80f;

    private uint wp;  // viewport width in pixel
    public uint Wp => wp;
    private uint hp;  // viewport height in pixel
    public uint Hp => hp;

    private int xp;   // viewport x from top left pixel
    public int Xp => xp;
    private int yp;   // viewport y from top left pixel
    public int Yp => yp;

    private float pitch = 0f;
    private float yaw = 0f;

    private float frustumPrevious;
    public float Frustum => frustum;

    public float AspectRatio => (left - right) / (top - bottom);

    private bool enableRotation = true;
    public bool EnableRotation { get => enableRotation; set => enableRotation = value; }


    private Vector3 position;
    public Vector3 Position { get => position; set => position = value; }

    private Vector3 frontVec;
    public Vector3 FrontVec { get => frontVec; set => frontVec = value; }

    private Vector3 globalUp = Vector3.UnitY;
    private Vector3 upVec;
    public Vector3 UpVec { get => upVec; set => upVec = value; }

    private Vector3 rightVec;
    public Vector3 RightVec { get => rightVec; set => rightVec = value; }

    private float zoomMin = 0.01f;
    private float zoomMax = 700f;

    private float zoomAccelerationWheel = 80f;
    private float zoomSpeedWheel = 10f;

    //private float zoomAccelerationMouse = 40f;
    //private float zoomSpeedMouse = 80f;

    private float pitchClamp = 89.99f * MathF.PI / 180f;

    public float Pitch
    {
        get => pitch;
        set
        {
            var angle = (float)Math.Clamp(value, -pitchClamp, pitchClamp);
            pitch = angle;
            UpdateVectors();
        }
    }
    public float PitchDegrees => pitch / MathF.PI * 180f;

    public float Yaw
    {
        get => yaw;
        set
        {
            yaw = value;
            UpdateVectors();
        }
    }
    public float YawDegrees => yaw / MathF.PI * 180f;


    public OrthographicCamera(Vector3 position, float frustum, float pitchDeg, float yawDeg, Vector2D<int> frameBufferSize)
    {
        this.frustum = frustum;
        near = -20f;
        far = 20f;
        Pitch = pitchDeg * MathF.PI / 180f;
        Yaw = yawDeg * MathF.PI / 180f;
        frustumPrevious = frustum;


        this.position = position;

        frontVec = Vector3.UnitZ;
        upVec = globalUp;
        rightVec = Vector3.UnitX;

        Resize(0, 0, (uint)frameBufferSize.X, (uint)frameBufferSize.Y);
    }


    private void updateOrtho()
    {
        // Vulkan does top = negative
        left = (frustum * aspect) / -2f;
        right = (frustum * aspect) / 2f;
        //width = right - left;
        top = frustum / -2f;
        bottom = frustum / 2f;
        //height = bottom - top;
    }




    // Get the view matrix using the amazing LookAt function described more in depth on the web tutorials
    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateLookAt(position, position + frontVec, upVec);
    }

    public Matrix4x4 GetViewMatrixGlm()
    {
        // from glm example
        //const glm::vec3 w{ glm::normalize(direction)};
        //const glm::vec3 u{ glm::normalize(glm::cross(w, up))};
        //const glm::vec3 v{ glm::cross(w, u)};

        //viewMatrix = glm::mat4{ 1.f};
        //viewMatrix[0][0] = u.x;
        //viewMatrix[1][0] = u.y;
        //viewMatrix[2][0] = u.z;
        //viewMatrix[0][1] = v.x;
        //viewMatrix[1][1] = v.y;
        //viewMatrix[2][1] = v.z;
        //viewMatrix[0][2] = w.x;
        //viewMatrix[1][2] = w.y;
        //viewMatrix[2][2] = w.z;
        //viewMatrix[3][0] = -glm::dot(u, position);
        //viewMatrix[3][1] = -glm::dot(v, position);
        //viewMatrix[3][2] = -glm::dot(w, position);

        Vector3 w = Vector3.Normalize((position + frontVec) - (position));
        Vector3 u = Vector3.Normalize(Vector3.Cross(w, upVec));
        Vector3 v = Vector3.Cross(w, u);

        return Matrix4x4.Identity with
        {
            M11 = u.X,
            M21 = u.Y,
            M31 = u.Z,
            M12 = v.X,
            M22 = v.Y,
            M32 = v.Z,
            M13 = w.X,
            M23 = w.Y,
            M33 = w.Z,
            M41 = -Vector3.Dot(u, position),
            M42 = -Vector3.Dot(v, position),
            M43 = -Vector3.Dot(w, position),
        };

    }

    // Get the projection matrix using the same method we have used up until this point
    public Matrix4x4 GetProjectionMatrix()
    {
        return Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, near, far);
    }
    public Matrix4x4 GetProjectionMatrixGlm()
    {
        // glm example

        // constructor camera.setOrthographicProjection(-aspect, aspect, -1,  1,     -1,    1);
        //                                              left     right   top  bottom  near  far

        //void LveCamera::setOrthographicProjection(
        //    float left, float right, float top, float bottom, float near, float far) {
        //    projectionMatrix = glm::mat4{ 1.0f};
        //    projectionMatrix[0][0] = 2.f / (right - left);
        //    projectionMatrix[1][1] = 2.f / (bottom - top);
        //    projectionMatrix[2][2] = 1.f / (far - near);
        //    projectionMatrix[3][0] = -(right + left) / (right - left);
        //    projectionMatrix[3][1] = -(bottom + top) / (bottom - top);
        //    projectionMatrix[3][2] = -near / (far - near);
        //}
        return Matrix4x4.Identity with
        {
            M11 = 2.0f / (right - left),
            M22 = 2.0f / (bottom - top),
            M33 = 1.0f / (far - near),
            M41 = -(right + left) / (right - left),
            M42 = -(bottom + top) / (bottom - top),
            M43 = -near / (far - near),
        };

    }


    // This function is going to update the direction vertices using some of the math learned in the web tutorials
    private void UpdateVectors()
    {
        // First the front matrix is calculated using some basic trigonometry
        frontVec.X = (float)Math.Cos(pitch) * (float)Math.Cos(yaw);
        frontVec.Y = (float)Math.Sin(pitch);
        frontVec.Z = (float)Math.Cos(pitch) * (float)Math.Sin(yaw);

        // We need to make sure the vectors are all normalized, as otherwise we would get some funky results
        frontVec = Vector3.Normalize(frontVec);

        // Calculate both the right and the up vector using cross product
        // Note that we are calculating the right from the global up, this behaviour might
        // not be what you need for all cameras so keep this in mind if you do not want a FPS camera
        rightVec = Vector3.Normalize(Vector3.Cross(frontVec, globalUp));
        upVec = Vector3.Normalize(Vector3.Cross(rightVec, frontVec));
    }

    public Vector2 Project(Vector3 mouse3d)
    {

        Vector4 vec;

        vec.X = mouse3d.X;
        vec.Y = mouse3d.Y;
        vec.Z = mouse3d.Z;
        vec.W = 1.0f;

        vec = Vector4.Transform(vec, GetViewMatrix());
        vec = Vector4.Transform(vec, GetProjectionMatrix());

        if (vec.W > float.Epsilon || vec.W < -float.Epsilon)
        {
            vec.X /= vec.W;
            vec.Y /= vec.W;
            vec.Z /= vec.W;
        }

        return new Vector2(vec.X, vec.Y);
    }

    public Vector3 UnProject(Vector2 mouse2d)
    {
        Vector4 vec;

        vec.X = mouse2d.X;
        vec.Y = mouse2d.Y;
        vec.Z = 0f;
        vec.W = 1.0f;

        Matrix4x4.Invert(GetViewMatrix(), out Matrix4x4 viewInv);
        Matrix4x4.Invert(GetProjectionMatrix(), out Matrix4x4 projInv);

        vec = Vector4.Transform(vec, projInv);
        vec = Vector4.Transform(vec, viewInv);

        if (vec.W > float.Epsilon || vec.W < -float.Epsilon)
        {
            vec.X /= vec.W;
            vec.Y /= vec.W;
            vec.Z /= vec.W;
        }

        return new Vector3(vec.X, vec.Y, vec.Z);
    }


    public void Pan(Vector3 vStart, Vector3 vStop)
    {
        var vdiff = vStart - vStop;
        position += vdiff;
        //Console.WriteLine($"position=[{position.X:+0.0000;-0.0000},{position.Y:+0.0000;-0.0000},{position.Z:+0.0000;-0.0000}");
    }

    public void Rotate(Vector2 vStart, Vector2 vStop)
    {
        if (!enableRotation) return;
        //Console.WriteLine($"[{vStart.X:+0.0000;-0.0000},{vStart.Y:+0.0000;-0.0000}] to [{vStop.X:+0.0000;-0.0000},{vStop.Y:+0.0000;-0.0000}]");
        var sx = 1.1f;
        Yaw += (vStop.X - vStart.X) * sx * aspect;
        Pitch -= (vStop.Y - vStart.Y) * sx;
        //Console.WriteLine($"pitch={pitch * 180f / MathF.PI:0.0000}, yaw={yaw * 180f / MathF.PI:0.0000}");
        UpdateVectors();
    }

    public void ZoomIncremental(float amount)
    {
        frustum = frustumPrevious + amount * zoomSpeedWheel * frustumPrevious / zoomAccelerationWheel;
        frustum = Math.Clamp(frustum, zoomMin, zoomMax);
        frustumPrevious = frustum;
        updateOrtho();
    }

    //public void ZoomMouse(float amount)
    //{
    //    frustum = frustumPrevious + amount * zoomSpeedMouse * frustumPrevious / zoomAccelerationMouse;
    //    frustum = Math.Clamp(frustum, zoomMin, zoomMax);
    //    left = (frustum * aspect) / -2f;
    //    right = (frustum * aspect) / 2f;
    //    width = right - left;
    //    top = frustum / 2f;
    //    bottom = frustum / -2f;
    //    height = bottom - top;
    //}

    public void ZoomSetPrevious()
    {
        frustumPrevious = frustum;
    }
    public void FitHeight(float fitHeight)
    {
        frustum = fitHeight;
        frustumPrevious = frustum;
        updateOrtho();
    }


    public void Resize(int xp, int yp, uint wp, uint hp)
    {
        this.xp = xp;
        this.yp = yp;
        this.wp = wp;
        this.hp = hp;

        aspect = (float)wp / (float)hp;

        updateOrtho();

        UpdateVectors();
        //Console.WriteLine($" camera {Name} | Resized {wp}x{hp}");
    }

    public void Reset()
    {
    }



}