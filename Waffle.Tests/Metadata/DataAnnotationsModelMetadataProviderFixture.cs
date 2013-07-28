// -----------------------------------------------------------------------
// <copyright file="Da.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Waffle.Tests.Metadata
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Waffle.Metadata;

    [TestClass]
    public class DataAnnotationsModelMetadataProviderFixture
    {
        class ReadOnlyModel
        {
            public int NoAttributes { get; set; }

            [ReadOnly(true)]
            public int ReadOnlyAttribute { get; set; }

            [Editable(false)]
            public int EditableAttribute { get; set; }

            [ReadOnly(true)]
            [Editable(true)]
            public int BothAttributes { get; set; }

            // Editable trumps ReadOnly
        }

        class DisplayModel
        {
            public DisplayModel()
            {
                this.Test = new ReadOnlyModel();   
            }

            public bool NoAttribute { get; set; }
            
            [Display]
            public string DescriptionNotSet { get; set; }

            [Display(Description = "Description text")]
            public int DescriptionSet { get; set; }

            public ReadOnlyModel Test { get; set; }
        }
        
        [TestMethod]
        public void ReadOnlyTests()
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataProvider();

            IModelFlattener test = new DefaultModelFlattener();
            var model = new DisplayModel();
            var result = test.Flatten(model, model.GetType(), provider, string.Empty);

            Assert.IsNotNull(result);
        }
    }
}
