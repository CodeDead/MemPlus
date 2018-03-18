﻿using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Interop;

namespace MemPlus.Business.UTILS
{
    /// <inheritdoc />
    /// <summary>
    /// Internal class used for controlling the registering and unregistering of hotkeys
    /// </summary>
    internal sealed class HotKeyController : IDisposable
    {
        #region Variables
        /// <summary>
        /// Holds the amount of hotkeys that were registered
        /// </summary>
        private int _currentId;
        /// <summary>
        /// The WindowInteropHelper that can be used to retrieve the handle of this Window
        /// </summary>
        private readonly WindowInteropHelper _helper;
        /// <summary>
        /// The HwndSource that can be used to retrieve messages
        /// </summary>
        private HwndSource _source;
        #endregion

        #region Events
        /// <summary>
        /// Event that is called when the hotkey was pressed
        /// </summary>
        internal event HotKeyPressed HotKeyPressedEvent;
        /// <summary>
        /// Delegate that is called when the hotkey was pressed
        /// </summary>
        internal delegate void HotKeyPressed();
        #endregion

        /// <summary>
        /// Initialize a new HotKeyController
        /// </summary>
        internal HotKeyController(WindowInteropHelper helper)
        {
            _helper = helper;
            _source = HwndSource.FromHwnd(_helper.Handle);
        }

        /// <summary>
        /// Register a hotkey
        /// </summary>
        /// <param name="modifier">The modifiers that are associated with the hotkey</param>
        /// <param name="key">The key that is associated with the hotkey</param>
        internal void RegisterHotKey(uint modifier, Keys key)
        {
            // Increment the counter.
            _currentId++;

            // Register the hotkey.
            _source = HwndSource.FromHwnd(_helper.Handle);
            if (_source == null) throw new ArgumentNullException(nameof(_source));
            _source.AddHook(HwndHook);

            if (!NativeMethods.RegisterHotKey(_helper.Handle, _currentId, modifier, (uint)key))
                throw new Exception("RegisterHotKey: ", new Win32Exception(Marshal.GetLastWin32Error()));
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int wmHotkey = 0x0312;
            if (msg == wmHotkey)
            {
                HotKeyPressedEvent?.Invoke();
                handled = true;
            }
            return IntPtr.Zero;
        }

        #region IDisposable Members
        /// <inheritdoc />
        /// <summary>
        /// Unregister all the hotkeys that were previously registered
        /// </summary>
        public void Dispose()
        {
            // Unregister all the registered hot keys.
            for (int i = _currentId; i > 0; i--)
            {
                NativeMethods.UnregisterHotKey(_helper.Handle, i);
            }
            // Remove the hook from the HwndSource
            _source.RemoveHook(HwndHook);
            _source = null;
        }
        #endregion
    }
}

/// <summary>
/// The enumeration of possible modifiers
/// </summary>
[Flags]
internal enum ModifierKeys : uint
{
    /// <summary>
    /// The uint value for the Alt key
    /// </summary>
    Alt = 1,
    /// <summary>
    /// The uint value for the Control key
    /// </summary>
    Control = 2,
    /// <summary>
    /// The uint value for the Shift key
    /// </summary>
    Shift = 4
}