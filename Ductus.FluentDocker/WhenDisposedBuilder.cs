﻿using System;
using System.Linq;
using Ductus.FluentDocker.Extensions;
using Ductus.FluentDocker.Internal;

namespace Ductus.FluentDocker
{
  public class WhenDisposedBuilder
  {
    private readonly DockerBuilder _builder;
    private readonly DockerParams _prms;

    internal WhenDisposedBuilder(DockerBuilder builder, DockerParams prms)
    {
      _builder = builder;
      _prms = prms;
    }

    /// <summary>
    ///   When a exception or other occurs, i.e. client havent marked <see cref="DockerContainer.Success" />
    ///   and this option is selected, it will export the container to the host path.
    /// </summary>
    /// <param name="hostPath">Path to export the container to (template string allowed).</param>
    /// <param name="explode">If it shall explode it or not.</param>
    /// <returns>Itself for fluent access.</returns>
    /// <remarks>
    ///   If <paramref name="explode" /> is set to false, the tar file will be written in the temp folder
    ///   with a random name. If explode, it is possible to select any path (even if boot2docker and not
    ///   reachable from e.g. VirtualBox) since it will copy the file to temp folder and then untar the file
    ///   while exploding.
    /// </remarks>
    public WhenDisposedBuilder ExportOnError(string hostPath, bool explode = true)
    {
      _prms.ExportContainerHostPath = hostPath;
      _prms.ExportContainerHostPathExplode = explode;
      return this;
    }

    /// <summary>
    ///   Copies a file or directory from the container onto the host before it is stopped.
    /// </summary>
    /// <param name="name">The name of the copy instruction. Can later be used to a lookup of the path.</param>
    /// <param name="containerPath">The path on the container to copy, either single file or subdirectory.</param>
    /// <param name="hostPath">The host path. Template arguments are accepted.</param>
    /// <returns>Itself for fluent access.</returns>
    /// <remarks>
    ///   The <paramref name="name" /> is used to tag a copy instruction and the host path can later be resolved using
    ///   <see cref="DockerContainer.GetHostCopyPath" />.
    /// </remarks>
    public WhenDisposedBuilder CopyFromContainer(string containerPath, string hostPath, string name = null)
    {
      if (null == name)
      {
        name = Guid.NewGuid().ToString();
      }

      _prms.CopyFilesWhenDisposed.Add(new Tuple<string, string, string>(name, containerPath,
        hostPath.Render().ToPlatformPath()));
      return this;
    }

    public WhenDisposedBuilder RemoveVolume(params string[] directory)
    {
      if (null == directory || 0 == directory.Length)
      {
        _prms.VolumesToRemoveOnDispose = null;
        return this;
      }

      _prms.VolumesToRemoveOnDispose = directory.Select(dir => dir.Render()).ToArray();
      return this;
    }

    public WhenDisposedBuilder KeepContainer()
    {
      _prms.RemoveContainerOnDispose = true;
      return this;
    }

    public WhenDisposedBuilder KeepContainerRunning()
    {
      _prms.StopContainerOnDispose = false;
      _prms.RemoveContainerOnDispose = false;
      return this;
    }

    public DockerBuilder ConfigureContainer()
    {
      return _builder;
    }

    public DockerContainer Build(bool startImmediately = false)
    {
      return _builder.Build(startImmediately);
    }
  }
}