using GovUkDesignSystem.Attributes.DataBinding;
using GovUkDesignSystem.GovUkDesignSystemComponents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GovUkDesignSystem.TagHelpers
{
    public abstract class BaseGovUkTagHelper : TagHelper
    {
        private readonly ICompositeViewEngine _compositeViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BaseGovUkTagHelper(ICompositeViewEngine compositeViewEngine, ITempDataProvider tempDataProvider, IHttpContextAccessor httpContextAccessor)
        {
            _compositeViewEngine = compositeViewEngine ?? throw new System.ArgumentNullException(nameof(compositeViewEngine));
            _tempDataProvider = tempDataProvider ?? throw new System.ArgumentNullException(nameof(tempDataProvider));
            _httpContextAccessor = httpContextAccessor ?? throw new System.ArgumentNullException(nameof(httpContextAccessor));
        }

        [ViewContext]
        public ViewContext ViewContext { get; set; }
        public string AspFor { get; set; }
        public string AspViewModel { get; set; }

        protected ErrorMessageViewModel ErrorMessageViewModel(string id)
        {
            ViewContext.ModelState.TryGetValue(id, out var modelStateEntry);
            if (modelStateEntry != null && modelStateEntry.Errors.Count > 0)
            {
                var errorMessages = modelStateEntry.Errors.Select(e => e.ErrorMessage).ToList();
                for (var i = 0; i < errorMessages.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(ErrorRequired)) { errorMessages[i] = Regex.Replace(errorMessages[i], "^The [a-zA-Z]+ is required$", ErrorRequired); }
                }
                return new ErrorMessageViewModel { Text = string.Join(", ", errorMessages) };
            }
            return null;
        }

        public string ErrorRequired { get; set; }

        protected string ErrorMessageIfMissing()
        {
            try
            {
                var modelType = string.IsNullOrEmpty(AspViewModel) ? ViewContext.ViewData.Model?.GetType() : Type.GetType(AspViewModel);
                if (modelType == null) { return null; }
                var property = modelType.GetProperty(AspFor);
                if (property == null) { return null; }
                var required = property.GetCustomAttributes(typeof(RequiredAttribute), false).SingleOrDefault() as RequiredAttribute;
                if (required != null) { return !string.IsNullOrWhiteSpace(ErrorRequired) ? ErrorRequired : required.ErrorMessage; }
                var mandatoryInt = property.GetCustomAttributes(typeof(GovUkDataBindingMandatoryIntErrorTextAttribute), false).SingleOrDefault() as GovUkDataBindingMandatoryIntErrorTextAttribute;
                if (mandatoryInt != null) { return !string.IsNullOrWhiteSpace(ErrorRequired) ? ErrorRequired : mandatoryInt.ErrorMessageIfMissing; }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        protected async Task<string> RenderPartialViewAsync<T>(string viewName, T model)
        {
            var viewResult = _compositeViewEngine.GetView(string.Empty, viewName, false);

            using var sw = new StringWriter();
            var viewContext = new ViewContext(
                new ActionContext(_httpContextAccessor.HttpContext, new RouteData(), new ActionDescriptor()),
                viewResult.View,
                new ViewDataDictionary<T>(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model },
                new TempDataDictionary(_httpContextAccessor.HttpContext, _tempDataProvider),
                sw,
                new HtmlHelperOptions());
            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }
    }
}
