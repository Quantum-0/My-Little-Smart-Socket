namespace MyLittleSmartSocket
{
    internal class SmartSocketStateChangedEventArgs: System.EventArgs
    {
        public int Timer { get; }
        public bool State { get; }

        public SmartSocketStateChangedEventArgs(bool state, int timer)
        {
            State = state;
            Timer = timer;
        }
    }
}