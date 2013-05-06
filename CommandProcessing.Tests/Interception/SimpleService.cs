namespace CommandProcessing.Tests.Interception
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    public class SimpleService
    {
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "For testing purpose.")]
        internal int ValueTypeValue = 123;

        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "For testing purpose.")]
        internal string StringValue = "test!";

        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "For testing purpose.")]
        internal StringSplitOptions EnumValue = StringSplitOptions.RemoveEmptyEntries;

        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "For testing purpose.")]
        internal Random ReferenceValue = new Random();
        
        public virtual void Parameterless()
        {
        }

        public void NonVirtual()
        {
        }

        public virtual void ValueTypeParameter(int value)
        {
            this.ValueTypeValue = value;
        }

        public virtual void ReferenceTypeParameter(Random value)
        {
            this.ReferenceValue = value;
        }

        public virtual void StringTypeParameter(string value)
        {
            this.StringValue = value;
        }

        public virtual void EnumTypeParameter(StringSplitOptions value)
        {
            this.EnumValue = value;
        }

        public virtual int ReturnsValueType()
        {
            return this.ValueTypeValue;
        }

        public virtual Random ReturnsReferenceType()
        {
            return this.ReferenceValue;
        }

        public virtual string ReturnsString()
        {
            return this.StringValue;
        }

        public virtual StringSplitOptions ReturnsEnum()
        {
            return this.EnumValue;
        }

        public virtual void ThrowsException()
        {
            throw new Exception();
        }
    }
}
