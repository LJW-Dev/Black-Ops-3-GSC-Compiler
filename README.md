# Black Ops 3 GSC Compiler
A Black Ops 3 GSC Compiler and Injector for both PC and Xbox.

# Features
Animation support. \
Single file or folder compile options. \
Function hash output for debugging the "\**** X Script Error(s)" error. \
Can play online with other people. \ 
Ability to change the header used to inject PC scripts with for multi-script injection. \
Simple PC Injector included so compiled scripts are simple to share and easy for non-developers to inject.

# Buttons Explanation
- Folder Compile: Recursively compiles folders with .gsc files in them (useful for InfinityLoader project migration) and requires a main.gsc as the starting file. If it is not ticked it will compile a single .gsc or .txt source file. \

- Output function Hashes: This will output a .txt in the CompiledScripts directory that lists all the functions used and their hashes. It can be used to debugging the "\**** X Script Error(s)" error and can be added to BlackOps3.txt when using cerberus to name every function. \

- Debug Output: Outputs information about the inection process, useful if the injection isn't working correctly (PC only). \

- Use Different GSC Header: This will change the GSC file being overwritten to a custom one. This can be used to have multiple custom GSC files loaded ingame (PC only). \

# Xbox 360 Use
Requires my injector DLL to be used: https://github.com/LJW-Dev/Black-Ops-3-GSC-Injector-for-Xbox-360.
If anyone knows how to code PS3 mods feel free to port the injector.
