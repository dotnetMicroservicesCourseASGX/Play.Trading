# Play.Trading.Contracts
Trading libraries used by Play Economy services

## Build the docker image
```powershell
$version="1.0.4"
$env:GH_OWNER="dotnetMicroservicesCourseASGX"
$env:GH_PAT="[PAT HERE]"
$appname="playeconomyaxsg1"
docker build --secret id=GH_OWNER --secret id=GH_PAT -t "$appname.azurecr.io/play.trading:$version" .
```

## Run the docker image
```powershell
$cosmosDbConnString="[CONN STRING HERE]"
$serviceBusConnString="[CONN STRING HERE]"

docker run -it --rm -p 5006:5006 --name trading -e MongoDbSettings__ConnectionString=$cosmosDbConnString -e ServiceBusSettings__ConnectionString=$serviceBusConnString -e ServiceSettings__MessageBroker="SERVICEBUS" play.trading:$version
```

## Publish the docker image
```powershell
az acr login --name $appname
docker push "$appname.azurecr.io/play.trading:$version"
```

## Create the kubernetes namespace
```powershell
$namespace="trading"
kubectl create namespace $namespace
```

## Creating the Azure Managed Identity and grantinng access to key vault secrets
```powershell 
az identity create --resource-group $appname --name $namespace

$IDENTITY_CLIENT_ID=az identity show -g $appname -n $namespace --query clientId -otsv
az keyvault set-policy -n $appname --secret-permissions get list --spn $IDENTITY_CLIENT_ID

``` 

## Establish the federeated identity credential
```powershell
$AKS_OIDC_ISSUER=az aks show -g $appname -n $appname --query "oidcIssuerProfile.issuerUrl" -otsv

az identity federated-credential create --name $namespace --identity-name $namespace --resource-group $appname --issuer $AKS_OIDC_ISSUER --subject "system:serviceaccount:${namespace}:${namespace}-serviceaccount" 
```

## Create the kubernetes pod
```powershell
kubectl apply -f .\kubernetes\trading.yaml --namespace $namespace
```