# Refactoring Notes

1. **Missing Cancellation Tokens**: Database queries continue consuming resources even if the client terminates the request early.
**Fix**: Pass a `CancellationToken` from the API endpoint down through to Entity Framework.

2. **Tight Coupling (DbContext)**: Instantiating `AppDbContext` directly heavily couples the controller to the database layer.
**Fix**: Inject `IOrderRepository` via constructor DI.

3. **Hardcoded Time**: Relying on `DateTime.Now` makes unit testing time-sensitive logic brittle and difficult.
**Fix**: Inject .NET's `TimeProvider` or hide time generation behind a custom service interface.

4. **Sync-over-Async**: Calling synchronous `SaveChanges()` within an async method unnecessarily blocks the executing thread.
**Fix**: Switch to `SaveChangesAsync(CancellationToken)`.

5. **SRP Violation (God Controller)**: The controller does too much by managing data access, business rules, and validation simultaneously.
**Fix**: Split responsibilities into `OrderController`, `IOrderService`, and `IOrderRepository`.

6. **Swallowed Exceptions**: Empty `try/catch` blocks silently suppress errors, masking bugs and making debugging difficult.
**Fix**: Remove empty `catch` blocks. Let global exception middleware handle unexpected errors, or catch specific exceptions and return structured `ProblemDetails`.

7. **Untyped Return Values**: Returning anonymous objects such as `{ Error = ... }` leads to messy and unpredictable API contracts.
**Fix**: Return a strongly typed `ActionResult<CreateOrderResponse>`.

8. **Untyped Payloads**: Using `[FromBody] dynamic` bypasses compile-time type safety and negatively affects Swagger/OpenAPI generation.
**Fix**: Create a strongly typed `CreateOrderRequest` DTO.

9. **Off-by-One Error**: A loop using `i <= payload.Items.Count` will eventually cause an `IndexOutOfRangeException`.
**Fix**: Use `foreach` or LINQ such as `.Sum()`.

10. **Null Dereference Risk**: Accessing `payload.Customer.Name` will throw an exception if `Customer` is null.
**Fix**: Perform proper DTO validation before mapping or accessing nested properties.