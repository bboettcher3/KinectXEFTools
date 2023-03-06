dotnet publish XEFExtract.sln -c Debug -r win10-x64 || pause
"%SystemRoot%\explorer.exe" ".\bin\Debug\netcoreapp2.1\win10-x64"