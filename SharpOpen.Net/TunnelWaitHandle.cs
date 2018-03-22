using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SharpOpen.Net
{
    public class TunnelWaitHandle : EventWaitHandle
    {
        private object _Result;
        private bool _IsSet;
        
        public TunnelWaitHandle() : base(false, EventResetMode.ManualReset) { }

        public object Result { get { return _Result; } set { _Result = value; } }
        internal bool IsSet { get { return _IsSet; } set { _IsSet = value; } }
    }
}
