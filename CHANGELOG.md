## [Unreleased]

### Deprecated
- `ArticleEditLogic.SaveArticle()` is now obsolete. Use `SaveArticleHandler` via `IMediator` pattern instead.
  - All production controllers have been migrated to the new pattern
  - Legacy method will remain available until v3.0 for backward compatibility
  - See `docs/MIGRATION-SAVE-ARTICLE.md` for migration guide

### Added
- **Vertical Slice Architecture** for article saving with CQRS pattern
  - `SaveArticleCommand` - Command object for save operations
  - `SaveArticleValidator` - Dedicated validation logic
  - `SaveArticleHandler` - Handler implementing save workflow
  - `IMediator` - Mediator pattern for command dispatching
- Comprehensive integration tests for `BlogController`
- Improved error handling with `CommandResult<T>` pattern

### Changed
- `EditorController` now uses mediator pattern for article saves
- `BlogController` now uses mediator pattern for blog operations
- Better separation of concerns in article editing workflow