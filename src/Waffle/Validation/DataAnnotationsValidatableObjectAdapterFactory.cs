namespace Waffle.Validation
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Provides a factory for validators that are based on <see cref="IValidatableObject"/>.
    /// </summary>
    /// <param name="validatorProviders">An enumeration of validator providers.</param>
    /// <returns>The <see cref="ModelValidator"/></returns>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Validatable", Justification = "False positive.")]
    public delegate ModelValidator DataAnnotationsValidatableObjectAdapterFactory(IEnumerable<ModelValidatorProvider> validatorProviders);
}