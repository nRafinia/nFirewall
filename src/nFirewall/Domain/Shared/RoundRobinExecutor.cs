namespace nFirewall.Domain.Shared;

public class RoundRobinExecutor
{
    private readonly int _maxThreads;
    private readonly IList<Thread> _threads;
    private int _currentThreadIndex;

    public RoundRobinExecutor(int maxThreads)
    {
        _maxThreads = maxThreads;
        _threads = new List<Thread>(_maxThreads);
    }
    
    public void Execute(Action action)
    {
        if (_threads.Count < _maxThreads)
        {
            var thread = new Thread(() => action());
            _threads.Add(thread);
            thread.Start();
            return;
        }

        if (_currentThreadIndex >= _maxThreads)
        {
            _currentThreadIndex = 0;
        }

        var currentThread = _threads[_currentThreadIndex];
        if (currentThread.IsAlive)
        {
            _currentThreadIndex++;
            Execute(action);
            return;
        }

        currentThread = new Thread(() => action());
        _threads[_currentThreadIndex] = currentThread;
        currentThread.Start();
        _currentThreadIndex++;
    }
}