using Godot;

namespace Bladesmith2048.scripts.framework.godot;

/// <summary>
///     把异步步骤贴回 Godot 主线程执行。
/// </summary>
/// <remarks>
///     节点只能在主线程访问，而 <c>await</c> 一个真正的 <c>async Task</c> 之后，
///     续体可能落在线程池（.NET 7+ 的异步方法默认 <c>RunContinuationsAsynchronously</c>，
///     且 Godot 没有装同步上下文把续体投回主线程）。
///     纯逻辑组件（如 <see cref="component.action_queue.ActionQueue" />）不该依赖 Godot，
///     因此由这一侧提供调度器，把每个步骤显式送回主线程。
/// </remarks>
public static class MainThread
{
    /// <summary>
    ///     在主线程执行一步异步逻辑，完成（或失败）后再回到主线程完成返回的任务。
    /// </summary>
    /// <remarks>
    ///     结果必须也在主线程设置：<c>await</c> 方会在设置结果的线程上续跑，
    ///     否则调用方以为回到了主线程，实际上仍在池线程。代价是每次多等一帧。
    /// </remarks>
    public static Task RunAsync(Func<Task> step)
    {
        var completion = new TaskCompletionSource();

        // 必须用语句体的 Action：表达式体 lambda 会被推断成 Func<Task>，
        // Godot 在 CallDeferred 时会把返回值当 Variant 转换而报错
        Callable.From(() =>
        {
            _ = RunStepAsync(step, completion);
        }).CallDeferred();

        return completion.Task;
    }

    private static async Task RunStepAsync(Func<Task> step, TaskCompletionSource completion)
    {
        try
        {
            await step();
            Callable.From(() => completion.TrySetResult()).CallDeferred();
        }
        catch (Exception exception)
        {
            Callable.From(() => completion.TrySetException(exception)).CallDeferred();
        }
    }
}
