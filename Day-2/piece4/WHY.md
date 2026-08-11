# Rich Domain Model vs. Anemic Domain Model

An anemic domain model is mostly a collection of properties with public getters and setters. The actual business logic is usually placed in controllers or services. As the application grows, this can result in repeated validation logic and make it easier for different parts of the application to create invalid data.

A rich domain model takes a different approach by keeping the data and the rules that control it together. For the `Quote` entity, `Quote.Create()` is the controlled way to create a quote, while private setters prevent its important values from being changed incorrectly. This allows the entity to protect its own state.

**Example of a Bug Avoided:**

Consider a new bulk-import feature created by another developer. With an anemic model, they could simply call `new Quote()` and create quotes without going through the validation implemented in the controller. They could also change `Text` later because the property has a public setter. This could result in invalid or modified quote data being saved to the database.

The rich model prevents these problems by controlling how quotes are created and modified. `Quote.Create()` checks the required author and text lengths, while `Text` cannot be changed after creation. As a result, the business rules are enforced consistently regardless of where the quote is created.
