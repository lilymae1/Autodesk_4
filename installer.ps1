winget install Schniz.fnm
fnm install 22
node -v
npm -v #installed npm/node
npm i electron
npm i axios
npm i express fs path
npm i jest

ollama create revit/archiemodel -f electron-app\ollama-modelfile\Modelfile


$RelativePath = [Environment]::CurrentDirectory + "\revit-addin\bin\Debug\net48\RevitChatBotPrototype1.dll"
$content = "
<?xml version='1.0' encoding='utf-8' standalone='no'?>
<RevitAddIns>
  <AddIn Type='Command'>
    <Name>ChatBot Add-in</Name>
    <Assembly>'"+$RelativePath+"'</Assembly>
    <AddInId>12345678-1234-1234-1234-123456789abc</AddInId>
    <FullClassName>RevitChatBotPrototype1.ChatBotCommand</FullClassName>
    <VendorId>YOURID</VendorId>
    <VendorDescription>Your Company Name</VendorDescription>
  </AddIn>
</RevitAddIns>
"
$path = [Environment]::GetFolderPath('ApplicationData') + "\Autodesk\Revit\Addins\2025\"
New-Item -Path $path -Name "RevitChatBotPrototype1.Addin" -ItemType "File" -Value $content

Write-Output "Done!"