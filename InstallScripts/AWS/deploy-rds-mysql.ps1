Param(
  [string] $DBInstanceIdentifier = "skycms-mysql",
  [string] $DBName = "skycms",
  [string] $MasterUsername = "skycms_admin",
  [string] $MasterUserPassword,             # If omitted, will prompt securely
  [string] $Region = "us-east-1",
  [int]    $AllocatedStorage = 20,
  [string] $DBInstanceClass = "db.t4g.micro",
  [string] $EngineVersion = "8.0",
  [switch] $PubliclyAccessible,             # default: false
  [string] $AllowCidr,                      # default: caller public IP /32 if PubliclyAccessible
  [string] $VpcId,                          # default: account default VPC
  [string] $SubnetGroupName = "skycms-mysql-subnets",
  [string] $SecurityGroupName = "skycms-mysql-sg"
)

# Requires AWS CLI v2
$awsCmd = Get-Command aws -ErrorAction SilentlyContinue
if (-not $awsCmd) {
  Write-Error "AWS CLI not found. Install AWS CLI v2: https://aws.amazon.com/cli/"
  exit 1
}

# Prompt password if not provided
if (-not $MasterUserPassword) {
  $sec = Read-Host -AsSecureString -Prompt "Enter Master password for user '$MasterUsername'"
  $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($sec)
  try { $MasterUserPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto($bstr) } finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr) }
}

# Resolve default VPC if not provided
if (-not $VpcId) {
  $VpcId = aws ec2 describe-vpcs --region $Region --filters Name=isDefault,Values=true --query 'Vpcs[0].VpcId' --output text
  if ($LASTEXITCODE -ne 0 -or $VpcId -eq 'None') { Write-Error "Could not find default VPC. Provide -VpcId and two subnet IDs manually."; exit 1 }
}

# Pick two subnets in the VPC (prefer different AZs)
$subnets = aws ec2 describe-subnets --region $Region --filters Name=vpc-id,Values=$VpcId --query 'Subnets[].{Id:SubnetId,Az:AvailabilityZone}' --output json | ConvertFrom-Json
if (-not $subnets -or $subnets.Count -lt 2) { Write-Error "Need at least two subnets in VPC $VpcId."; exit 1 }
# Choose first two distinct-AZ subnets if possible
$chosen = @()
foreach ($sn in $subnets | Sort-Object Az) { if ($chosen.Az -notcontains $sn.Az) { $chosen += $sn }; if ($chosen.Count -eq 2) { break } }
if ($chosen.Count -lt 2) { $chosen = $subnets | Select-Object -First 2 }
$subnetIds = $chosen | ForEach-Object { $_.Id }

# Create or reuse DB subnet group
$existingSng = aws rds describe-db-subnet-groups --region $Region --db-subnet-group-name $SubnetGroupName 2>$null
if ($LASTEXITCODE -ne 0) {
  Write-Host "Creating DB subnet group '$SubnetGroupName'..."
  aws rds create-db-subnet-group `
    --region $Region `
    --db-subnet-group-name $SubnetGroupName `
    --db-subnet-group-description "SkyCMS MySQL subnets" `
    --subnet-ids $subnetIds | Out-Null
  if ($LASTEXITCODE -ne 0) { Write-Error "Failed to create subnet group."; exit 1 }
} else {
  Write-Host "Using existing DB subnet group '$SubnetGroupName'."
}

# Create or reuse security group
$sgId = aws ec2 describe-security-groups --region $Region --filters Name=vpc-id,Values=$VpcId Name=group-name,Values=$SecurityGroupName --query 'SecurityGroups[0].GroupId' --output text
if ($sgId -eq 'None') {
  Write-Host "Creating security group '$SecurityGroupName'..."
  $sgId = aws ec2 create-security-group --region $Region --group-name $SecurityGroupName --description "SkyCMS MySQL access" --vpc-id $VpcId --query GroupId --output text
  if ($LASTEXITCODE -ne 0 -or -not $sgId) { Write-Error "Failed to create security group."; exit 1 }
} else {
  Write-Host "Using existing security group '$SecurityGroupName' ($sgId)."
}

# If publicly accessible, open 3306 from caller IP (or provided CIDR)
$pub = $PubliclyAccessible.IsPresent
if ($pub) {
  if (-not $AllowCidr) {
    try { $ip = (Invoke-RestMethod -Uri https://checkip.amazonaws.com/ -UseBasicParsing).Trim() } catch { Write-Error "Failed to resolve public IP. Provide -AllowCidr manually (e.g., 1.2.3.4/32)."; exit 1 }
    $AllowCidr = "$ip/32"
  }
  Write-Host "Authorizing ingress to 3306 from $AllowCidr ..."
  aws ec2 authorize-security-group-ingress --region $Region --group-id $sgId --ip-permissions IpProtocol=tcp,FromPort=3306,ToPort=3306,IpRanges="CidrIp=$AllowCidr,Description=SkyCMS"
  # ignore errors if rule already exists
}

# Check if DB exists
$exists = aws rds describe-db-instances --region $Region --db-instance-identifier $DBInstanceIdentifier 2>$null
if ($LASTEXITCODE -eq 0) {
  Write-Host "DB instance '$DBInstanceIdentifier' already exists. Skipping creation."
} else {
  Write-Host "Creating RDS MySQL instance '$DBInstanceIdentifier'... (this can take ~10-15 minutes)"
  aws rds create-db-instance `
    --region $Region `
    --db-instance-identifier $DBInstanceIdentifier `
    --db-name $DBName `
    --allocated-storage $AllocatedStorage `
    --db-instance-class $DBInstanceClass `
    --engine mysql `
    --engine-version $EngineVersion `
    --master-username $MasterUsername `
    --master-user-password $MasterUserPassword `
    --vpc-security-group-ids $sgId `
    --db-subnet-group-name $SubnetGroupName `
    --multi-az `
    --no-multi-az `
    --publicly-accessible:$pub | Out-Null
  if ($LASTEXITCODE -ne 0) { Write-Error "Failed to create DB instance."; exit 1 }
}

Write-Host "Waiting for DB to be available..."
aws rds wait db-instance-available --region $Region --db-instance-identifier $DBInstanceIdentifier
if ($LASTEXITCODE -ne 0) { Write-Error "DB did not become available."; exit 1 }

# Fetch connection info
$desc = aws rds describe-db-instances --region $Region --db-instance-identifier $DBInstanceIdentifier --query 'DBInstances[0].{Endpoint:Endpoint.Address,Port:Endpoint.Port,Arn:DBInstanceArn}' --output json | ConvertFrom-Json

$result = [PSCustomObject]@{
  Engine              = "mysql"
  Region              = $Region
  DBInstanceId        = $DBInstanceIdentifier
  DBName              = $DBName
  Host                = $desc.Endpoint
  Port                = $desc.Port
  MasterUsername      = $MasterUsername
  MasterUserPassword  = "(hidden)"
  SecurityGroupId     = $sgId
  SubnetGroupName     = $SubnetGroupName
  PubliclyAccessible  = $pub
  ConnectionString    = "Server=$($desc.Endpoint);Port=$($desc.Port);Database=$DBName;Uid=$MasterUsername;Pwd=<your-password>;SslMode=Preferred;"
}

Write-Host "\n=== SkyCMS MySQL Connection Details ==="
$result | Format-List | Out-String | Write-Host
Write-Host "Save your password securely. You can rotate later with: aws rds modify-db-instance --master-user-password ..."
