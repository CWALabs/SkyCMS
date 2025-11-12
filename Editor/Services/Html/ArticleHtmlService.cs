// <copyright file="ArticleHtmlService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Html
{
    using HtmlAgilityPack;
    using System;

    /// <summary>
    /// Concrete implementation of <see cref="IArticleHtmlService"/> using HtmlAgilityPack
    /// to safely parse and manipulate HTML fragments for editorial tooling and content analysis.
    /// </summary>
    public sealed class ArticleHtmlService : IArticleHtmlService
    {
        /// <inheritdoc />
        public string EnsureEditableMarkers(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return $"<div contenteditable='true' data-ccms-ceid='{Guid.NewGuid():N}'></div>";
            }

            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var editable = doc.DocumentNode.SelectNodes("//*[@contenteditable='true' or translate(@contenteditable,'TRUE','true')='true']")
                              ?? new HtmlNodeCollection(null);
                int i = 0;
                foreach (var node in editable)
                {
                    if (node.Attributes["data-ccms-ceid"] == null)
                        node.Attributes.Add("data-ccms-ceid", Guid.NewGuid().ToString("N"));
                    if (node.Attributes["data-ccms-index"] == null)
                        node.Attributes.Add("data-ccms-index", (i++).ToString());
                }

                if (editable.Count == 0)
                {
                    var wrapper = doc.CreateElement("div");
                    wrapper.SetAttributeValue("contenteditable", "true");
                    wrapper.SetAttributeValue("data-ccms-ceid", Guid.NewGuid().ToString("N"));
                    wrapper.InnerHtml = doc.DocumentNode.InnerHtml;
                    doc.DocumentNode.RemoveAllChildren();
                    doc.DocumentNode.AppendChild(wrapper);
                }

                return doc.DocumentNode.OuterHtml;
            }
            catch
            {
                return html;
            }
        }

        /// <inheritdoc />
        public string EnsureAngularBase(string headerFragment, string urlPath)
        {
            if (string.IsNullOrWhiteSpace(headerFragment))
                return string.Empty;

            var doc = new HtmlDocument();
            try { doc.LoadHtml(headerFragment); } catch { return headerFragment; }

            var meta = doc.DocumentNode.SelectSingleNode("//meta[@name='ccms:framework']");
            if (meta == null ||
                meta.Attributes["value"] == null ||
                !meta.Attributes["value"].Value.Equals("angular", System.StringComparison.OrdinalIgnoreCase))
                return headerFragment;

            var baseNode = doc.DocumentNode.SelectSingleNode("//base");
            var normalized = "/" + (urlPath ?? string.Empty).Trim('/').ToLowerInvariant() + "/";
            if (normalized == "//") normalized = "/";

            if (baseNode == null)
            {
                baseNode = doc.CreateElement("base");
                baseNode.SetAttributeValue("href", normalized);
                doc.DocumentNode.AppendChild(baseNode);
            }
            else
            {
                if (baseNode.Attributes["href"] == null)
                    baseNode.Attributes.Add("href", normalized);
                else
                    baseNode.Attributes["href"].Value = normalized;
            }

            return doc.DocumentNode.OuterHtml;
        }

        /// <inheritdoc />
        public string ExtractIntroduction(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var p = doc.DocumentNode.SelectSingleNode("//p[normalize-space()]");
                if (p == null)
                {
                    return string.Empty;
                }

                var text = System.Net.WebUtility.HtmlDecode(p.InnerText).Trim();
                return text.Length > 512 ? text[..512] : text;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}