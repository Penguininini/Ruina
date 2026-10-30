# RuinaModding

# Building the DLL

Download or clone the source code.

Open the .slnx solution file in Visual Studio.

Most required references should be found automatically, as they are included within the project's assemblies folder.

The Library of Ruina reference must be added manually.

Locate your Library of Ruina installation folder.

Navigate to LibraryOfRuina_Data/Managed.

Find Assembly-CSharp.dll.

Add it as a reference to the project through Visual Studio.

Once all references are resolved, build the solution using Build → Build Solution.

If the build succeeds, the compiled DLL will be available in the project's bin folder, either under Debug or Release depending on build configuration.
