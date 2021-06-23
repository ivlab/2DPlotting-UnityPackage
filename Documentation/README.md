# Auto-generating documentation for IVLab-Utilities-UnityPackage

[**View the IVLab-Template Documentation**](https://pages.github.umn.edu/ivlab-cs/IVLab-Template-UnityPackage/api/IVLab.Template.html)

Documentation is generated using
[DocFx](https://dotnet.github.io/docfx/index.html). There's a [handy
repo](https://github.com/NormandErwan/DocFxForUnity) for using this with Unity,
which we build on.

Documentation should be generated for each release. Commit the HTML files
in the /docs folder of this repo.


## Required components and installation

- DocFx is needed to generate the documentation. Download the latest stable
version from [DocFx releases](https://github.com/dotnet/docfx/releases).
- Unzip to somewhere useful to you, optionally somewhere on PATH.


## Project setup

You will need to modify the following files to point to your package name and namespaces contained within your package:
- `docfx.json`
- `filterConfig.yml`


## Generating docs

Run the following command from the root of this repo (tested with DocFX v2.57.2 on Windows):

Windows:

```
docfx.exe Documentation/docfx.json --serve
```

(You may need to replace `docfx.exe` with the absolute path `C:\Absolute\Path\To\docfx.exe`)

Then go to a browser at http://localhost:8080 to view the docs

**Note: After you generate, the docs, *before* you commit them, make sure to
enter the Unity editor at least once so that the corresponding .meta files are
generated and your end users don't end up with hundreds of errors about missing
.meta files!**

**Note 2:** DocFX sometimes generates a folder called "obj" in a random place within the `Runtime` folder of your package. Add that folder to your .gitignore.


## Deploying Docs

1. After building the docs and focusing the Unity editor, commit the generated changes
2. In your repo on github.umn.edu, go to *Settings > GitHub Pages > Source* and change it to "master branch /docs folder"
3. After a few minutes, visit https://pages.github.umn.edu/ivlab-cs/YourPackage-UnityPackage/api/IVLab.YourPackage.html