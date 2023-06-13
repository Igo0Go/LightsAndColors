using UnityEngine.Events;

public class DebugCommand : BaseDebugCommand
{
    public UnityEvent Execute { get; }

    public DebugCommand(string id, string description, string format) : base(id, description, format)
    {
        Execute = new UnityEvent();
    }

    public void Invoke()
    {
        Execute.Invoke();
    }
}

public class DebugCommand<T1> : BaseDebugCommand
{
    public UnityEvent<T1> Execute { get; }

    public DebugCommand(string id, string description, string format) : base(id, description, format)
    {
        Execute = new UnityEvent<T1>();
    }

    public void Invoke(T1 value)
    {
        Execute.Invoke(value);
    }
}

public class DebugCommand<T1,T2> : BaseDebugCommand
{
    public UnityEvent<T1, T2> Execute { get; }

    public DebugCommand(string id, string description, string format) : base(id, description, format)
    {
        Execute = new UnityEvent<T1, T2>();
    }

    public void Invoke(T1 t1, T2 t2)
    {
        Execute.Invoke(t1, t2);
    }
}

public class DebugCommand<T1, T2, T3> : BaseDebugCommand
{
    public UnityEvent<T1, T2, T3> Execute { get; }

    public DebugCommand(string id, string description, string format) : base(id, description, format)
    {
        Execute = new UnityEvent<T1, T2, T3>();
    }

    public void Invoke(T1 t1, T2 t2, T3 t3)
    {
        Execute.Invoke(t1, t2, t3);
    }
}
