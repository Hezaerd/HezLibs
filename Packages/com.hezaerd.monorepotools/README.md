# Monorepo Tools

## Description

This package provides editor tools for managing a Unity monorepo and creating packages.

## Features

-   **Package Creation Wizard:** Simplifies the process of creating new Unity packages with pre-configured assembly definitions and package.json files.
-   **Monorepo Management:** (Future feature) Tools to help manage dependencies and links between packages within a monorepo.

## Installation

1.  Open the Package Manager in Unity (Window -> Package Manager).
2.  Click the "+" button and select "Add package from git URL...".
3.  Enter `https://github.com/Hezaerd/HezLibs.git?path=Packages/com.hezaerd.monorepotools`.

## Usage

### Package Creation

1.  Open the Package Creator window (Tools -> Package Creator).
2.  Enter the desired package information, such as the package name, display name, version, and description.
3.  Fill in the author information.
4.  Click the "Create Package" button.

This will create a new package in the `Packages` directory of your project, with the following structure:

```
com.yourname.newpackage/
├── Editor/
│   └── Editor.asmdef
├── Runtime/
│   └── Runtime.asmdef
├── Tests/
│   ├── Editor/
│   │   └── Editor.asmdef
│   └── Runtime/
│       └── Runtime.asmdef
└── package.json
```
