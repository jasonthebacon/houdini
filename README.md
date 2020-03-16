# houdini

Tool to hide sensitive files/blatant rip-off of https://github.com/josephp27/GitCrypt

USE AT YOUR OWN RISK. No, seriously, if you have to ask yourself whether or not you should use this tool, the answer is ~~probably~~certainly *no*. It was written on the quick for a particular use case, and might even be dangerous if you're not sure why you're here.


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

Configure
---------
The Houdini.ini file located next to the Houdini[.exe] binary configures the name of the environment variable containing the encryption key as well as the name of the file specifying encryption targets.
* The default Houdini.ini file comes set up for compatibility with [GitCrypt](https://github.com/josephp27/GitCrypt), i.e. it will use the `ENCRYPTION_TOOLS_KEY` environment variable and `.gitCrypt` files. 
* If you move/rename/delete Houdini.ini, or if you blank out the configuration properties, Houdini will default to using `HOUDINI_KEY` and `.houdini` files. 
* (There's a good chance these defaults will change soon, since most of the people looking at this page were probably looking at GitCrypt to start with.)

Usage
-----
After the git aliases are set up, you can run
* `git initEncrypt` to create a `.gitCrypt`/`.houdini` file in the current directory, where you can specify files to be encrypted
* `git hide` to encrypt sensitive files specified in the current working directory's `.gitCrypt`/`.houdini` file
* `git reveal` to decrypt sensitive files specified in the current working directory's `.gitCrypt`/`.houdini` file

You can also pass the `help` command to the Houdini executable for brief help information. `git initEncrypt`, `git hide`, and `git reveal` are just aliases for the `initialize`, `encrypt`, and `decrypt` commands, respectively.

Uninstall
---------
Houdini does not generate config files (or registry entries) automatically; it will create only `.gitCrypt`/`.houdini` files where and when you ask it to. So, "uninstallation" consists merely of unsetting any pertinent git aliases and deleting the Houdini binaries (i.e. the directory you unzipped in the "Install" step).

To unregister the git aliases, `cd` into the directory where Houdini is installed and run
  * `.\Houdini.exe unsetup` on Windows
OR
 * `./Houdini unsetup` on *nix

You can, of course, change or unset your git aliases manually.
