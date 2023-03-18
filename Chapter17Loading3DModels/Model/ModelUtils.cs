using System.IO;

namespace Chapter17Loading3DModels;

public static class ModelUtils
{

    public static string LoadEmbeddedResource(string path, Type type)
    {
        using (var s = type.Assembly.GetManifestResourceStream(path))
        {
            if (s is null) return string.Empty;
            using (var sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }
    }

    public static string GetEmbeddedResourceObjText(string filename)
    {
        //foreach (var item in assembly.GetManifestResourceNames())
        //{
        //    Console.WriteLine($"{item}");
        //}
        //var resourceName = $"Chapter05SwapChain.{filename.Replace('/', '.')}";
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(s => s.EndsWith(filename));
        if (resourceName is null) throw new ApplicationException($"*** No obj file found with name {filename}\n*** Check that resourceName and try again!  Did you forget to set obj file to Embedded Resource/Do Not Copy?");

        using (var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new ApplicationException($"*** No shader file found at {resourceName}\n*** Check that resourceName and try again!  Did you forget to set glsl file to Embedded Resource/Do Not Copy?"))
        {
            using (var reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                return result;
            }
        }

    }




    public static LveModel LoadModelFromFile(Vk vk, LveDevice device, string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException($"Error loading model file, Can't find file at {path}");
        var builder = new Builder();
        builder.LoadModel(path);

        return new LveModel(vk, device, builder);
    }

}
