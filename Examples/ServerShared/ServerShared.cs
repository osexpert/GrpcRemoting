

using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace ServerShared
{
    interface ITestService
    {
        string Echo(string s);
        void TestProgress(Action<string> progress);
        Task GetMessages(Action<string> message);
        void CompleteGetMessages();
        void SendMessage(string mess);
        void GetFile(string file, Action<byte[], int, int> write, Action<string> progress);
        void SendFile(string file, Func<int, (byte[], int, int)> read, Action<string> progress);
    }

    class TestService : ITestService
    {
        static ConcurrentDictionary<Guid, MessageGetters> pMessageGetters = new ConcurrentDictionary<Guid, MessageGetters>();

        private Guid pSessionID;

        public TestService(Guid sessionID)
        {
            pSessionID = sessionID;
        }

        public void TestProgress(Action<string> progress)
        {
            Task.Run(() =>
            {
                for (int i = 0; i < 1000; i++)
                    progress("hi");
            });
            Task.Run(() =>
            {
                for (int i = 0; i < 1000; i++)
                    progress("hello");
            });
            Task.Run(() =>
            {
                for (int i = 0; i < 1000; i++)
                    progress("bye");
            });

            Thread.Sleep(1000);
        }

        public string Echo(string s)
        {
            Console.WriteLine("Enter Echo: " + s);
            return s;
        }

        public void CompleteGetMessages()
        {
            if (pMessageGetters.TryGetValue(pSessionID, out var mg))
                mg.Completed.SetResult(true);
        }

        public async Task GetMessages(Action<string> message)
        {
            var mg = new MessageGetters { sessionID = pSessionID, message = message };

            if (pMessageGetters.TryAdd(pSessionID, mg))
            {
                await mg.Completed.Task;
                pMessageGetters.TryRemove(pSessionID, out _);
            }
        }

        public void SendMessage(string mess)
        {
            foreach (var mg in pMessageGetters.Values)
                mg.message(mess);
        }

        public void GetFile(string file, Action<byte[], int, int> write, Action<string> progress)
        {
            progress("hi");

            var sw = new WriteStreamWrapper(write);
            using (var f = File.OpenRead(file))
                f.CopyTo(sw, 1024 * 1024);
        }

        public void SendFile(string file, Func<int, (byte[], int, int)> read, Action<string> progress)
        {
            progress("hello");

            var sr = new ReadStreamWrapper(read);
            using (var f = File.OpenWrite(file))
                sr.CopyTo(f, 1024 * 1024);

            // Alternative to using the stream wrapper, it will avoid the buffer copy but does not help much for performance:
            //using (var f = File.OpenWrite(file))
            //{
            //    while (true)
            //    {
            //        (var data, var off, var len) = read(1024 * 1204);
            //        if (len > 0)
            //            f.Write(data, off, len);
            //        else
            //            break;
            //    }

            //}
        }
    }

    class MessageGetters
    {
        public Action<string> message;
        public Guid sessionID;
        public TaskCompletionSource<bool> Completed = new TaskCompletionSource<bool>();
    }

    class WriteStreamWrapper : Stream
    {
        Action<byte[], int, int> pChunk;

        public WriteStreamWrapper(Action<byte[], int, int> chunk)
        {
            pChunk = chunk;
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotImplementedException();
        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override void Flush()
        {
            //throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            pChunk(buffer, offset, count);
        }
    }

    class ReadStreamWrapper : Stream
    {
        Func<int, (byte[], int, int)> pChunk;


        public ReadStreamWrapper(Func<int, (byte[], int, int)> chunk)
        {
            pChunk = chunk;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotImplementedException();
        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush()
        {
            //throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var res = pChunk(count);

            if (res.Item3 > 0)
                Buffer.BlockCopy(res.Item1, res.Item2, buffer, offset, res.Item3);

            return res.Item3;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
