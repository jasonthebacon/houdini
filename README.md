# houdini

Tool to hide sensitive files/blatant rip-off of https://github.com/josephp27/GitCrypt

USE AT YOUR OWN RISK.


Prerequisites
-------------
* .NET Core 3.1 runtime (or SDK): https://dotnet.microsoft.com/download

Install
-------
1. Go to the releases page and download the ZIP file corresponding to your OS
2. Unzip the ZIP file to a suitable place (preferably your home directory).
3. From your command line, ***change directory into the unzipped files*** and run:
  * `.\Houdini.exe setup` on Windows
OR
  * `./Houdini setup` on *nix
Git aliases for Houdini can always be set up manually, however the `setup` command will guide you through the process.

Usage
-----
After the git aliases are set up, you can run
* `git initEncrypt` to create a `.houdini` file in the current directory, where you can specify files to be encrypted
* `git hide` to encrypt sensitive files specified in the current working directory's `.houdini` file
* `git reveal` to decrypt sensitive files specified in the current working directory's `.houdini` file

You can also pass the `help` command to the Houdini executable for brief help information. `git initEncrypt`, `git hide`, and `git reveal` are just aliases for the `initialize`, `encrypt`, and `decrypt` commands, respectively.

Uninstall
---------
Houdini does not deposit config files (or registry entries) automatically; it will create only `.houdini` files where and when you ask it to. So, "uninstallation" consists merely of unsetting any pertinent git aliases and deleting the Houdini binaries (i.e. the directory you unzipped in the "Install" step).

To unregister the git aliases, `cd` into the directory where Houdini is installed and run
  * `.\Houdini.exe unsetup` on Windows
OR
 * `./Houdini unsetup` on *nix

You can, of course, change or unset your git aliases manually.
