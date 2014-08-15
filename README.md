#Waffle
[![Build status](https://ci.appveyor.com/api/projects/status/gdk39oc6q617r8y2)](https://ci.appveyor.com/project/ycrumeyrolle/waffle)

*A CQRS framework flipping system.*

This command processor aims to implement the command pattern in an extensible way. 


##Features

* Command validation, based on DataAnnotations and IValidatableObject
* Command handling, with children handlers possiblity
* Fully configurable dependency scope management (default implementation with Unity)
* Asynchronous execution
* Handler filtering to wrap generic behaviour
* Transactionnal execution possibilities
* Events messaging possibilities
* Event sourcing (default implementation with MongoDB)
* Retry capability with policy configuration

Usage
=====

```C#

    // The send email command
    public class SendEmailCommand : ICommand
    {
        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string From { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string To { get; set; }

        [Required]
        [StringLength(256)]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }
    }

    // The sent email event
    public class EmailSent : IEvent
    {
        public EmailSent(string from, string to, string subject)
        {
            this.From = from;
            this.To = to;
            this.Subject = subject;
        }   

        public string From { get; set; }

        public string To { get; set; }

        public string Subject { get; set; }
    }

    // Command without return value
    var command = new SendEmailCommand
    {
      From = "address@email.com",
      To = "to_address@email.com",
      Subject = "Hello !",
      Message = "Hello World !"
    };
    
    // processor should be a singleton for better performance
    using (CommandProcessor processor = new MessageProcessor())
    {
      // The processor will find the handler and delegates to it the action
      await processor.ProcessAsync(command);
    }

    public class EmailSender : MessageHandler, 
        IAsyncCommandHandler<SendEmailCommand>
    {
        public Task HandleAsync(SendEmailCommand command)
        {
            // Email sending logic here.

            // Sent an event to indicate that the email was sent.
            EmailSent emailSent = new EmailSent(command.From, command.To, command.Subject);
            return this.CommandContext.Request.Processor.PublishAsync(orderCreated);
        }
    }

```

