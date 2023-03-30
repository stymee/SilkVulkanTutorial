using Sandbox02ImGui;

var app = new FirstApp();

try
{
	throw new Exception("Nothing to see here yet, just a placeholder!");
	app.Run();

}
catch (Exception ex)
{
	Console.WriteLine($"Error!\n{ex.Message}");
	throw;
}


