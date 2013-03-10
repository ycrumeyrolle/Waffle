namespace CommandProcessing.Tests.Validation
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;
    using CommandProcessing;
    using CommandProcessing.Dispatcher;
    using CommandProcessing.Validation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DefaultCommandValidatorFixture
    {
        [TestMethod]
        public void WhenValidatingUnvalidatableObjectThenReturnsTrue()
        {
            // Assign
            ICommandValidator validator = new DefaultCommandValidator();
            Command command = new UnvalidatableCommand { Property1 = "01234567" };

            // Act
            bool result = validator.Validate(command);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void WhenValidatingValidDataAnnotationsValidatableCommandThenReturnsTrue()
        {
            // Assign
            ICommandValidator validator = new DefaultCommandValidator();
            Command command = new DataAnnotationsValidatableCommand { Property1 = "01234567" };

            // Act
            bool result = validator.Validate(command);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(0, command.ValidationResults.Count);
        }

        [TestMethod]
        public void WhenValidatingInvalidDataAnnotationsValidatableCommandThenReturnsFalse()
        {
            // Assign
            ICommandValidator validator = new DefaultCommandValidator();
            Command command = new DataAnnotationsValidatableCommand { Property1 = "01234567890123456789" };

            // Act
            bool result = validator.Validate(command);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(1, command.ValidationResults.Count);
        }
        
        [TestMethod]
        public void WhenValidatingValidValidatableObjectCommandThenReturnsTrue()
        {
            // Assign
            ICommandValidator validator = new DefaultCommandValidator();
            Command command = new ValidatableObjectCommand(true);

            // Act
            bool result = validator.Validate(command);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(0, command.ValidationResults.Count);
        }

        [TestMethod]
        public void WhenValidatingInvalidValidatableObjectCommandThenReturnsFalse()
        {
            // Assign
            ICommandValidator validator = new DefaultCommandValidator();
            Command command = new ValidatableObjectCommand(false);

            // Act
            bool result = validator.Validate(command);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(2, command.ValidationResults.Count);
        }

        [TestMethod]
        public void WhenValidatingValidMixedValidatableCommandThenReturnsTrue()
        {
            // Assign
            ICommandValidator validator = new DefaultCommandValidator();
            MixedValidatableCommand command = new MixedValidatableCommand(true);
            command.Property1 = "123456";

            // Act
            bool result = validator.Validate(command);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(0, command.ValidationResults.Count);
        }

        [TestMethod]
        public void WhenValidatingInvalidMixedValidatableCommandThenReturnsFalse()
        {
            // Assign
            ICommandValidator validator = new DefaultCommandValidator();
            MixedValidatableCommand command = new MixedValidatableCommand(false);
            command.Property1 = "123456789456132456";

            // Act
            bool result = validator.Validate(command);

            // Assert
            Assert.IsFalse(result);

            // Validator ignore IValidatableObject validation until DataAnnotations succeed.
            Assert.AreEqual(1, command.ValidationResults.Count);
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