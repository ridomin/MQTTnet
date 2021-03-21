using System;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.Pipelines
{
    public sealed class SocketAwaitable : ICriticalNotifyCompletion
    {
        static readonly Action CallbackCompleted = () => { };

        readonly PipeScheduler _ioScheduler;

        Action _callback;
        int _bytesTransferred;
        SocketError _error;

        public SocketAwaitable(PipeScheduler ioScheduler)
        {
            _ioScheduler = ioScheduler;
        }

        public bool IsCompleted => ReferenceEquals(_callback, CallbackCompleted);

        public SocketAwaitable GetAwaiter() => this;

        public int GetResult()
        {
            Debug.Assert(ReferenceEquals(_callback, CallbackCompleted));

            _callback = null;

            if (_error != SocketError.Success)
            {
                throw new SocketException((int)_error);
            }

            return _bytesTransferred;
        }

        public void OnCompleted(Action continuation)
        {
            if (ReferenceEquals(_callback, CallbackCompleted) ||
                ReferenceEquals(Interlocked.CompareExchange(ref _callback, continuation, null), CallbackCompleted))
            {
                Task.Run(continuation);
            }
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            OnCompleted(continuation);
        }

        public void Complete(int bytesTransferred, SocketError socketError)
        {
            _error = socketError;
            _bytesTransferred = bytesTransferred;

            var continuation = Interlocked.Exchange(ref _callback, CallbackCompleted);

            if (continuation != null)
            {
                _ioScheduler.Schedule(state => ((Action)state)(), continuation);
            }
        }
    }
}