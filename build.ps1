
param(
    [string]$nuget_path= $("C:\nuget")
    )
    
	wget "https://raw.githubusercontent.com/rducom/ALM/master/build/ComputeVersion.ps1" -outfile "ComputeVersion.ps1"
    . .\ComputeVersion.ps1 
    
    $version = Compute Lucca.Logs\Lucca.Logs.csproj
    $props = "/p:Configuration=Debug,VersionPrefix="+($version.Prefix)+",VersionSuffix="+($version.Suffix)
    $propack = "/p:PackageVersion="+($version.Semver) 
 
    dotnet restore
    dotnet build Lucca.Logs.sln $props
    dotnet pack Lucca.Logs\Lucca.Logs.csproj  --configuration Debug $propack -o $nuget_path