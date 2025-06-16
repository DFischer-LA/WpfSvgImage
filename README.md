# WpfSvgImage

**WpfSvgImage** is a lightweight .NET 8 library that provides a WPF `Image` control capable of displaying SVG images. It enables easy integration of SVG graphics into WPF applications, supporting loading from files, streams, and application resources.

## Features

- WPF `Image`-derived control for SVG rendering
- Load SVGs from file paths, streams, or resource URIs
- Dependency property support for XAML data binding
- Simple API for programmatic SVG image creation

## Installation

Using Nuget: 
Download and install the published nuget package through Visual Studio. https://www.nuget.org/packages/WpfSvgImage

From source:
Add the `WpfSvgImage` project to your solution or reference the compiled DLL in your WPF application targeting .NET 8 or later.

## Usage

<Window x:Class="MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:svg="clr-namespace:WpfSvgImage;assembly=WpfSvgImage"
        mc:Ignorable="d"
        Title="MainWindow" Height="600" Width="900">
    <Grid>
        <svg:SvgImage SvgSourceUri="pack://application:,,,/Resources/icon.svg" Width="150" Height="150" Stretch="Uniform" />
    </Grid>
</Window>

### XAML

