using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using stal.HSM.Core.Interfaces;

namespace stal.HSM.Core.Sequences
{
  /// <summary>
  /// Can be used to store the steps in a transition sequence. (i.e. Activities).
  /// Acts as a list of operations we want to perform in order or in parallel.
  /// </summary>
  public delegate Task PhaseStep(CancellationToken cancellationToken);

  /// <summary>
  /// Following the Null Object design pattern, this is a "Null" sequence. This sequence is completed immediately.
  /// </summary>
  public class NullPhase : ISequence
  {
    public bool IsDone { get; private set; }
    public void Start() => IsDone = true;
    public bool Update() => IsDone;
  }

  public class SequentialPhase : ISequence
  {
    private readonly List<PhaseStep> _steps;
    private readonly CancellationToken _cancellationToken;

    private int _progress = -1;
    private Task _currentTask;

    public bool IsDone { get; private set; }

    public SequentialPhase(List<PhaseStep> steps, CancellationToken cancellationToken)
    {
      _steps = steps;
      _cancellationToken = cancellationToken;
    }

    public void Start() => Next();

    private void Next()
    {
      _progress++;
      if (_progress >= _steps.Count)
      {
        IsDone = true;
        return;
      }

      _currentTask = _steps[_progress](_cancellationToken);
    }

    public bool Update()
    {
      if (IsDone) return true;
      if (_currentTask == null || _currentTask.IsCompleted) Next();
      return IsDone;
    }
  }

  public class ParallelPhase : ISequence
  {
    private readonly List<PhaseStep> _steps;
    private readonly CancellationToken _cancellationToken;

    private List<Task> _currentTasks;

    public ParallelPhase(List<PhaseStep> steps, CancellationToken cancellationToken)
    {
      _steps = steps;
      _cancellationToken = cancellationToken;
    }

    public bool IsDone { get; private set; }

    public void Start()
    {
      // Guard clause
      if (_steps == null || _steps.Count == 0)
      {
        IsDone = true;
        return;
      }

      _currentTasks = new(_steps.Count);

      for (int i = 0; i < _steps.Count; i++)
      {
        // Invoke each task and add it to our list of tasks.
        _currentTasks.Add(_steps[i](_cancellationToken));
      }
    }

    public bool Update()
    {
      if (IsDone) return true;
      IsDone = _currentTasks == null || _currentTasks.TrueForAll(task => task.IsCompleted);
      return IsDone;
    }
  }
}