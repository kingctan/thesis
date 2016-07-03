using System;
using Utils;

namespace ArgsLibrary
{
    /// <summary>
    /// Event arguments for counting down with the feedback methods.
    /// </summary>
    public class DataCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// Name of the event.
        /// </summary>
        public Recordings Title { get; set; }

        /// <summary>
        /// Name of the feedback method.
        /// </summary>
        public MethodTypes Method { get; set; }
    }
}