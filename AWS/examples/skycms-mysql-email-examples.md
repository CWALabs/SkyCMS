# SkyCMS Email Configuration Examples

This document shows how to configure different email providers with the SkyCMS CloudFormation template.

## SendGrid Configuration

```json
{
  "ParameterKey": "AdminEmailAddress",
  "ParameterValue": "admin@yourcompany.com"
},
{
  "ParameterKey": "SendGridApiKey",
  "ParameterValue": "SG.xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
},
{
  "ParameterKey": "SmtpHostName",
  "ParameterValue": ""
},
{
  "ParameterKey": "SmtpPort",
  "ParameterValue": ""
},
{
  "ParameterKey": "SmtpEnableSsl",
  "ParameterValue": ""
},
{
  "ParameterKey": "SmtpUserName",
  "ParameterValue": ""
},
{
  "ParameterKey": "SmtpPassword",
  "ParameterValue": ""
},
{
  "ParameterKey": "AzureCommunicationsConnectionString",
  "ParameterValue": ""
}
```

## SMTP Configuration (Gmail example)

```json
{
  "ParameterKey": "AdminEmailAddress",
  "ParameterValue": "admin@yourcompany.com"
},
{
  "ParameterKey": "SendGridApiKey",
  "ParameterValue": ""
},
{
  "ParameterKey": "SmtpHostName",
  "ParameterValue": "smtp.gmail.com"
},
{
  "ParameterKey": "SmtpPort",
  "ParameterValue": "587"
},
{
  "ParameterKey": "SmtpEnableSsl",
  "ParameterValue": "true"
},
{
  "ParameterKey": "SmtpUserName",
  "ParameterValue": "your-email@gmail.com"
},
{
  "ParameterKey": "SmtpPassword",
  "ParameterValue": "your-app-password"
},
{
  "ParameterKey": "AzureCommunicationsConnectionString",
  "ParameterValue": ""
}
```

## SMTP Configuration (Office 365 example)

```json
{
  "ParameterKey": "AdminEmailAddress",
  "ParameterValue": "admin@yourcompany.com"
},
{
  "ParameterKey": "SendGridApiKey",
  "ParameterValue": ""
},
{
  "ParameterKey": "SmtpHostName",
  "ParameterValue": "smtp.office365.com"
},
{
  "ParameterKey": "SmtpPort",
  "ParameterValue": "587"
},
{
  "ParameterKey": "SmtpEnableSsl",
  "ParameterValue": "true"
},
{
  "ParameterKey": "SmtpUserName",
  "ParameterValue": "your-email@yourcompany.com"
},
{
  "ParameterKey": "SmtpPassword",
  "ParameterValue": "your-password"
},
{
  "ParameterKey": "AzureCommunicationsConnectionString",
  "ParameterValue": ""
}
```

## Azure Communications Configuration

```json
{
  "ParameterKey": "AdminEmailAddress",
  "ParameterValue": "admin@yourcompany.com"
},
{
  "ParameterKey": "SendGridApiKey",
  "ParameterValue": ""
},
{
  "ParameterKey": "SmtpHostName",
  "ParameterValue": ""
},
{
  "ParameterKey": "SmtpPort",
  "ParameterValue": ""
},
{
  "ParameterKey": "SmtpEnableSsl",
  "ParameterValue": ""
},
{
  "ParameterKey": "SmtpUserName",
  "ParameterValue": ""
},
{
  "ParameterKey": "SmtpPassword",
  "ParameterValue": ""
},
{
  "ParameterKey": "AzureCommunicationsConnectionString",
  "ParameterValue": "endpoint=https://your-resource.communication.azure.com/;accesskey=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
}
```

## Notes

- **Choose only one email provider** - configure parameters for only one provider and leave others blank
- **SendGrid**: Requires API key from SendGrid dashboard
- **SMTP**: Most email providers support SMTP (Gmail, Office 365, Amazon SES, etc.)
- **Azure Communications**: Requires Azure Communications Service connection string
- **Security**: Store sensitive values (API keys, passwords) securely and never commit them to source control