namespace Waffle.Validation
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    ///  Represents a container for the results of a validation request.
    /// </summary>
    public class ModelValidationResult
    {
        private string memberName;

        private string message;

        /// <summary>
        /// Gets the member name that indicate which field have validation errors.
        /// </summary>
        /// <value>
        /// The member name that indicate which field have validation errors.
        /// </value>
        public string MemberName
        {
            get { return this.memberName ?? string.Empty; }
            set { this.memberName = value; }
        }

        /// <summary>
        ///   Gets the error message for the validation.
        /// </summary>
        /// <value>
        /// The error message for the validation.
        /// </value>
        public string Message
        {
            get { return this.message ?? string.Empty; }
            set { this.message = value; }
        }
    }
}
