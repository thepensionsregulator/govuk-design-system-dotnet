using GovUkDesignSystem.GovUkDesignSystemComponents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GovUkDesignSystem.TagHelpers
{
    [HtmlTargetElement("govuk-textinput")]
    public class TextInputTagHelper : BaseGovUkTagHelper
    {
        public TextInputTagHelper(ICompositeViewEngine compositeViewEngine, ITempDataProvider tempDataProvider, IHttpContextAccessor httpContextAccessor) :
            base(compositeViewEngine, tempDataProvider, httpContextAccessor)
        { }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Hint { get; set; }
        public string Class { get; set; }

        public override async void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;
            ViewContext.ModelState.TryGetValue(Name, out var modelStateEntry);

            var textInput = new TextInputViewModel
            {
                Label = new LabelViewModel
                {
                    Text = Label
                },
                Hint = new HintViewModel
                {
                    Text = Hint
                    //Html = @< text > @content.Hint </ text >
                },
                Name = Name,
                Id = Id,
                Value = modelStateEntry?.AttemptedValue,
                Classes = Class,
                ErrorMessage = ErrorMessageViewModel(Id),
                ErrorMessageRequired = ErrorMessageIfMissing()
            };

            output.Content.SetHtmlContent(await RenderPartialViewAsync("/GovUkDesignSystemComponents/TextInput.cshtml", textInput));
        }
    }
}
