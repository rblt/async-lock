# async-lock
An awaitable semaphore class for the .NET's TPL library which can be used similar to the conventional `lock` block. 

The `lock` block is mimicked by the `using(await ...)`construct, so you can await entering the critical section in your asynchronous method.

#### Solution
Under the hood there are `SemaphoreSlim` instances are being used for synchronization, which is wrapped in a disposable class,
see and build the source code with the given Visual Studio 2015 solution. The tests are using Moq and MSPEC.

#### Usage examples
You can create many `AsyncLock` instances if needed:
```c#
private AsyncLock Lock = new AsyncLock();

var token = new object();

using(await Lock.AcquireAsync(token)) {
  // critical section
}
```

Or use the singleton version across your application:
```c#
using(await AsyncLock.CreateAsync(token)) {
  // critical section
}
```

#### Remarks

You can also use the synchronous version of both examples - without the 'Async' postfixes.
