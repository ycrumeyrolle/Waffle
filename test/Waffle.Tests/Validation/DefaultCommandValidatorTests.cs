namespace Waffle.Tests.Validation
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Waffle;
    using Waffle.Commands;
    using Waffle.Validation;
    using Xunit;

    public sealed class DefaultCommandValidatorTests : IDisposable
    {
        private readonly ProcessorConfiguration configuration = new ProcessorConfiguration();

        [Fact]
        public void WhenValidatingUnvalidatableObjectThenReturnsTrue()
        {
            // Assign
            ICommandValidator validator = new DefaultCommandValidator();
            ICommand command = new UnvalidatableCommand { Property1 = "01234567" };
            CommandHandlerRequest request = new CommandHandlerRequest(this.configuration, command);

            // Act
            bool result = validator.Validate(request);

            // Assert
            Assert.True(result);
            Assert.Equal(0, request.ModelState.Count);
        }

        [Fact]
        public void WhenValidatingValidDataAnnotationsValidatableCommandThenReturnsTrue()
        {
            // Assign
            ICommandValidator validator = new DefaultCommandValidator();
            ICommand command = new DataAnnotationsValidatableCommand { Property1 = "01234567" };
            CommandHandlerRequest request = new CommandHandlerRequest(this.configuration, command);

            // Act
            bool result = validator.Validate(request);

            // Assert
            Assert.True(result);
            Assert.Equal(0, request.ModelState.Count);
        }

        [Fact]
        public void WhenValidatingInvalidDataAnnotationsValidatableCommandThenReturnsFalse()
        {
            // Assign
            ICommandValidator validator = new DefaultCommandValidator();
            ICommand command = new DataAnnotationsValidatableCommand { Property1 = "01234567890123456789" };
            CommandHandlerRequest request = new CommandHandlerRequest(this.configuration, command);

            // Act
            bool result = validator.Validate(request);

            // Assert
            Assert.False(result);
            Assert.Equal(1, request.ModelState.Count);
        }
        
        [Fact]
        public void WhenValidatingValidValidatableObjectCommandThenReturnsTrue()
        {
            // Assign
            ICommandValidator validator = new DefaultCommandValidator();
            ICommand command = new ValidatableObjectCommand(true);
            CommandHandlerRequest request = new CommandHandlerRequest(this.configuration, command);

            // Act
            bool result = validator.Validate(request);

            // Assert
            Assert.True(result);
            Assert.Equal(0, request.ModelState.Count);
        }

        [Fact]
        public void WhenValidatingInvalidValidatableObjectCommandThenReturnsFalse()
        {
            // Assign
            ICommandValidator validator = new DefaultCommandValidator();
            ICommand command = new ValidatableObjectCommand(false);
            CommandHandlerRequest request = new CommandHandlerRequest(this.configuration, command);

            // Act
            bool result = validator.Validate(request);

            // Assert
            Assert.False(result);
            Assert.Equal(2, request.ModelState.Sum(kvp => kvp.Value.Errors.Count));
        }

        [Fact]
        public void WhenValidatingValidMixedValidatableCommandThenReturnsTrue()
        {
            // Assign
            ICommandValidator validator = new DefaultCommandValidator();
            MixedValidatableCommand command = new MixedValidatableCommand(true);
            command.Property1 = "123456";
            CommandHandlerRequest request = new CommandHandlerRequest(this.configuration, command);

            // Act
            bool result = validator.Validate(request);

            // Assert
            Assert.True(result);
            Assert.Equal(0, request.ModelState.Count);
        }

        [Fact]
        public void WhenValidatingInvalidMixedValidatableCommandThenReturnsFalse()
        {
            // Assign
            ICommandValidator validator = new DefaultCommandValidator();
            MixedValidatableCommand command = new MixedValidatableCommand(false);
            command.Property1 = "123456789456132456";
            CommandHandlerRequest request = new CommandHandlerRequest(this.configuration, command);

            // Act
            bool result = validator.Validate(request);

            // Assert
            Assert.False(result);

            // Validator ignore IValidatableObject validation until DataAnnotations succeed.
            Assert.Equal(1, request.ModelState.Count);
        }
        
        [Fact]
        public void WhenValidatingCommandWithUriThenReturnsTrue()
        {
            // Assign
            ICommandValidator validator = new DefaultCommandValidator();
            CommandWithUriProperty command = new CommandWithUriProperty();
            command.Property1 = new Uri("/test/values", UriKind.Relative);
            CommandHandlerRequest request = new CommandHandlerRequest(this.configuration, command);

            // Act
            bool result = validator.Validate(request);

            // Assert
            // A lots of properties of Uri throw exceptions but its still valid
            Assert.True(result);
            Assert.Equal(0, request.ModelState.Count);
        }

        private class UnvalidatableCommand : ICommand
        {
            public string Property1 { get; set; }
        }

        private class DataAnnotationsValidatableCommand : ICommand
        {
            [StringLength(15)]
            public string Property1 { get; set; }
        }

        private class ValidatableObjectCommand : ICommand, IValidatableObject
        {
            private readonly bool valid;

            public ValidatableObjectCommand(bool valid)
            {
                this.valid = valid;
            }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (this.valid)
                {
                    yield break;
                }

                yield return new ValidationResult("Error 1");
                yield return new ValidationResult("Error 2");
            }
        }

        private class MixedValidatableCommand : ICommand, IValidatableObject
        {
            private readonly bool valid;

            public MixedValidatableCommand(bool valid)
            {
                this.valid = valid;
            }

            [StringLength(15)]
            public string Property1 { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (this.valid)
                {
                    yield break;
                }

                yield return new ValidationResult("Error 1");
                yield return new ValidationResult("Error 2");
            }
        }

        private class CommandWithUriProperty : ICommand
        {
            [Required]
            public Uri Property1 { get; set; }
        }

        public void Dispose()
        {
            this.configuration.Dispose();
        }
    }
}