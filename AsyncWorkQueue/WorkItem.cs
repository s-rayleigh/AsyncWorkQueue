using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncWorkQueue
{
	public class WorkItem
	{
		internal readonly Func<Task> action;
	
		internal readonly Func<WorkItem, Task> success;
	
		internal readonly Func<WorkItem, Exception, Task> fail;

		internal readonly Func<WorkItem, Task> cancelled;

		// NOTE: item must be idempotent to use this feature
		public bool Retry { get; }

		internal byte retries;
		
		internal CancellationToken cancellationToken;

		internal Exception ex;

		public bool Used { get; internal set; }

		internal WorkItem() { }

		public WorkItem(Func<Task> action, Func<WorkItem, Task> success = null,
			Func<WorkItem, Exception, Task> fail = null, Func<WorkItem, Task> cancelled = null, bool retry = false,
			byte retries = 5, CancellationToken cancellationToken = default)
		{
			this.action = action;
			this.success = success;
			this.fail = fail;
			this.cancelled = cancelled;
			this.Retry = retry;
			this.retries = retries;
			this.cancellationToken = cancellationToken;
		}
	}
}