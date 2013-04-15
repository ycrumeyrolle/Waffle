// -----------------------------------------------------------------------
// <copyright file="Da.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace CommandProcessing.Tests.Metadata
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using CommandProcessing.Filters;
    using CommandProcessing.Metadata;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                Test = new ReadOnlyModel();   
            }
            public bool NoAttribute { get; set; }

            // Description

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
            HandlerContext context =new HandlerContext();
            var result = test.Flatten(model, model.GetType(), provider, string.Empty);

            Assert.IsNotNull(result );
        }

    }
}
