﻿using System;
using System.ComponentModel;
using H.Hooks.Core;
using H.Hooks.Core.Interop;
using H.Hooks.Core.Interop.WinUser;

namespace H.Hooks
{
    public abstract class Hook : IDisposable
    {
        public static Keys FromString(string text) => Enum.TryParse<Keys>(text, true, out var result) ? result : Keys.None;

        #region Properties

        public bool IsStarted { get; private set; }

        private IntPtr HookHandle { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<Exception>? ExceptionOccurred;

        private void OnExceptionOccurred(Exception value)
        {
            ExceptionOccurred?.Invoke(this, value);
        }

        #endregion

        #region Protected methods

        protected abstract IntPtr InternalCallback(int nCode, int wParam, IntPtr lParam);

        #endregion

        #region Private methods

        private IntPtr Callback(int nCode, int wParam, IntPtr lParam)
        {
            try
            {
                return InternalCallback(nCode, wParam, lParam);
            }
            catch (Exception exception)
            {
                OnExceptionOccurred(exception);

                return User32.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Starts hook process.
        /// </summary>
        /// <exception cref="Win32Exception">If SetWindowsHookEx return error code</exception>
        internal void Start(HookProcedureType type)
        {
            if (IsStarted)
            {
                return;
            }

            var moduleHandle = Kernel32Methods.GetCurrentProcessModuleHandle();

            HookHandle = User32.SetWindowsHookEx(type, Callback, moduleHandle, 0).Check();

            IsStarted = true;
        }

        /// <summary>
        /// Stop hook process
        /// </summary>
        public void Stop()
        {
            if (!IsStarted)
            {
                return;
            }

            User32.UnhookWindowsHookEx(HookHandle);

            IsStarted = false;
        }
        
        /// <summary>
        /// Dispose internal system hook resources.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        #endregion
    }
}