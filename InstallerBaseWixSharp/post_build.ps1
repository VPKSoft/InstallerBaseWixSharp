<#
MIT License

Copyright (c) 2020 Petteri Kautonen

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
#>

$version_value = "1.0.0"

$files = Get-ChildItem $Env:CIRCLE_WORKING_DIRECTORY -r -Filter *.msi
for ($i = 0; $i -lt $files.Count; $i++) 
{ 
    $file = $files[$i]

    $newName = -join($file.BaseName, $version_value, ".msi");

    Write-Output -join("Rename file: '", [System.IO.Path]::GetFileName($file.FullName), " --> '", $newName, "'...")

    Rename-Item -Path $file.FullName -NewName -join($file.BaseName, $version_value, ".msi");
}