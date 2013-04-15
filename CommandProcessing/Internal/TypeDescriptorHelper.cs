﻿namespace CommandProcessing.Internal
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    internal static class TypeDescriptorHelper
    {
        internal static ICustomTypeDescriptor Get(Type type)
        {
            return new AssociatedMetadataTypeTypeDescriptionProvider(type).GetTypeDescriptor(type);
        }
    }
}
