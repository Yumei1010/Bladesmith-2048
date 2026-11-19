using GFramework.Game.Abstractions.Input;

namespace Bladesmith2048.scripts.component.input_rebind;

/// <summary>
///     输入绑定描述工厂：把键码/按钮索引等原始信息转成框架绑定描述。
/// </summary>
/// <remarks>
///     框架的 <c>GodotInputBindingCodec</c> 未公开，本类按相同的 Code 格式补齐：
///     <c>key:65</c> / <c>mouse:1</c> / <c>joy-button:0</c> / <c>joy-axis:0:-1</c>。
///     只接受基本类型，可脱离 Godot 运行时单测。
/// </remarks>
public static class InputBindingDescriptorFactory
{
    /// <summary>
    ///     创建键盘按键绑定。
    /// </summary>
    /// <param name="displayName">显示名（如 "W"）；为空时退化为键码文本。</param>
    /// <returns>绑定描述。</returns>
    public static InputBindingDescriptor ForKey(int keyCode, string? displayName = null)
    {
        return new InputBindingDescriptor(
            InputDeviceKind.KeyboardMouse,
            InputBindingKind.Key,
            $"key:{keyCode}",
            displayName ?? $"Key {keyCode}");
    }

    /// <summary>
    ///     创建鼠标按键绑定。
    /// </summary>
    /// <param name="displayName">显示名；为空时退化为按钮索引文本。</param>
    /// <returns>绑定描述。</returns>
    public static InputBindingDescriptor ForMouse(int buttonIndex, string? displayName = null)
    {
        return new InputBindingDescriptor(
            InputDeviceKind.KeyboardMouse,
            InputBindingKind.MouseButton,
            $"mouse:{buttonIndex}",
            displayName ?? $"Mouse {buttonIndex}");
    }

    /// <summary>
    ///     创建手柄按键绑定。
    /// </summary>
    /// <param name="displayName">显示名（如 "A"）；为空时退化为按钮索引文本。</param>
    /// <returns>绑定描述。</returns>
    public static InputBindingDescriptor ForGamepadButton(int buttonIndex, string? displayName = null)
    {
        return new InputBindingDescriptor(
            InputDeviceKind.Gamepad,
            InputBindingKind.GamepadButton,
            $"joy-button:{buttonIndex}",
            displayName ?? $"Button {buttonIndex}");
    }

    /// <summary>
    ///     创建手柄轴绑定（按轴的当前值方向取 ±1）。
    /// </summary>
    /// <param name="displayName">显示名（如 "左摇杆 ←"）；为空时退化为轴索引文本。</param>
    /// <returns>绑定描述。</returns>
    public static InputBindingDescriptor ForGamepadAxis(int axis, float value, string? displayName = null)
    {
        var direction = value >= 0f ? 1f : -1f;
        return new InputBindingDescriptor(
            InputDeviceKind.Gamepad,
            InputBindingKind.GamepadAxis,
            $"joy-axis:{axis}:{direction}",
            displayName ?? $"Axis {axis}",
            direction);
    }

    /// <summary>
    ///     判断绑定是否属于指定设备类别（用于捕获时过滤其他设备的输入）。
    /// </summary>
    /// <returns>true 表示匹配。</returns>
    public static bool MatchesDevice(InputBindingDescriptor binding, InputDeviceKind deviceKind)
    {
        return binding.DeviceKind == deviceKind;
    }
}
