using Microsoft.Extensions.Logging;
using Temporalio.Api.OperatorService.V1;
using Temporalio.Client;
using Temporalio.Worker;
using TemporalioSamples.ActivitySimple;

// Create a client to localhost on default namespace
var client = await TemporalClient.ConnectAsync(new("localhost:7233")
{
    LoggerFactory = LoggerFactory.Create(builder =>
        builder.
            AddSimpleConsole(options => options.TimestampFormat = "[HH:mm:ss] ").
            SetMinimumLevel(LogLevel.Information)),
});

async Task RunWorkerAsync()
{
    // Cancellation token cancelled on ctrl+c
    using var tokenSource = new CancellationTokenSource();
    Console.CancelKeyPress += (_, eventArgs) =>
    {
        tokenSource.Cancel();
        eventArgs.Cancel = true;
    };

    // Create an activity instance with some state
    var activities = new MyActivities();

    // Run worker until cancelled
    Console.WriteLine("Running worker");
    using var worker = new TemporalWorker(
        client,
        new TemporalWorkerOptions(taskQueue: "activity-simple-sample").
            AddActivity(activities.SelectFromDatabaseAsync).
            AddActivity(MyActivities.DoStaticThing).
            AddWorkflow<MyWorkflow>());
    try
    {
        await worker.ExecuteAsync(tokenSource.Token);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Worker cancelled");
    }
}

async Task ExecuteWorkflowAsync()
{
    // make sure the keyword list is created
    try
    {
        await client.OperatorService.AddSearchAttributesAsync(new AddSearchAttributesRequest()
        {
            Namespace = "default",
            SearchAttributes = { { MySearchAttributes.PetNames.Name, MySearchAttributes.PetNames.ValueType } }
        });
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }

    Console.WriteLine("Executing workflow");
    await client.ExecuteWorkflowAsync(
        (MyWorkflow wf) => wf.RunAsync(),
        new(id: "activity-simple-workflow-id", taskQueue: "activity-simple-sample"));

    // try to get the keyword list
    var handle = client.GetWorkflowHandle("activity-simple-workflow-id");
    var description = await handle.DescribeAsync();

    // the value is internally a List<object> instead of List<string> so it returns false
    if (description.TypedSearchAttributes.TryGetValue(MySearchAttributes.PetNames, out var petNames))
    {
        Console.WriteLine($"Number of pets: {petNames.Count}");
    }
    else
    {
        throw new InvalidOperationException("Pet names were not found!");
    }
}

switch (args.ElementAtOrDefault(0))
{
    case "worker":
        await RunWorkerAsync();
        break;
    case "workflow":
        await ExecuteWorkflowAsync();
        break;
    default:
        throw new ArgumentException("Must pass 'worker' or 'workflow' as the single argument");
}