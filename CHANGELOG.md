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

## [9.2.3.11] - 2025-12-20

### Fixed
- Setup wizard HTTP 400s (antiforgery) caused by proxy scheme mismatch. CloudFront now forwards `Host`, `X-Forwarded-For`, and `X-Forwarded-Proto` to ALB via a custom Origin Request Policy so ASP.NET Core recognizes original HTTPS.

### Changed
- ECS containers temporarily run with `ASPNETCORE_ENVIRONMENT=Development` to surface detailed errors during debugging. Revert to `Production` after verification.
- Editor connection string is generated dynamically from the RDS endpoint and Secrets Manager credentials (removed hardcoded Azure connection string).

### Notes
- No stack teardown required; apply changes with a standard redeploy (`InstallScripts/AWS/cdk-deploy.ps1`).
- Documentation updated: see [InstallScripts/AWS/CDK_DEPLOYMENT_GUIDE.md](InstallScripts/AWS/CDK_DEPLOYMENT_GUIDE.md) and [InstallScripts/AWS/README.md](InstallScripts/AWS/README.md).
