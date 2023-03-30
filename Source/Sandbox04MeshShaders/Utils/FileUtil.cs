
namespace Sandbox04MeshShaders;

public sealed class FileUtil
{
    public static byte[] GetShaderBytes(string filename, string renderSystemName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        //foreach (var item in assembly.GetManifestResourceNames())
        //{
        //    Console.WriteLine($"{item}");
        //}
        var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(s => s.EndsWith(filename));
        if (resourceName is null) throw new ApplicationException($"*** In {renderSystemName}, No shader file found with name {filename}\n*** Check that resourceName and try again!  Did you forget to set glsl file to Embedded Resource/Do Not Copy?");

        using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new ApplicationException($"*** In {renderSystemName}, No shader file found at {resourceName}\n*** Check that resourceName and try again!  Did you forget to set glsl file to Embedded Resource/Do Not Copy?");
        using var ms = new MemoryStream();
        if (stream is null) return Array.Empty<byte>();
        stream.CopyTo(ms);
        return ms.ToArray();

    }



}
