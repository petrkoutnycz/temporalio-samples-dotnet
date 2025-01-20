using Temporalio.Extensions.OpenTelemetry;
using Temporalio.Workflows;

namespace TemporalioSamples.OpenTelemetry.Common;

public class CustomTracingInterceptor : TracingInterceptor
{
    protected override IEnumerable<KeyValuePair<string, object?>> CreateInWorkflowTags()
    {
        var tags = base.CreateInWorkflowTags();

        if (Workflow.TypedSearchAttributes.TryGetValue(WorkflowWithCustomId.CustomId, out var customId))
        {
            tags = tags.Append(new("my.custom.id", customId));
        }

        return tags;
    }
}