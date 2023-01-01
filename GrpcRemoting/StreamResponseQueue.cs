using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Grpc.Core;
#if NETSTANDARD2_0
using Open.ChannelExtensions;
#endif
using Channel = System.Threading.Channels.Channel;

namespace GrpcRemoting
{
	/// <summary>
	/// Wraps <see cref="IServerStreamWriter{T}"/> which only supports one writer at a time.
	/// This class can receive messages from multiple threads, and writes them to the stream
	/// one at a time.
	/// 
	/// fixes System.InvalidOperationException: 'Only one write can be pending at a time
	/// https://github.com/grpc/grpc-dotnet/issues/579
	/// https://github.com/grpc/grpc-dotnet/issues/579#issuecomment-574056565
	/// 
	/// </summary>
	/// <typeparam name="T">Type of message written to the stream</typeparam>
	public class StreamResponseQueue<T>
	{
		private readonly IServerStreamWriter<T> _stream;
		private readonly Task _consumer;

		private readonly Channel<T> _channel = Channel.CreateUnbounded<T>(
			new UnboundedChannelOptions
			{
				SingleWriter = false,
				SingleReader = true,
			});

		public StreamResponseQueue(
			IServerStreamWriter<T> stream,
			CancellationToken cancellationToken = default
		)
		{
			_stream = stream;
			_consumer = Consume(cancellationToken);
		}

		/// <summary>
		/// Asynchronously writes an item to the channel.
		/// </summary>
		/// <param name="message">The value to write to the channel.</param>
		/// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> used to cancel the write operation.</param>
		/// <returns>A <see cref="T:System.Threading.Tasks.ValueTask" /> that represents the asynchronous write operation.</returns>
		public async ValueTask WriteAsync(T message, CancellationToken cancellationToken = default)
		{
			await _channel.Writer.WriteAsync(message, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Marks the writer as completed, and waits for all writes to complete.
		/// </summary>
		public Task CompleteAsync()
		{
			_channel.Writer.Complete();
			return _consumer;
		}

		private async Task Consume(CancellationToken cancellationToken)
		{

#if NETSTANDARD2_0
            // Using Open.ChannelExtensions since ReadAllAsync not available in netstandard 2.0
            // ValueTask confusion here...
            var _ = await _channel.Reader.ReadAllAsync(cancellationToken, msg => new ValueTask(_stream.WriteAsync(msg))).ConfigureAwait(false);
#else

			await foreach (var message in _channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
			{
				await _stream.WriteAsync(message).ConfigureAwait(false);
			}

#endif
        }
    }
}
