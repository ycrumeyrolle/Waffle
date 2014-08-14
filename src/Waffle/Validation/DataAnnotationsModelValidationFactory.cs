namespace Waffle.Validation
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents the method that creates a <see cref="DataAnnotationsModelValidatorProvider"/> instance.
    /// </summary>
    /// <param name="validatorProviders">An enumeration of validator providers.</param>
    /// <param name="attribute">The attribute.</param>
    /// <returns>The <see cref="ModelValidator"/></returns>
    public delegate ModelValidator DataAnnotationsModelValidationFactory(IEnumerable<ModelValidatorProvider> validatorProviders, ValidationAttribute attribute);
}