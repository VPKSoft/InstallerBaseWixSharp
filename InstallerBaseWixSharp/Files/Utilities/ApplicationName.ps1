$files = Get-ChildItem -Recurse ..\..\* -Include "*.cs", "*.wxl" -Exclude "*.Designer.cs"

# English (United States);Finnish (Finland)
$languages = "0x00000102;0x00000118"; # see the Files\Localization\SupportedLanguages.cs file..

$locale_running_ids = $languages -split ";"

# replace the #APPLICATION# tag in the installer project with specified application name
foreach ($file in $files)
{
    Write-Output (-join("Change application name for the file: ", $file.FullName, "...")) 
    ((Get-Content -path $file.FullName -Raw) -replace '#APPLICATION#','amp#') | Set-Content -Path $file.FullName
    Write-Output (-join("File handled: ", $file.FullName, ".")) 
}

# Enabled the specified languages in the .\Files\Localization\SupportedLanguages.cs files..
$language_enumfile_contents = (Get-Content -path ..\..\Files\Localization\SupportedLanguages.cs -Raw)

# loop through the localization entries specified by the running id number..
foreach ($locale_running_id in $locale_running_ids) 
{    
    # set the specified language as enabled..
    $language_enumfile_contents = $language_enumfile_contents.Replace(-join($locale_running_id, ")]"), -join ($locale_running_id, ", true", ")]"))
}

# update the .\Files\Localization\SupportedLanguages.cs file contents..
Set-Content -path ..\..\Files\Localization\SupportedLanguages.cs -Value $language_enumfile_contents