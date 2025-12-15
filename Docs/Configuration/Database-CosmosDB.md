# Azure Cosmos DB with SkyCMS

Azure Cosmos DB is a fully managed, serverless NoSQL database ideal for global, low-latency applications. SkyCMS integrates seamlessly with Cosmos DB for storing content, users, and metadata.

## Values you need

- **Account Endpoint**: URI of your Cosmos DB account (e.g., `https://myaccount.documents.azure.com:443/`)
- **Account Key**: Primary or Secondary key for authentication
- **Database Name**: Name of the database within your Cosmos DB account

## Create a Cosmos DB account in Azure Portal

1. **Azure Portal** → **Create a resource** → search **"Cosmos DB"** → **Create**.
2. Choose **NoSQL** API.
3. Fill in:
   - **Account Name**: Globally unique name (becomes part of the URI)
   - **Location**: Choose a region close to your users
   - **Capacity mode**: **Serverless** (recommended for most use cases) or **Provisioned Throughput**
4. Click **Create** and wait for deployment.

## Get your credentials

1. Portal → **Cosmos DB account** → **Keys** (under Settings).
2. Copy:
   - **URI**: The account endpoint (e.g., `https://myaccount.documents.azure.com:443/`)
   - **PRIMARY KEY**: Your authentication key
   - **PRIMARY CONNECTION STRING**: Pre-formatted connection string (easier to copy)

3. Navigate to **Data Explorer** and note your **Database name**.

## Configure in SkyCMS

### Using the Setup Wizard (recommended)

1. Deploy SkyCMS with `CosmosAllowSetup=true`.
2. Open the Editor setup wizard.
3. When prompted for **Database**, paste the **PRIMARY CONNECTION STRING** from the Keys page:
   ```
   AccountEndpoint=https://myaccount.documents.azure.com:443/;AccountKey=your-key;Database=your-db;
   ```
4. Click **Validate** and proceed.

### Manual Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "ApplicationDbContextConnection": "AccountEndpoint=https://myaccount.documents.azure.com:443/;AccountKey=your-key;Database=your-db;"
  }
}
```

### Environment Variables

```powershell
$env:ConnectionStrings__ApplicationDbContextConnection = "AccountEndpoint=https://myaccount.documents.azure.com:443/;AccountKey=your-key;Database=your-db;"
```

## Best practices

- **Use managed identity** in production (instead of keys). Set `AccountKey=AccessToken` and assign the Cosmos DB Data Contributor role to your app's identity.
- **Secure your keys** in Azure Key Vault, not in code or appsettings.
- **Rotate keys** periodically.
- **Choose serverless** for variable workloads; **provisioned throughput** for predictable, sustained traffic.
- **Partition strategy**: Cosmos DB requires a partition key for collections. SkyCMS handles this automatically.

## Tips and troubleshooting

- Connection string must include the database name: `Database=your-db-name`
- If validation fails, verify the key and account endpoint are correct.
- Cosmos DB charges per RU (request unit) consumed; monitor usage in the Azure Portal.
