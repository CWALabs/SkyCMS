// <copyright file="IArticleHtmlService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Html
{
    using System;
    using HtmlAgilityPack;

    /// <summary>
    /// Provides operations for manipulating and analyzing article HTML fragments.
    /// Responsibilities include:
    /// - Ensuring editor (contenteditable) markers and stable identifiers exist.
    /// - Normalizing Angular application href values based on a URL path.
    /// - Extracting an introductory text snippet from article body HTML.
    /// </summary>
    public interface IArticleHtmlService
    {
        /// <summary>
        /// Ensures that all editable regions (contenteditable='true') contain a stable
        /// unique identifier and ordering metadata. If no editable region exists,
        /// the entire document is wrapped in a new editable container.
        /// </summary>
        /// <param name="html">Raw HTML fragment representing an article body.</param>
        /// <returns>HTML with required editing markers injected.</returns>
        string EnsureEditableMarkers(string html);

        /// <summary>
        /// For Angular-based articles (detected via meta name='ccms:framework' value='angular'),
        /// ensures a element exists (or is updated) with an href derived from the provided URL path.
        /// If the fragment is not Angular-marked, the original fragment is returned unchanged.
        /// </summary>
        /// <param name="headerFragment">HTML header fragment (head-level markup or similar).</param>
        /// <param name="urlPath">Logical URL path used to construct the base href.</param>
        /// <returns>Header fragment with a normalized element when applicable.</returns>
        string EnsureAngularBase(string headerFragment, string urlPath);

        /// <summary>
        /// Extracts the first non-empty paragraph's text content as an introduction,
        /// truncated to a maximum of 512 characters. Returns an empty string if none found or on parse failure.
        /// </summary>
        /// <param name="html">HTML fragment containing article content.</param>
        /// <returns>Introduction text snippet or empty string.</returns>
        string ExtractIntroduction(string html);
    }

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
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var p = doc.DocumentNode.SelectSingleNode("//p[normalize-space()]");
                if (p == null) return string.Empty;
                var text = p.InnerText.Trim();
                return text.Length > 512 ? text[..512] : text;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}