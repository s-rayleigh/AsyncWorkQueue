# AsyncWorkQueue

Asynchronous work queue allows to enqueue some work to the background without using 'fire-and-forget' and allows to do
an error handling with retry on fail. Under the hood it uses the thread pool. 

Please note that queue guarantee only call order for work items that means it can execute new work item even if
execution of the previous is not over.

To use the retry feature work item must be idempotent.

## Usage
```c#
var queue = new WorkQueue();
await queue.EnqueueAsync(new WorkItem(async () => { ... }));
```

## TODO
- [ ] Implement `IAsyncEnumerable` for `AsyncQueue`
- [ ] Proxy `Task` object for background work tracking
- [ ] Global async event handlers for `success`, `error` and `cancelled` events
- [ ] Builder for `WorkItem`
- [ ] `Clone` method for `WorkItem`