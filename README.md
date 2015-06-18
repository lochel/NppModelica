NppModelica
===========

NppModelica is a plugin for Notepad++ that provides an outline and call graph for MetaModelica source files.

## Download binaries
You will find the [latest release](../../releases/latest) here.

* [v1.0.4](../../releases/tag/v1.0.4)
  * [improved] Make outline and project explorer usable via keyboard
  * [fixed] Add horizontal scrollbar to project explorer
* [v1.0.3](../../releases/tag/v1.0.3)
  * [fixed] Fixed outline for protected types
* [v1.0.2](../../releases/tag/v1.0.2)
  * [fixed] Outline is fixed on start-up
  * [improved] Self-updater installs now the latest instead of the next version
  * [improved] Bug fixes
* [v1.0.1](../../releases/tag/v1.0.1)
  * [new] Add support for interface packages
  * [improved] Generate outline recursive, since packages can contain packages
* [v1.0.0](../../releases/tag/v1.0.0)
  * [new] Support for self-updating: If a new version of NppModelica is available, it can be updated with a single click.
  * [improved] Local file system explorer to easily navigate current project
  * [improved] Graphical user interface
* [v0.2.4](../../releases/tag/v0.2.4)
  * [improved] Bug fixes
* [v0.2.3](../../releases/tag/v0.2.3)
  * [new] Support for Modelica Text Template Language Susan (outline and call graph)
  * [new] Local file system explorer to easily navigate current project
  * [improved] Performance improvements
  * [improved] Various bug fixes
* [v0.2.2](../../releases/tag/v0.2.2)
* [v0.2.1](../../releases/tag/v0.2.1)

## Setup
* Start Notepad++ and click "Settings" -> "Import" -> "Import plugin(s)…".
* The call graph feature depends on [Graphviz](http://graphviz.org/). Therefore, it is necessary to specify Graphviz root folder via "SETTINGS" -> "Graphviz" within the NppModelica menu strip.

## Issues
You may report any issues with using the [Issues](../../issues) button.
