# AI Tools Reflection

1. **Claude – Architecture and Design**: Claude was more useful when dealing with high-level design and architectural decisions. When I moved the order validation logic to the Strategy pattern, it suggested an `IOrderValidationStrategy` interface and injecting `IEnumerable<IOrderValidationStrategy>` into `OrderService`.
**Observation**: This allowed multiple validation rules to run independently without putting all the logic inside one service. Claude did suggest using reflection to automatically discover and register the strategies, but I considered that unnecessarily complex.
**Decision**: I rejected the reflection-based approach and used explicit DI registration in `Program.cs` instead.

2. **Copilot – Boilerplate and Testing**: Copilot was particularly helpful for generating repetitive code and unit tests. For example, the comment `// Test: validation rejects orders with negative quantity` generated a complete xUnit test with the expected Arrange, Act, and Assert structure.
**Observation**: The generated test was not completely reliable because Copilot checked for a generic `Exception` instead of the specific `ValidationException` expected from the validation strategy.
**Risk**: If left unchanged, an unrelated exception such as a `NullReferenceException` could cause the test to pass, potentially hiding an actual bug.

3. **Production Debugging Approach**: If I had to troubleshoot a production issue late at night, I would use Claude first to analyze the logs and stack traces and help identify the underlying architectural or logical problem.
**Fix**: Once the root cause was understood, I would use Copilot inside the IDE to quickly implement the required localized fix.

4. **Overall Comparison**: Claude is better suited for architectural reasoning, design decisions, and understanding complex problems, while Copilot is more effective for quickly producing implementation details and boilerplate code.
**Takeaway**: Neither tool should be treated as an authority. Generated code still needs to be reviewed, tested, and validated by the developer to ensure that its assumptions and implementation are correct.
