// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// Suppress MSTEST0037: Use proper 'Assert' methods across the entire project
// Justification: We prefer explicit assertion methods like Assert.IsTrue/IsFalse for readability
// even when more specific assertions like Assert.Contains, Assert.IsEmpty, etc. are available.
[assembly: SuppressMessage("Usage", "MSTEST0037:Use proper 'Assert' methods", Justification = "Project prefers explicit assertions for clarity", Scope = "namespaceanddescendants", Target = "~N:Sky.Tests")]

// Suppress MSTEST0044: DataTestMethod is obsolete warnings
[assembly: SuppressMessage("Usage", "MSTEST0044:'DataTestMethod' is obsolete. Use 'TestMethod' instead.", Justification = "Keeping DataTestMethod for backward compatibility", Scope = "namespaceanddescendants", Target = "~N:Sky.Tests")]

// Suppress MSTEST0036: Member already exists in base class warnings
[assembly: SuppressMessage("Usage", "MSTEST0036:Member already exists in the base class", Justification = "Intentional override of Setup method for test initialization", Scope = "namespaceanddescendants", Target = "~N:Sky.Tests")]

// Suppress MSTEST0032: Review or remove assertion warnings
[assembly: SuppressMessage("Usage", "MSTEST0032:Review or remove the assertion as its condition is known to be always true", Justification = "Assertions kept for documentation and safety", Scope = "namespaceanddescendants", Target = "~N:Sky.Tests")]

// Suppress MSTEST0052: Avoid passing explicit DynamicDataSourceType
[assembly: SuppressMessage("Usage", "MSTEST0052:Remove the 'DynamicDataSourceType' argument to use the default auto detect behavior", Justification = "Explicit type needed for legacy compatibility", Scope = "namespaceanddescendants", Target = "~N:Sky.Tests")]