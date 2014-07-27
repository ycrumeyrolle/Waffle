namespace Waffle.Tests.Metadata
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using Xunit;
    using Waffle.Metadata;
    
    public class DataAnnotationsModelMetadataProviderTests
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
        
        [Fact]
        public void ReadOnlyTests()
        {
            // Arrange
            var provider = new DataAnnotationsModelMetadataProvider();

            IModelFlattener test = new DefaultModelFlattener();
            var model = new DisplayModel();
            var result = test.Flatten(model, model.GetType(), provider, string.Empty);

            Assert.NotNull(result);
        }
    }
}
