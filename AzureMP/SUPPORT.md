# SkyCMS Support

## Support Channels

### Community Support (Free)

**GitHub Discussions**
- Best for: General questions, feature requests, community help
- Response time: Community-driven (typically 24-48 hours)
- Link: [GitHub Discussions](https://github.com/CWALabs/SkyCMS/discussions)

**GitHub Issues**
- Best for: Bug reports, technical issues
- Response time: 1-5 business days
- Link: [GitHub Issues](https://github.com/CWALabs/SkyCMS/issues)

**Documentation**
- Comprehensive guides: [https://docs-sky-cms.com](https://docs-sky-cms.com)
- Quick Start: [QuickStart.md](./Docs/QuickStart.md)
- FAQs: [FAQ.md](./Docs/FAQ.md)

**Community Resources**
- Slack Channel: [sky-cms.slack.com](https://sky-cms.slack.com) *(coming soon)*
- YouTube Tutorials: [@Sky-cms](https://www.youtube.com/@Sky-cms) *(coming soon)*

### Commercial Support

For enterprises requiring guaranteed response times and dedicated support:

**Email Support**
- Contact: support@moonrise.net
- Available for: Azure Marketplace customers and commercial license holders
- Response time: 1 business day for standard, 4 hours for priority

**Professional Services**
- Custom development and integration
- Migration assistance from other CMS platforms
- Performance optimization and scaling
- Azure architecture consulting
- Training and onboarding

Contact: sales@moonrise.net for pricing and availability.

## What to Include in Support Requests

To help us assist you quickly, please include:

### For Bug Reports

1. **SkyCMS version** (check About page or docker image tag)
2. **Deployment environment** (Azure App Service, Container Apps, AWS ECS, etc.)
3. **Database type and version** (Cosmos DB, MySQL, SQL Server, SQLite)
4. **Steps to reproduce** the issue
5. **Expected behavior** vs. **actual behavior**
6. **Error messages** (from browser console and server logs)
7. **Screenshots** (if UI-related)

### For Configuration Issues

1. **Deployment method** (Azure Portal, ARM template, Docker, etc.)
2. **Configuration files** (redact secrets!)
3. **Environment variables** (redact sensitive values)
4. **Error logs** from Azure Application Insights or container logs
5. **What you've already tried**

### For Performance Issues

1. **Current load** (page views/month, concurrent users)
2. **Azure resources** (SKU sizes, scaling configuration)
3. **Performance metrics** (page load times, TTFB, etc.)
4. **Caching configuration** (CDN, Redis, etc.)

## Self-Service Resources

### Documentation

| Topic | Link |
|-------|------|
| Installation | [Installation Guides](./Docs/Installation/) |
| Azure Deployment | [Azure README](./InstallScripts/Azure/README.md) |
| AWS Deployment | [AWS README](./InstallScripts/AWS/README.md) |
| Configuration | [Configuration Guides](./Docs/Configuration/) |
| Troubleshooting | [Troubleshooting Guide](./Docs/Troubleshooting.md) |
| Developer Docs | [Developer Guides](./Docs/Developers/) |

### Common Issues

**Setup wizard won't load**
- Ensure `CosmosAllowSetup=true` in environment variables
- Check database connection string is valid
- Verify container/app has started (wait 2-3 minutes)

**Database connection fails**
- Verify connection string format matches your database type
- Check firewall rules allow your IP or Azure services
- For managed identity, confirm RBAC permissions are assigned

**Static files not publishing**
- Verify storage connection string is correct
- Check storage container exists and has proper permissions
- Review publisher logs for error messages

**Performance is slow**
- Enable CDN for static content delivery
- Configure distributed caching (Redis or Azure Cache)
- Scale up App Service or Container Apps SKU
- Review Application Insights for bottlenecks

See [Troubleshooting.md](./Docs/Troubleshooting.md) for more solutions.

## Service Level Expectations

### Open Source (Community Support)

- **Availability**: Best effort, no SLA
- **Response time**: Community-driven
- **Updates**: Released when ready
- **Security patches**: Released ASAP for critical issues

### Commercial Support Tiers

**Standard Support** (Azure Marketplace customers)
- Business hours: Monday-Friday, 9am-5pm US Central Time
- Response time: 1 business day
- Channels: Email, GitHub
- Included: Bug fixes, configuration assistance

**Priority Support** (Enterprise customers)
- Coverage: 24/7 for critical issues
- Response time: 4 hours for priority, 1 business day for standard
- Channels: Email, phone, Slack
- Included: Everything in Standard + proactive monitoring suggestions

**Premium Support** (Custom SLA)
- Coverage: Custom (24/7 or business hours)
- Response time: Custom (as low as 1 hour)
- Channels: Dedicated Slack channel, phone, email
- Included: Dedicated support engineer, architecture reviews, custom development

Contact sales@moonrise.net for commercial support pricing.

## Contributing

SkyCMS is open source! If you've solved an issue or built a feature, please consider contributing:

1. Check [CONTRIBUTING.md](./Docs/CONTRIBUTING.md) for guidelines
2. Fork the repository
3. Create a feature branch
4. Submit a pull request

Community contributions help everyone!

## Feedback

We value your feedback to improve SkyCMS:

- **Feature requests**: [GitHub Discussions](https://github.com/CWALabs/SkyCMS/discussions)
- **Documentation improvements**: Submit PR or create issue
- **General feedback**: support@moonrise.net

---

**Last Updated**: December 2025
