Waffle
================
This command processor aims to implement the command pattern in an extensible way. 


Features
========
* Command validation, based on DataAnnotations and IValidatableObject
* Command handling, with children handlers possiblity
* Fully configurable dependency scope management (default implementation with Unity)
* Asynchronous execution
* Handler filtering to wrap generic behaviour
* Result caching possibilities
* Transactionnal execution possibilities
* Events messaging possibilities
* Event sourcing (default implementation with MongoDB)
* Retry capability with policy configuration

Usage
=====
```C#
// Command without return value
var command = new SendEmailCommand
{
  From = "address@email.com",
  To = "to_address@email.com",
  Subject = "Hello !",
  Message = "Hello World !"
};

// processor should be a singleton for better performance
using (CommandProcessor processor = new CommandProcessor())
{
  // The processor will find the handler and delegates to it the action
  processor.Process(command);
}
```

```C#
// Command with return value
var command = new ComputePricingCommand
{
  Quantity = 12  
};

// processor should be a singleton for better performance
using (CommandProcessor processor = new CommandProcessor())
{
  // The processor will find the handler and delegates to it the action 
  Pricing result = processor.Process<Pricing>(command);
  Console.WriteLine("The pricing is : " + result.Amount);
}
```

