using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncWorkQueue
{
	public class AsyncWorkQueue
	{
		private readonly AsyncQueue<WorkItem> queue;

		public AsyncWorkQueue(CancellationToken stopToken = default)
		{
			this.queue = new AsyncQueue<WorkItem>();

			Task.Run(async () =>
			{
				while(true)
				{
					stopToken.ThrowIfCancellationRequested();
					
					var item = await this.queue.DequeueWhenAvailable(stopToken);
					item.Used = true;
					
					var cancellationToken = CancellationTokenSource
						.CreateLinkedTokenSource(stopToken, item.cancellationToken).Token;

					try
					{
						// Do work on ThreadPool
						_ = Task.Run(async () =>
						{
							if(cancellationToken.IsCancellationRequested)
							{
								if(item.cancelled != null)
								{
									await item.cancelled(item).ConfigureAwait(false);
								}

								return;
							}
							
							try
							{
								await item.action().ConfigureAwait(false);
							}
							catch(TaskCanceledException)
							{
								if(item.cancelled != null)
								{
									await item.cancelled(item).ConfigureAwait(false);
								}
							}
							catch(Exception e)
							{
								item.ex = e;

								if(item.Retry && item.retries > 0)
								{
									item.retries--;

									try
									{
										// Enqueue item for retry
										item.Used = false;
										await this.queue.Enqueue(item, cancellationToken).ConfigureAwait(false);
									}
									catch
									{
										// ignored
									}
								}
								else if(item.fail != null)
								{
									try
									{
										await item.fail(item, e).ConfigureAwait(false);
									}
									catch
									{
										// ignored
									}
								}

								return;
							}

							if(item.success != null)
							{
								try
								{
									await item.success(item).ConfigureAwait(false);
								}
								catch
								{
									// ignored
								}
							}
						}, cancellationToken);
					}
					catch(TaskCanceledException) { }
				}
			}, stopToken);
		}
		
		public Task EnqueueAsync(WorkItem item, CancellationToken cancellationToken = default)
		{
			if(item is null)
			{
				throw new ArgumentNullException(nameof(item));
			}
			
			if(item.Used)
			{
				throw new ArgumentException("Work item already used.", nameof(item));
			}
			
			return this.queue.Enqueue(item, cancellationToken);
		}
	}
}