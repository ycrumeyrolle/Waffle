namespace CommandProcessing.Validation
{
    public class ModelValidationResult
    {
        private string memberName;

        private string message;

        public string MemberName
        {
            get { return this.memberName ?? string.Empty; }
            set { this.memberName = value; }
        }

        public string Message
        {
            get { return this.message ?? string.Empty; }
            set { this.message = value; }
        }
    }
}
