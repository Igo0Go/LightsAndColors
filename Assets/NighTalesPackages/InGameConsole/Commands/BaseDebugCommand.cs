public class BaseDebugCommand
{
    public string CommandId { get; }
    public string CommandDescription { get; }
    public string CommandFormat { get; }

    public BaseDebugCommand(string id, string description, string format)
    {
        CommandId = id;
        CommandDescription = description;
        CommandFormat = format;
    }
}