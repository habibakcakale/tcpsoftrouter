using System;
using System.Collections.Generic;
using System.Text;

namespace SharpOpen.Net.Observers.ConnectionObservers
{
    public class BufferedConnectionObserver : DefaultConnectionObserver
    {
        private List<byte> _Buffer = new List<byte>();
        private List<byte[]> _RequestCompleteTests = new List<byte[]>();

        public delegate void OnRequestCompletedDelegate(Connection client, int matchingTestIndex, byte[] buffer);
        public event OnRequestCompletedDelegate OnRequestCompleted;

        public int AddTest(byte[] requestCompleteTest)
        {
            _RequestCompleteTests.Add(requestCompleteTest);
            return _RequestCompleteTests.Count - 1;
        }

        private void AppendReceivedBytes(byte[] receiveBuffer, int offset, int count)
        {
            byte[] received = new byte[count];
            Array.ConstrainedCopy(receiveBuffer, offset, received, 0, count);
            _Buffer.AddRange(received);
        }

        public override void HandleReceive(byte[] receiveBuffer, int offset, int count)
        {
            AppendReceivedBytes(receiveBuffer, offset, count);

            for(int testIndex = 0; testIndex < _RequestCompleteTests.Count; testIndex++)
            {
                byte[] requestCompleteTest = _RequestCompleteTests[testIndex];
                if (_Buffer.Count < requestCompleteTest.Length) continue;

                bool isCompleted = true;
                for (int i = 0; i < requestCompleteTest.Length; i++)
                {
                    if (_Buffer[_Buffer.Count - 1 - i] != requestCompleteTest[requestCompleteTest.Length - 1 - i])
                    {
                        isCompleted = false;
                        break;
                    }
                }

                if (isCompleted && OnRequestCompleted != null)
                {
                    try
                    {
                        OnRequestCompleted.Invoke(_Client, testIndex, _Buffer.ToArray());
                    }
                    catch
                    {
                    }
                    _Buffer.Clear();
                }
            }
        }

        public override void HandleLocalDisconnect()
        {

        }

        public override void HandleRemoteDisconnect()
        {

        }
    }
}
