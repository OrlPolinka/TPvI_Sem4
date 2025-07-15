using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using ANC25_WEBAPI_DLL; // Для CelebritiesConfig
using DAL_Celebrity_MSSQL; // Для Celebrity
using System.Text.Encodings.Web; // Для HtmlEncoder

namespace ASPA008_1.Helpers
{
    public static class CelebrityFotoHelper
    {
        public static IHtmlContent CelebrityFoto(this IHtmlHelper htmlHelper, Celebrity celebrity, int width = 200, int height = 200)
        {
            if (string.IsNullOrEmpty(celebrity.ReqPhotoPath))
            {
                var placeholderDiv = new TagBuilder("div");
                placeholderDiv.AddCssClass("celebrity-photo-placeholder");
                placeholderDiv.Attributes.Add("style", $"width:{width}px; height:{height}px; line-height:{height}px;");
                placeholderDiv.InnerHtml.Append("No Photo");
                using (var writer = new StringWriter())
                {
                    placeholderDiv.WriteTo(writer, HtmlEncoder.Default);
                    return new HtmlString(writer.ToString());
                }
            }

            var config = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IOptions<CelebritiesConfig>>().Value;
            var photoRequestPath = config.PhotosRequestPath;
            var imageUrl = $"{photoRequestPath}/{celebrity.ReqPhotoPath}";

            var imgTag = new TagBuilder("img");
            imgTag.Attributes.Add("src", imageUrl);
            imgTag.Attributes.Add("alt", $"Фото {celebrity.FullName}");
            imgTag.Attributes.Add("width", width.ToString());
            imgTag.Attributes.Add("height", height.ToString());
            imgTag.AddCssClass("celebrity-photo-img");

            var anchorTag = new TagBuilder("a");
            anchorTag.Attributes.Add("href", $"/{celebrity.Id}");
            anchorTag.AddCssClass("celebrity-photo-link");

            var wrapperDiv = new TagBuilder("div");
            wrapperDiv.AddCssClass("celebrity-photo-wrapper");
            wrapperDiv.InnerHtml.AppendHtml(imgTag);

            anchorTag.InnerHtml.AppendHtml(wrapperDiv);

            using (var writer = new StringWriter())
            {
                anchorTag.WriteTo(writer, HtmlEncoder.Default);
                return new HtmlString(writer.ToString());
            }
        }
    }
}