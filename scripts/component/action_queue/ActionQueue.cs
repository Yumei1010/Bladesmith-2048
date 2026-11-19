using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bladesmith2048.scripts.component.action_queue;

/// <summary>
///     串行执行异步步骤：前一步完成才执行下一步。
/// </summary>
/// <remarks>
///     零 Godot / GFramework 依赖，可跨线程入队。
///     一步抛异常即中断整链、丢弃剩余步骤，并通过 <see cref="OnStepFailed" /> 与
///     <see cref="WaitUntilIdleAsync" /> 传出去；续体线程由调用方传入的调度器决定
///     （Godot 里传 <c>MainThread.RunAsync</c>）。
/// </remarks>
public sealed class ActionQueue
{
    private readonly Lock _gate = new();
    private readonly Queue<Func<Task>> _pending = new();
    private readonly Func<Func<Task>, Task>? _schedule;
    private TaskCompletionSource? _idle;
    private bool _isRunning;

    /// <summary>
    ///     创建队列。
    /// </summary>
    public ActionQueue(Func<Func<Task>, Task>? schedule = null) => _schedule = schedule;

    /// <summary>
    ///     某一步骤执行失败时触发（参数为该步骤抛出的异常）。
    /// </summary>
    /// <remarks>
    ///     无论是否订阅该事件，异常都会通过 <see cref="WaitUntilIdleAsync" /> 传播给等待方。
    /// </remarks>
    public event Action<Exception>? OnStepFailed;

    /// <summary>
    ///     队列是否正在执行（有步骤运行中或待执行）。
    /// </summary>
    public bool IsRunning
    {
        get
        {
            lock (_gate)
            {
                return _isRunning;
            }
        }
    }

    /// <summary>
    ///     当前待执行步骤数（不含正在执行的）。
    /// </summary>
    public int PendingCount
    {
        get
        {
            lock (_gate)
            {
                return _pending.Count;
            }
        }
    }

    /// <summary>
    ///     是否为空（无待执行且未在运行）。
    /// </summary>
    public bool IsEmpty
    {
        get
        {
            lock (_gate)
            {
                return !_isRunning && _pending.Count == 0;
            }
        }
    }

    /// <summary>
    ///     排入一个异步步骤。若队列空闲则立即开始执行，否则等待前序完成。
    /// </summary>
    /// <exception cref="ArgumentNullException">当 <paramref name="step" /> 为 <see langword="null" /> 时抛出。</exception>
    public void Enqueue(Func<Task> step)
    {
        ArgumentNullException.ThrowIfNull(step);

        var shouldStart = false;
        lock (_gate)
        {
            _pending.Enqueue(step);
            if (!_isRunning)
            {
                _isRunning = true;
                _idle ??= new TaskCompletionSource();
                shouldStart = true;
            }
        }

        if (shouldStart)
        {
            _ = RunAsync();
        }
    }

    /// <summary>
    ///     清空所有待执行步骤（正在执行的步骤不受影响）。
    /// </summary>
    public void Clear()
    {
        lock (_gate)
        {
            _pending.Clear();
        }
    }

    /// <summary>
    ///     等待队列执行完毕（无待执行且未在运行）。
    /// </summary>
    /// <returns>队列空闲时完成；若期间某步骤失败，则该 Task 以该异常失败。</returns>
    public Task WaitUntilIdleAsync()
    {
        lock (_gate)
        {
            if (!_isRunning && _pending.Count == 0)
            {
                return Task.CompletedTask;
            }

            return (_idle ??= new TaskCompletionSource()).Task;
        }
    }

    /// <summary>
    ///     串行取出并执行步骤，直到队列清空、被清空或某一步失败。
    /// </summary>
    private async Task RunAsync()
    {
        Exception? failure = null;

        while (true)
        {
            Func<Task>? step;
            lock (_gate)
            {
                if (_pending.Count == 0)
                {
                    break;
                }

                step = _pending.Dequeue();
            }

            try
            {
                var running = _schedule is null ? step() : _schedule(step);
                await running.ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                failure = exception;
                lock (_gate)
                {
                    // 失败即丢弃剩余步骤，避免在前置条件未满足时继续执行后续动作
                    _pending.Clear();
                }

                break;
            }
        }

        TaskCompletionSource? idle;
        lock (_gate)
        {
            _isRunning = false;
            idle = _idle;
            _idle = null;
        }

        if (failure is null)
        {
            idle?.TrySetResult();
            return;
        }

        OnStepFailed?.Invoke(failure);
        idle?.TrySetException(failure);
    }
}
