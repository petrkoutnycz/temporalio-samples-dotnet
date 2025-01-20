using Temporalio.Common;
using Temporalio.Workflows;

namespace TemporalioSamples.OpenTelemetry.Common;

[Workflow]
public class WorkflowWithCustomId
{
    public static readonly SearchAttributeKey<string> CustomId = SearchAttributeKey.CreateKeyword(nameof(CustomId));

    [WorkflowRun]
    public async Task<string> RunAsync()
    {
        // do something
        await Workflow.ExecuteActivityAsync(
            () => Activities.MyActivity("input"),
            new() { StartToCloseTimeout = TimeSpan.FromMinutes(5), });

        // update search attribute
        Workflow.UpsertTypedSearchAttributes(SearchAttributeUpdate.ValueSet(CustomId, "123456"));

        return "complete!";
    }
}