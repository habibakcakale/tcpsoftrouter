using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.AccessControl;

namespace SharpOpen.Net
{
    public class Traceable
    {
        protected string _UniqueIdentifier = Guid.NewGuid().ToString().ToUpperInvariant();

        private Stream _TraceWriter;

        public virtual bool IsBeingTraced
        {
            get
            {
                return _TraceWriter != null;
            }
            set
            {
                if (value)
                {
                    if (IsBeingTraced) return;
                    if (!Directory.Exists("LogFiles")) Directory.CreateDirectory("LogFiles");
                    if (!Directory.Exists("LogFiles\\SharpOpen.Net.TcpSoftRouter")) Directory.CreateDirectory("LogFiles\\SharpOpen.Net.TcpSoftRouter");
                    _TraceWriter = File.Create("LogFiles\\SharpOpen.Net.TcpSoftRouter\\" + TraceFileName, 4096, FileOptions.Asynchronous);
                }
                else
                {
                    if (IsBeingTraced)
                    {
                        try { _TraceWriter.Flush(); }
                        catch { }
                        try { _TraceWriter.Close(); }
                        catch { }
                        _TraceWriter = null;
                    }
                }
            }
        }

        public virtual string TraceFileName
        {
            get
            {
                return _UniqueIdentifier.ToString() + ".txt";
            }
        }

        protected void Trace(byte[] buffer, int offset, int count)
        {
            if (_TraceWriter != null)
            {
                try
                {
                    _TraceWriter.Write(buffer, offset, count);
                    _TraceWriter.Flush();
                }
                catch { }
            }
        }
    }
}
