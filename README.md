# UABEA-batch-texture-sprite
Uses UABEA to batch add textures and sprites to unity bundle files

Console app, use "ConsoleApp1.exe" "filetargetto" "filetargetfrom" "optionalinttocontroltexturefunction"

filetargetto

 -- string, filename of unity bundle, will default to "__data" if one argument is given only
 
filetargetfrom

 -- string, filename of unity bundle, sprites/textures from this bundle will be added to filetargetto
 
optionalinttocontroltexturefunction
 
 -- int, optional, will default to 0 if no argument is given.
 
 -- 2 == textures will be automatically exported and imported
 
 -- 1 == textures will be automatically imported, must be supplied manually in the form of .png files
 
 -- 0 == textures will not be imported (useful for adding later manually in uabea)

exporting/importing textures uses .png files in the same directory, works with files with the same name as the asset.

provided are .bat files as well, drag a filetargetfrom onto them.

Example: have a __data file in the directory, then drag your new asset bundle onto one of the .bat files.
