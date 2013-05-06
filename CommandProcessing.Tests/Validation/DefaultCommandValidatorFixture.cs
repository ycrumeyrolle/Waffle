namespace CommandProcessing.Tests.Validation
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using CommandProcessing;
    using CommandProcessing.Validation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DefaultCommandValidatorFixture
    {
        private readonly ProcessorConfiguration configuration = new ProcessorConfiguration();

        [TestMethod]
        public void WhenValidatingUnvalidatableObjectThenReturnsTrue()
        {
            // Assign
            ICommandValidator validator = new DefaultCommandValidator();
            Command command = new UnvalidatableCommand { Property1 = "01234567" };
            HandlerRequest request = new HandlerRequest(this.configuration, command);

            // Act
            bool result = validator.Validate(request);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(command.IsValid);
        }

        [TestMethod]
        public void WhenValidatingValidDataAnnotationsValidatableCommandThenReturnsTrue()
        {
            // Assign
            ICommandValidator validator = new DefaultCommandValidator();
            Command command = new DataAnnotationsValidatableCommand { Property1 = "01234567" };
            HandlerRequest request = new HandlerRequest(this.configuration, command);

            // Act
            bool result = validator.Validate(request);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(command.IsValid);
            Assert.AreEqual(0, command.ModelState.Count);
        }

        [TestMethod]
        public void WhenValidatingInvalidDataAnnotationsValidatableCommandThenReturnsFalse()
        {
            // Assign
            ICommandValidator validator = new DefaultCommandValidator();
            Command command = new DataAnnotationsValidatableCommand { Property1 = "01234567890123456789" };
            HandlerRequest request = new HandlerRequest(this.configuration, command);

            // Act
            bool result = validator.Validate(request);

            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(command.IsValid);
            Assert.AreEqual(1, command.ModelState.Count);
        }
        
        [TestMethod]
        public void WhenValidatingValidValidatableObjectCommandThenReturnsTrue()
        {
            // Assign
            ICommandValidator validator = new DefaultCommandValidator();
            Command command = new ValidatableObjectCommand(true);
            HandlerRequest request = new HandlerRequest(this.configuration, command);

            // Act
            bool result = validator.Validate(request);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(command.IsValid);
            Assert.AreEqual(0, command.ModelState.Count);
        }

        [TestMethod]
        public void WhenValidatingInvalidValidatableObjectCommandThenReturnsFalse()
        {
            // Assign
            ICommandValidator validator = new DefaultCommandValidator();
            Command command = new ValidatableObjectCommand(false);
            HandlerRequest request = new HandlerRequest(this.configuration, command);

            // Act
            bool result = validator.Validate(request);

            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(command.IsValid);
            Assert.AreEqual(2, command.ModelState.Sum(kvp => kvp.Value.Errors.Count));
        }

        [TestMethod]
        public void WhenValidatingValidMixedValidatableCommandThenReturnsTrue()
        {
            // Assign
            ICommandValidator validator = new DefaultCommandValidator();
            MixedValidatableCommand command = new MixedValidatableCommand(true);
            command.Property1 = "123456";
            HandlerRequest request = new HandlerRequest(this.configuration, command);

            // Act
            bool result = validator.Validate(request);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(command.IsValid);
            Assert.AreEqual(0, command.ModelState.Count);
        }

        [TestMethod]
        public void WhenValidatingInvalidMixedValidatableCommandThenReturnsFalse()
        {
            // Assign
            ICommandValidator validator = new DefaultCommandValidator();
            MixedValidatableCommand command = new MixedValidatableCommand(false);
            command.Property1 = "123456789456132456";
            HandlerRequest request = new HandlerRequest(this.configuration, command);

            // Act
            bool result = validator.Validate(request);

            // Assert
            Assert.IsFalse(result);

            // Validator ignore IValidatableObject validation until DataAnnotations succeed.
            Assert.IsFalse(command.IsValid);
            Assert.AreEqual(1, command.ModelState.Count);
        }


        private class UnvalidatableCommand : Command
        {
            public string Property1 { get; set; }
        }

        private class DataAnnotationsValidatableCommand : Command
        {
            [StringLength(15)]
            public string Property1 { get; set; }
        }
        
        private class ValidatableObjectCommand : Command, IValidatableObject
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

        private class MixedValidatableCommand : Command, IValidatableObject
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
    }
}