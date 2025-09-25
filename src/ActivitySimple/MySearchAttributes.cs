using Temporalio.Common;

namespace TemporalioSamples.ActivitySimple;

public static class MySearchAttributes
{
    public static SearchAttributeKey<IReadOnlyCollection<string>> PetNames = SearchAttributeKey.CreateKeywordList("PetNames");
}