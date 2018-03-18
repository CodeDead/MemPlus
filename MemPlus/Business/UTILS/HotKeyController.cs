using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

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
        /// The NativeWindow that can be used to receive messages
        /// </summary>
        private readonly Window _window = new Window();
        /// <summary>
        /// Holds the amount of hotkeys that were registered
        /// </summary>
        private int _currentId;
        #endregion

        #region Events
        /// <summary>
        /// The event that is called when a hotkey has been pressed.
        /// </summary>
        internal event EventHandler<KeyPressedEventArgs> KeyPressed;
        #endregion

        /// <inheritdoc cref="NativeWindow" />
        /// <summary>
        /// Represents the Window object that is used internally to get messages
        /// </summary>
        private sealed class Window : NativeWindow, IDisposable
        {
            /// <summary>
            /// The value for the Window Message, representing a hotkey
            /// </summary>
            private const int WmHotkey = 0x0312;

            /// <summary>
            /// The event that is called when the hotkey was pressed
            /// </summary>
            internal event EventHandler<KeyPressedEventArgs> KeyPressed;

            /// <inheritdoc />
            /// <summary>
            /// Initialize a new NativeWindow object
            /// </summary>
            public Window()
            {
                // Create the handle for the window.
                CreateHandle(new CreateParams());
            }

            /// <inheritdoc />
            /// <summary>
            /// Overridden to get the notifications.
            /// </summary>
            /// <param name="m"></param>
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                // Only continue if the hotkey message was sent
                if (m.Msg != WmHotkey) return;
                // Get the keys
                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

                // Invoke the event to notify the parent.
                KeyPressed?.Invoke(this, new KeyPressedEventArgs(modifier, key));
            }

            #region IDisposable Members
            /// <inheritdoc />
            /// <summary>
            /// Dispose this NativeWindow and destroy the handles
            /// </summary>
            public void Dispose()
            {
                DestroyHandle();
            }
            #endregion
        }

        /// <summary>
        /// Initialize a new HotKeyController object
        /// </summary>
        internal HotKeyController()
        {
            // register the event of the inner native window.
            _window.KeyPressed += delegate (object sender, KeyPressedEventArgs args)
            {
                KeyPressed?.Invoke(this, args);
            };
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
            if (!NativeMethods.RegisterHotKey(_window.Handle, _currentId, modifier, (uint)key))
                throw new Exception("RegisterHotKey: ", new Win32Exception(Marshal.GetLastWin32Error()));
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
                NativeMethods.UnregisterHotKey(_window.Handle, i);
            }

            // dispose the inner native window.
            _window.Dispose();
        }
        #endregion
    }
}

/// <inheritdoc />
/// <summary>
/// Custom EventArgs for the event that is fired after the hotkey has been pressed
/// </summary>
internal class KeyPressedEventArgs : EventArgs
{
    #region Properties
    /// <summary>
    /// Property containing the ModifierKeys that were pressed
    /// </summary>
    internal ModifierKeys Modifier { get; }
    /// <summary>
    /// Property containing the key that was pressed
    /// </summary>
    internal Keys Key { get; }
    #endregion

    /// <inheritdoc />
    /// <summary>
    /// Initialize a new KeyPressedEventArgs
    /// </summary>
    /// <param name="modifier">The ModifierKeys that were pressed</param>
    /// <param name="key">The Kkey that was pressed</param>
    internal KeyPressedEventArgs(ModifierKeys modifier, Keys key)
    {
        Modifier = modifier;
        Key = key;
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