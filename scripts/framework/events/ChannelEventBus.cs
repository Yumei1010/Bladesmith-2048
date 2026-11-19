using System.Collections.Concurrent;
using GFramework.Core.Abstractions.Events;
using GFramework.Core.Events;

namespace Bladesmith2048.scripts.framework.events;

/// <summary>
///     带频段的事件总线：同名事件发到不同频段互不干扰。
/// </summary>
/// <remarks>
///     频段懒创建、常驻内存：按局/按房间动态命名的频段必须 <see cref="RemoveChannel" />，
///     否则会一直累积；固定用途用 <c>ChannelConstants</c> 常量。
///     内部是 ConcurrentDictionary，可跨线程收发。
/// </remarks>
public class ChannelEventBus : EventBus
{
    private readonly ConcurrentDictionary<string, EventBus> _channels = new(StringComparer.Ordinal);

    /// <summary>
    ///     当前已创建的频段名称集合（用于调试/诊断）。
    /// </summary>
    public IReadOnlyCollection<string> ChannelNames => _channels.Keys.ToArray();

    /// <summary>
    ///     当前已创建的频段数量。
    /// </summary>
    public int ChannelCount => _channels.Count;

    /// <summary>
    ///     订阅指定频段上的事件。
    /// </summary>
    /// <returns>取消订阅句柄</returns>
    /// <exception cref="ArgumentException">当 <paramref name="channel" /> 为空时抛出。</exception>
    public IUnRegister RegisterOnChannel<T>(string channel, Action<T> handler)
    {
        ArgumentException.ThrowIfNullOrEmpty(channel);
        ArgumentNullException.ThrowIfNull(handler);

        return GetChannel(channel).Register(handler);
    }

    /// <summary>
    ///     向指定频段发送事件（带数据）。
    /// </summary>
    /// <exception cref="ArgumentException">当 <paramref name="channel" /> 为空时抛出。</exception>
    public void SendOnChannel<T>(string channel, T eventData)
    {
        ArgumentException.ThrowIfNullOrEmpty(channel);
        GetChannel(channel).Send(eventData);
    }

    /// <summary>
    ///     向指定频段发送事件（无数据标记事件）。
    /// </summary>
    /// <exception cref="ArgumentException">当 <paramref name="channel" /> 为空时抛出。</exception>
    public void SendOnChannel<T>(string channel) where T : new()
    {
        ArgumentException.ThrowIfNullOrEmpty(channel);
        GetChannel(channel).Send<T>();
    }

    /// <summary>
    ///     判断指定频段是否已创建。
    /// </summary>
    /// <returns>该频段已存在时返回 true。</returns>
    public bool ContainsChannel(string channel)
    {
        return !string.IsNullOrEmpty(channel) && _channels.ContainsKey(channel);
    }

    /// <summary>
    ///     移除指定频段（连同其上的全部订阅）。
    /// </summary>
    /// <returns>成功移除返回 true；频段不存在返回 false。</returns>
    /// <remarks>
    ///     移除后该频段名再次被使用时将创建全新的频段实例，因此旧订阅者不会再收到新事件
    ///     （其持有的取消句柄仍指向已丢弃的实例，调用无副作用）。
    ///     动态频段使用完毕后应调用本方法释放，避免内存持续累积。
    /// </remarks>
    public bool RemoveChannel(string channel)
    {
        return !string.IsNullOrEmpty(channel) && _channels.TryRemove(channel, out _);
    }

    /// <summary>
    ///     移除全部频段（连同其上的全部订阅）。
    /// </summary>
    public void ClearChannels()
    {
        _channels.Clear();
    }

    /// <summary>
    ///     获取指定频段的 EventBus，不存在则创建。
    /// </summary>
    /// <param name="channel">频段名称（已由调用方校验非空）</param>
    /// <returns>该频段对应的 EventBus</returns>
    private EventBus GetChannel(string channel)
    {
        return _channels.GetOrAdd(channel, static _ => new EventBus());
    }
}
