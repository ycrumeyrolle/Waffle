namespace Waffle.Validation
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    internal class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1311:StaticReadonlyFieldsMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
        private static readonly ReferenceEqualityComparer DefaultInstance = new ReferenceEqualityComparer();

        private ReferenceEqualityComparer()
        {
        }

        public static ReferenceEqualityComparer Instance
        {
            get
            {
                return DefaultInstance;
            }
        }

        public new bool Equals(object x, object y)
        {
            return object.ReferenceEquals(x, y);
        }

        public int GetHashCode(object obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
