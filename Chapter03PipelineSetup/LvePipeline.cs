

using System.Reflection;

namespace Chapter03PipelineSetup;

public class LvePipeline
{
    public LvePipeline(string vertPath, string fragPath)
    {
        createGraphicsPipeline(vertPath, fragPath);
    }


    private void createGraphicsPipeline(string vertPath, string fragPath)
    {
        var vertSource = getShaderBytes(vertPath);
        var fragSource = getShaderBytes(fragPath);

        Console.WriteLine($"shader bytes are {vertSource.Length} and {fragSource.Length}");

    }

    private static byte[] getShaderBytes(string filename)
    {
        //foreach (var item in assembly.GetManifestResourceNames())
        //{
        //    Console.WriteLine($"{item}");
        //}
        //var resourceName = $"Chapter03PipelineSetup.{filename.Replace('/', '.')}";
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(s => s.EndsWith(filename));
        if (resourceName is null) throw new ApplicationException($"*** No shader file found with name {filename}\n*** Check that resourceName and try again!  Did you forget to set glsl file to Embedded Resource/Do Not Copy?");

        using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new ApplicationException($"*** No shader file found at {resourceName}\n*** Check that resourceName and try again!  Did you forget to set glsl file to Embedded Resource/Do Not Copy?");
        using var ms = new MemoryStream();
        if (stream is null) return Array.Empty<byte>();
        stream.CopyTo(ms);
        return ms.ToArray();

    }
}
