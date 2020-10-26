using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncWorkQueue
{
	public class AsyncQueue<T>
	{
		private readonly Queue<T> queue;

		private readonly SemaphoreSlim semaphore;

		/// <summary>
		/// Completed only if there is some items in queue.
		/// </summary>
		private volatile TaskCompletionSource<bool> tcs;

		public AsyncQueue()
		{
			this.queue = new Queue<T>();
			this.semaphore = new SemaphoreSlim(1, 1);
			this.tcs = new TaskCompletionSource<bool>();
		}

		public async Task Enqueue(T item, CancellationToken cancellationToken = default)
		{
			await this.semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

			try
			{
				this.queue.Enqueue(item);
				this.tcs.TrySetResult(true);
			}
			finally
			{
				this.semaphore.Release();
			}
		}

		public async Task<T> DequeueWhenAvailable(CancellationToken cancellationToken = default)
		{
			wait:
			
			// Wait for items in queue
			// No lock because there is always a complete task or new instance
			await this.tcs.Task.ConfigureAwait(false);
			
			// lock on queue
			await this.semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

			T val;
			
			try
			{
				if(this.queue.Count == 0)
				{
					// Some other caller already consumed item, waiting for another one
					goto wait;
				}
				
				val = this.queue.Dequeue();

				// Reset 'tsc' if ran out of items
				if(this.queue.Count == 0)
				{
					this.tcs = new TaskCompletionSource<bool>();
				}
			}
			finally
			{
				this.semaphore.Release();
			}

			return val;
		}

		public async Task<Option<T>> TryDequeue(CancellationToken cancellationToken = default)
		{
			T val;
			
			// lock on queue
			await this.semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
			
			try
			{
				if(!this.queue.TryDequeue(out val))
				{
					return Option<T>.None;
				}

				// Reset 'tsc' if ran out of items
				if(this.queue.Count == 0)
				{
					this.tcs = new TaskCompletionSource<bool>();
				}
			}
			finally
			{
				this.semaphore.Release();
			}

			return Option<T>.Some(val);
		}
	}
}