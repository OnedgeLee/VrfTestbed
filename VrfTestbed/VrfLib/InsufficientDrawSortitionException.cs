using System;

namespace VrfTestbed.VrfLib
{
    public class InsufficientDrawSortitionException : Exception
    {
        public InsufficientDrawSortitionException(int required, int drawn)
            : base($"Failed to draw sufficient number, required : {required} > drawn : {drawn}")
        {
        }
    }
}