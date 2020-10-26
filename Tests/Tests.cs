using System.Collections.Generic;
using System.Threading.Tasks;
using AsyncWorkQueue;
using NUnit.Framework;

namespace Tests
{
	[TestFixture]
	public class Tests
	{
		[Test]
		public async Task Test()
		{
			var queue = new AsyncQueue<int>();
			var tasks = new List<Task>();

			for(var i = 0; i < 1000; i++)
			{
				tasks.Add(Task.Run(async () =>
				{
					for(var j = 0; j < 100; j++)
					{
						await queue.DequeueWhenAvailable();
					}
				}));

				tasks.Add(Task.Run(async () =>
				{
					for(var j = 0; j < 100; j++)
					{
						await queue.Enqueue(j);
					}
				}));
			}

			await Task.WhenAll(tasks);
			Assert.Pass();
		}
	}
}