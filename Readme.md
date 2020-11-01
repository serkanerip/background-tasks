# Features

* Adding instant task.
* Adding scheduled job.
* Listing failed jobs.
* Retrying failed job.

# Usage

## To Add Instant Job

```c#
BackgroundJob.AddJob(() =>
{
    Console.WriteLine("Instant job");
});
```

## To Add Schedule Job

```c#
BackgroundJob.Schedule(() =>
{
    Console.WriteLine("This is a scheduled job!");
}, TimeSpan.FromMinutes(10));
```