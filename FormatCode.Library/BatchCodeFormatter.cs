using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FormatCode.Library;

public static class BatchCodeFormatter {
	public static void BeginRun(CodeFormatter formatter, IEnumerable<string> paths, Action<int> onProgress, Action<string> onComplete) {
		void Run() {
			object syncObj = new();
			int processedCount = 0;
			TimeSpan progressUpdateInterval = TimeSpan.FromMilliseconds(15);
			DateTime nextProgressUpdateTime = DateTime.UtcNow;
			void Process(string path) {
				try {
					formatter.Format(path);
				}
				catch (Exception ex) {
					throw new Exception($"{path}\r\n\r\n{ex.Message}");
				}
				lock (syncObj) {
					processedCount++;
					DateTime timeNow = DateTime.UtcNow;
					if (timeNow >= nextProgressUpdateTime) {
						onProgress(processedCount);
						nextProgressUpdateTime = timeNow + progressUpdateInterval;
					}
				}
			}

			string errorMessage = null;
			try {
				Parallel.ForEach(paths, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, Process);
			}
			catch (AggregateException ae) {
				errorMessage = ae.InnerException?.Message ?? "(Empty AggregateException)";
			}
			onProgress(processedCount);
			onComplete(errorMessage);
		}
		new Thread(Run).Start();
	}
}
