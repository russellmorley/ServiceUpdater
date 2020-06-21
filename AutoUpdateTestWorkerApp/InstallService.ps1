# more info on running powershell scripts https://www.windowscentral.com/how-create-and-run-your-first-powershell-script-file-windows-10
# more info on service users https://social.msdn.microsoft.com/forums/sqlserver/en-US/31d57870-1faa-4e14-8527-ce77b1ff40e4/local-service-local-system-or-network-service
# more info on service user account: https://docs.microsoft.com/en-us/windows/win32/services/networkservice-account

$appdir = "C:\Users\rm\source\repos\AutoUpdateTestWorkerApp\publish-win"
$appfullpath ="C:\Users\rm\source\repos\AutoUpdateTestWorkerApp\publish-win\AutoUpdateTestWorkerApp.exe"
$description = "AutoUpdateTestWorkerApp is a wonderful app"
$displayname = "Auto Update Test Worker App"

$servicename = "AutoUpdateTestWorkerApp"
$user = "NT AUTHORITY\SYSTEM"
$acl = Get-Acl $appdir
$aclRuleArgs =  $user, "Read,Write,ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule($aclRuleArgs)
$acl.SetAccessRule($accessRule)
$acl | Set-Acl $appfullpath

New-Service -Name $servicename -BinaryPathName $appfullpath -Credential $user -Description $description -DisplayName $displayname -StartupType Automatic

#just cancel login

# sc.exe delete $servicename
# sc.exe stop $servicename

