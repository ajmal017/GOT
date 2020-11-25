using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GOT.SharedKernel.Enums;

namespace GOT.UI.Common
{
    public static class KeyHandler
    {
        /// <summary>
        ///     If the high-order bit is 1, the key is down; otherwise, it is up.
        ///     If the low-order bit is 1, the key is toggled.
        ///     A key, such as the CAPS LOCK key, is toggled if it is turned on.
        ///     The key is off and untoggled if the low-order bit is 0.
        ///     A toggle key's indicator light (if any) on the keyboard will be on when
        ///     the key is toggled, and off when the key is untoggled.
        /// </summary>
        /// <param name="key"></param>
        [DllImport("user32.dll")]
        public static extern short GetKeyState(int key);

        public static GotKeyCommand GetKeyStates()
        {
            var command = GotKeyCommand.None;
            var isCtrlDown = IsKeyDown(Keys.ControlKey);
            var isShiftDown = IsKeyDown(Keys.ShiftKey);

            if (IsKeyDown(Keys.Add)) {
                command = GotKeyCommand.AddStrategy;
            }

            if (IsKeyDown(Keys.Add) && isCtrlDown) {
                command = GotKeyCommand.AddBuyStrategy;
            }

            if (IsKeyDown(Keys.Subtract) && isCtrlDown) {
                command = GotKeyCommand.AddSellStrategy;
            }

            if (IsKeyDown(Keys.Delete)) {
                command = GotKeyCommand.SingleDelete;
            }

            if (IsKeyDown(Keys.Delete) && isCtrlDown) {
                command = GotKeyCommand.AllDelete;
            }

            if (IsKeyDown(Keys.Q) && isCtrlDown) {
                command = GotKeyCommand.OpenOptionWindow;
            }

            if (IsKeyDown(Keys.G) && isCtrlDown) {
                command = GotKeyCommand.OpenStopStrategyWindow;
            }

            if (IsKeyDown(Keys.Space)) {
                command = GotKeyCommand.SingleStop;
            }

            if (IsKeyDown(Keys.Space) && isCtrlDown) {
                command = GotKeyCommand.AllStop;
            }

            if (IsKeyDown(Keys.Enter) && isShiftDown) {
                command = GotKeyCommand.SingleStart;
            }

            if (IsKeyDown(Keys.Enter) && isCtrlDown) {
                command = GotKeyCommand.AllStart;
            }

            if (IsKeyDown(Keys.S) && isCtrlDown) {
                command = GotKeyCommand.Save;
            }

            if (IsKeyDown(Keys.I)) {
                command = GotKeyCommand.InfoStrategy;
            }

            return command;
        }

        private static bool IsKeyDown(Keys key)
        {
            return (GetKeyState(Convert.ToInt16(key)) & 0X80) == 0X80;
        }
    }
}