# AI Usage Summary

## Key AI Interactions
1. **Prompt**: How to structure xUnit tests with EF Core In-Memory database for a .NET service?
   - **Suggestion**: Provided a test fixture pattern using `DbContextOptionsBuilder` and unique database names per test instance.
   - **Action**: Accepted and implemented.

2. **Prompt**: Resolving compiler error regarding missing primary constructor arguments for DTO records.
   - **Suggestion**: Pointed out that positional records require constructor syntax `new Dto(arg1, arg2)` instead of object initializer curly braces `{ Property = value }`.
   - **Action**: Accepted and updated all DTO instantiations in tests.

3. **Prompt**: Fixing `TransactionIgnoredWarning` exception during unit testing.
   - **Suggestion**: Add `.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))` to the DbContext options builder.
   - **Action**: Accepted, resolving all transaction-related test failures.

## Three Things AI Got Wrong
1. **Syntax Assumption**: Initially suggested standard object initializer syntax (`{ Field = val }`) for the C# records even though they were defined with positional primary constructors (`record Dto(Type Field)`).
2. **Transaction Scope Oversight**: Initially missed that the service implementation called database transactions, which caused runtime exceptions against the In-Memory provider until explicitly configured to ignore the warning.
3. **Package Version Compatibility**: Recommended older package version ranges that conflicted with .NET 10 SDK defaults, requiring adjustments to align with current package feeds.