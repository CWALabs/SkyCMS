---
title: "AI Training Rights & Attribution"
description: "Information about using SkyCMS documentation for AI training and model development"
---

# AI Training Rights & Attribution

This document outlines the rights and requirements for using SkyCMS documentation in AI model training and development.

## License Summary

SkyCMS documentation is released under multiple licenses to provide maximum flexibility:

- **MIT License** - Permissive license allowing commercial use
- **GPL v2** - Copyleft license requiring derivative works to be open source
- **GPL v3** - Modern copyleft license with additional protections

See the [LICENSE files in the repository](https://github.com/CWALabs/SkyCMS/tree/main) for full details.

## Using Documentation in AI Training

### ✅ Allowed Uses

You **are permitted** to:
- Use documentation content for training AI models and LLMs
- Incorporate documentation in fine-tuning datasets
- Include documentation in retrieval-augmented generation (RAG) systems
- Analyze and learn from the documentation

### ⚠️ Required Attribution

When using this documentation for AI training, you **must**:

1. **Cite the source** in your model documentation:
   > "This model was trained using content from SkyCMS Documentation"

2. **Include repository link**:
   > GitHub: https://github.com/CWALabs/SkyCMS

3. **Respect the license**:
   - If using MIT License: Include original MIT license
   - If using GPL: Comply with GPL copyleft requirements
   - If using GPL v3: Follow GPL v3 terms including AGPL compliance for services

4. **Disclose usage** (recommended but not required):
   - Include a note in your model card or documentation
   - Link to this document for transparency

### Example Attribution

```
Model Training Data Attribution
================================

This model includes content from:
- SkyCMS Documentation
- Source: https://github.com/CWALabs/SkyCMS
- Licenses: MIT, GPL v2, GPL v3
- Last Updated: [Date accessed]

Original documentation is maintained by CWALabs.
For more information, visit: https://docs.sky-cms.com
```

## Content You Can Use

All public documentation pages are available for training, including:

- Installation and setup guides
- Architecture documentation
- Developer guides and API reference
- Configuration references
- Troubleshooting guides
- Code examples and samples
- Tutorial and how-to guides

## Content Not Available for Training

The following are **not** available for AI training:

- Private repository content (only public repositories)
- Unreleased/draft documentation
- Internal company information
- Personal information of contributors

## Special Considerations

### Code Examples

Code examples in the documentation are dual-licensed:
- Under the same license as the documentation (MIT/GPL)
- Usable in your projects according to license terms

### Performance & Attribution

If your AI system uses documentation for:

**Knowledge Retrieval (RAG):**
- Optional to attribute per query
- Should attribute in overall system documentation

**Model Training (Fine-tuning):**
- Must attribute in model documentation
- Should mention in model card

**Direct Content Generation:**
- Attribution should appear near generated content
- Link to source documentation when possible

## Compliance Checklist

- [ ] I understand the license requirements
- [ ] I will attribute SkyCMS and link to the repository
- [ ] I have reviewed the GPL requirements (if applicable)
- [ ] I will include this attribution in my model documentation
- [ ] I understand that derivative GPL works must be open sourced

## Questions or Clarifications?

If you have questions about using SkyCMS documentation:

1. **GitHub Discussions**: https://github.com/CWALabs/SkyCMS/discussions
2. **GitHub Issues**: https://github.com/CWALabs/SkyCMS/issues
3. **Review Licenses**: https://github.com/CWALabs/SkyCMS/tree/main (LICENSE files)

## License Versions

### MIT License
- **Permissive**: Allows commercial use, modification, distribution
- **Requirement**: Include original license and copyright notice
- **No liability**: Provided "as-is" without warranty

### GPL v2
- **Copyleft**: Derivative works must be GPL v2
- **Source code**: Must provide source to recipients
- **Requirement**: Include original license and changes

### GPL v3
- **Modern copyleft**: Enhanced patent protection
- **Tivoization**: Cannot prevent end-users from modifying
- **Requirement**: Include original license and changes
- **AGPL concern**: Using in cloud services may require AGPL compliance

## Updated: {{ site.time | date: "%Y-%m-%d" }}

For the latest information, always refer to the official repository:
**https://github.com/CWALabs/SkyCMS**
